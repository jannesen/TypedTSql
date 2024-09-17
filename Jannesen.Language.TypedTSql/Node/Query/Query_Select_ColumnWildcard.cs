using System;
using System.Collections.Generic;
using System.Text;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnWildcard: Query_Select_Column
    {
        public      readonly        Query_SelectContext     n_SelectContext;
        public      readonly        Core.TokenWithSymbol    n_RowSetName;
        public      readonly        Core.TokenWithSymbol    n_Star;

        public                      DataModel.Column[]      ResultColumns        { get; private set; }

        public      static          bool                    CanParse(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(3);

            return (peek[0].ID == Core.TokenID.Star) ||
                   (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot && peek[2].ID == Core.TokenID.Star);
        }
        public                                              Query_Select_ColumnWildcard(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            n_SelectContext = selectContext;

            if (reader.CurrentToken.isToken(Core.TokenID.Star)) {
                n_Star = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Star);
            }
            else {
                n_RowSetName = ParseName(reader);
                ParseToken(reader, Core.TokenID.Dot);
                n_Star = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Star);
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            ResultColumns = null;

            var  sourceColumns = new List<DataModel.Column>();

            if (n_RowSetName != null) {
                var rowset     = context.FindRowSet(n_RowSetName.ValueString);

                if (rowset != null) {
                    n_RowSetName.SetSymbolUsage(rowset, DataModel.SymbolUsageFlags.Reference);

                    if ((rowset.Flags & DataModel.RowSetFlags.DynamicList) == 0) {
                        context.CaseWarning(n_RowSetName, rowset.Name);
                        sourceColumns.AddRange(rowset.GetColumns());
                    }
                    else
                        context.AddError(n_RowSetName, "Not allowed to use wildcard on a dynamic column list.");
                }
                else
                    context.AddError(n_RowSetName, "Unknown rowset alias [" + n_RowSetName.ValueString + "].");
            }
            else {
                foreach (var rowset in context.RowSets) {
                    if ((rowset.Flags & DataModel.RowSetFlags.DynamicList) == 0) {
                        sourceColumns.AddRange(rowset.GetColumns());
                    }
                    else {
                        context.AddError(this, "Not allowed to use wildcard on a dynamic column list [" + rowset.Name + "].");
                    }
                }
            }

            var target = context.Target;
            var symbolData = n_SelectContext != Query_SelectContext.TableSourceSubquery || target != null ? new List<DataModel.SymbolData>() : null;
            if (target != null) {
                var targetColumns = new List<DataModel.Column>();

                foreach (var column in sourceColumns) {
                    var targetColumn = target.GetColumnForAssign(column.Name,
                                                                 column.SqlType,
                                                                 column.CollationName,
                                                                 column.ValueFlags,
                                                                 null,
                                                                 null,
                                                                 out var declared);

                    if (targetColumn != null) {
                        targetColumns.Add(targetColumn);
                        symbolData?.Add(new DataModel.SymbolSourceTarget(new DataModel.SymbolUsage(column.Symbol,       DataModel.SymbolUsageFlags.Read),
                                                                         new DataModel.SymbolUsage(targetColumn.Symbol, declared ? DataModel.SymbolUsageFlags.Declaration | DataModel.SymbolUsageFlags.Write : DataModel.SymbolUsageFlags.Write)));
                    }
                    else {
                        context.AddError(this, "Unknown column [" + column.Name + "] in target.");
                    }
                }

                ResultColumns = targetColumns.ToArray();
            }
            else {
                if (symbolData != null) {
                    foreach(var column in sourceColumns) {
                        symbolData.Add(new DataModel.SymbolUsage(column.Symbol, DataModel.SymbolUsageFlags.Read));
                    }
                }
                ResultColumns = sourceColumns.ToArray();
            }

            if (symbolData != null) {
                n_Star.SetSymbolData(new DataModel.SymbolWildcard(symbolData.ToArray()));
            }
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (ResultColumns != null) {
                columns.AddRange(ResultColumns);
            }
        }

        public      override        void                    Emit(Core.EmitWriter emitWriter)
        {
            if (n_SelectContext == Query_SelectContext.StatementInsertTargetNamed ||
                n_SelectContext == Query_SelectContext.StatementInsertTargetVarVariable) {
                foreach (var child in Children) {
                    if (child is Core.Token token && token.isToken(Core.TokenID.Star)) {
                        var columnNames = new StringBuilder();

                        foreach (var column in ResultColumns) {
                            if (columnNames.Length > 0) {
                                columnNames.Append(',');
                            }
                            columnNames.Append(Library.SqlStatic.QuoteName(column.Name));
                        }

                        emitWriter.WriteText(columnNames.ToString());
                    }
                    else {
                        child.Emit(emitWriter);
                    }
                }
            }
            else {
                base.Emit(emitWriter);
            }
        }
    }
}
