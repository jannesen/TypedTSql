using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms180169.aspx
    //  Statement_DECLARE_cursor ::=
    //      DECLARE cursor_name CURSOR
    //              [ LOCAL | GLOBAL ]
    //              [ FORWARD_ONLY | FORWARD_ONLY ]
    //              [ STATIC | KEYSET | DYNAMIC | FAST_FORWARD ]
    //              [ READ_ONLY | SCROLL_LOCKS | OPTIMISTIC ]
    //              [ TYPE_WARNING ]
    //              FOR Data_QueryExpression
    //              [ FOR UPDATE [ OF column_name [ ,...n ] ] ]
    [StatementParser(Core.TokenID.DECLARE, prio:3)]
    public class Statement_DECLARE_CURSOR: Statement
    {
        public      readonly    Core.TokenWithSymbol                n_Name;
        public      readonly    DataModel.CursorFlags               n_CursorFlags;
        public      readonly    Query_Select                        n_Select;
        public      readonly    Node_QueryOptions                   n_QueryOptions;
        public      readonly    Core.TokenWithSymbol[]              n_UpdateColumns;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            Core.Token[]        peek = reader.Peek(3);

            return (peek[0].isToken(Core.TokenID.DECLARE) && peek[1].isNameOrQuotedName && peek[2].isToken(Core.TokenID.CURSOR));
        }
        public                                                      Statement_DECLARE_CURSOR(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DECLARE);
            n_Name = ParseName(reader);
            ParseToken(reader, Core.TokenID.CURSOR);

            n_CursorFlags |= (DataModel.CursorFlags)ParseOptionalToken(reader, _options1);
            n_CursorFlags |= (DataModel.CursorFlags)ParseOptionalToken(reader, _options2);
            n_CursorFlags |= (DataModel.CursorFlags)ParseOptionalToken(reader, _options3);
            n_CursorFlags |= (DataModel.CursorFlags)ParseOptionalToken(reader, _options4);
            n_CursorFlags |= (DataModel.CursorFlags)ParseOptionalToken(reader, _options5);

            ParseToken(reader, Core.TokenID.FOR);

            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.StatementDeclareCursor));

            if (reader.CurrentToken.isToken(Core.TokenID.OPTION))
                n_QueryOptions = AddChild(new Node_QueryOptions(reader));

            if (ParseOptionalToken(reader, Core.TokenID.FOR) != null) {
                ParseToken(reader, Core.TokenID.UPDATE);

                if (ParseOptionalToken(reader, Core.TokenID.OF) != null) {
                    var columns = new List<Core.TokenWithSymbol>();

                    do {
                        columns.Add(ParseName(reader));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_UpdateColumns = columns.ToArray();
                }
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            var contextStatement = new Transpile.ContextStatementQuery(context);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Select.TranspileNode(contextStatement);

            if (n_UpdateColumns != null) {
                foreach(var updateColumn in n_UpdateColumns) {
                    var column = n_Select.Resultset.FindColumn(updateColumn.ValueString, out var ambigous);
                    if (column != null) {
                        if (ambigous)
                            context.AddError(updateColumn, "Column [" + updateColumn.ValueString + "] is ambiguous.");

                        updateColumn.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Reference);
                        context.CaseWarning(updateColumn, column.Name);
                    }
                    else
                        context.AddError(updateColumn, "Unknown column '" + updateColumn.ValueString + "' in result set.");
                }
            }

            if ((n_CursorFlags & (DataModel.CursorFlags.GLOBAL|DataModel.CursorFlags.LOCAL)) == 0)
                context.AddError(n_Name, "Missing LOCAL, GLOBAL cursor option.");

            var cursor = ((n_CursorFlags & (DataModel.CursorFlags.GLOBAL)) != 0
                                ? context.Catalog.GetGlobalCursorList()
                                : context.RootContext.GetCursorList()
                         ).Define(n_Name.ValueString, n_Name, n_CursorFlags, n_Select.Resultset);
            n_Name.SetSymbolUsage(cursor, DataModel.SymbolUsageFlags.Declaration);
        }

        private     static      Core.TokenNameID[]                  _options1 = new Core.TokenNameID[]
                                                                                {
                                                                                    new Core.TokenNameID("LOCAL",           (int)DataModel.CursorFlags.LOCAL),
                                                                                    new Core.TokenNameID("GLOBAL",          (int)DataModel.CursorFlags.GLOBAL)
                                                                                };
        private     static      Core.TokenNameID[]                  _options2 = new Core.TokenNameID[]
                                                                                {
                                                                                    new Core.TokenNameID("FORWARD_ONLY",    (int)DataModel.CursorFlags.FORWARD_ONLY),
                                                                                    new Core.TokenNameID("SCROLL",          (int)DataModel.CursorFlags.SCROLL)
                                                                                };
        private     static      Core.TokenNameID[]                  _options3 = new Core.TokenNameID[]
                                                                                {
                                                                                    new Core.TokenNameID("FAST_FORWARD",    (int)DataModel.CursorFlags.FAST_FORWARD),
                                                                                    new Core.TokenNameID("STATIC",          (int)DataModel.CursorFlags.STATIC),
                                                                                    new Core.TokenNameID("KEYSET",          (int)DataModel.CursorFlags.KEYSET),
                                                                                    new Core.TokenNameID("DYNAMIC",         (int)DataModel.CursorFlags.DYNAMIC)
                                                                                };
        private     static      Core.TokenNameID[]                  _options4 = new Core.TokenNameID[]
                                                                                {
                                                                                    new Core.TokenNameID("READ_ONLY",       (int)DataModel.CursorFlags.READ_ONLY),
                                                                                    new Core.TokenNameID("SCROLL_LOCKS",    (int)DataModel.CursorFlags.SCROLL_LOCKS),
                                                                                    new Core.TokenNameID("OPTIMISTIC",      (int)DataModel.CursorFlags.OPTIMISTIC)
                                                                                };
        private     static      Core.TokenNameID[]                  _options5 = new Core.TokenNameID[]
                                                                                {
                                                                                    new Core.TokenNameID("TYPE_WARNING",    (int)DataModel.CursorFlags.TYPE_WARNING),
                                                                                };
    }
}
