using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnWildcard: Query_Select_Column
    {
        public      readonly        Core.TokenWithSymbol    n_RowSetName;
        public                      DataModel.RowSet        RowSet                  { get; private set; }

        public      static          bool                    CanParse(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(3);

            return (peek[0].ID == Core.TokenID.Star) ||
                   (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot && peek[2].ID == Core.TokenID.Star);
        }
        public                                              Query_Select_ColumnWildcard(Core.ParserReader reader)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.Star)) {
                ParseToken(reader, Core.TokenID.Star);
            }
            else {
                n_RowSetName = ParseName(reader);
                ParseToken(reader, Core.TokenID.Dot);
                ParseToken(reader, Core.TokenID.Star);
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            RowSet = null;

            if (n_RowSetName != null) {
                var rowset     = context.RowSets.FindRowSet(n_RowSetName.ValueString);

                if (rowset != null) {
                    n_RowSetName.SetSymbol(rowset);

                    if ((rowset.Columns.Flags & DataModel.ColumnListFlags.DynamicList) == 0) {
                        context.CaseWarning(n_RowSetName, rowset.Name);
                        RowSet = rowset;
                    }
                    else
                        context.AddError(n_RowSetName, "Not allowed to use wildcard on a dynamic column list [" + n_RowSetName.ValueString + "].");
                }
                else
                    context.AddError(n_RowSetName, "Unknown rowset alias [" + n_RowSetName.ValueString + "].");
            }
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (n_RowSetName != null) {
                if (RowSet != null) {
                    columns.AddRange(RowSet.Columns);

                    foreach (var c in RowSet.Columns)
                        c.SetUsed();
                }
            }
            else {
                foreach (var rowset in context.RowSets) {
                    if ((rowset.Columns.Flags & DataModel.ColumnListFlags.DynamicList) != 0)
                        context.AddError(this, "Not allowed to use wildcard on a dynamic column list [" + rowset.Name + "].");

                    columns.AddRange(rowset.Columns);

                    foreach (var c in rowset.Columns)
                        c.SetUsed();
                }
            }
        }
    }
}
