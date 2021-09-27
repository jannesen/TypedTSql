using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms189499.aspx
    //      SELECT [ ALL | DISTINCT ] [TOP ( expression ) [PERCENT] [ WITH TIES ] ] < SelectList >
    //      [ INTO new_table ]
    //      [ FROM { <TableSource> } [ ,...n ]
    //      [ WHERE <search_condition> ]
    //      [ <GROUP BY> ]
    //      [ HAVING < search_condition > ] ]
    public class Query_Select_SELECT: Core.AstParseNode
    {
        public      readonly    IExprNode                   n_Top;
        public      readonly    Query_Select_ColumnList     n_Columns;
        public      readonly    Core.TokenWithSymbol        n_Into;
        public      readonly    TableSource                 n_From;
        public      readonly    IExprNode                   n_Where;
        public      readonly    Query_Select_GroupBy        n_GroupBy;
        public      readonly    IExprNode                   n_Having;

        public                                              Query_Select_SELECT(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            ParseToken(reader, Core.TokenID.SELECT);

            if (selectContext != Query_SelectContext.ExpressionEXISTS) {
                ParseOptionalToken(reader, Core.TokenID.ALL, Core.TokenID.DISTINCT);

                if (ParseOptionalToken(reader, Core.TokenID.TOP) != null) {
                    ParseToken(reader, Core.TokenID.LrBracket);
                    n_Top = ParseExpression(reader);
                    ParseToken(reader, Core.TokenID.RrBracket);

                    if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                        ParseToken(reader, "TIES");
                    }
                }

                n_Columns = AddChild(new Query_Select_ColumnList(reader, selectContext));
            }
            else {
                ParseToken(reader, Core.TokenID.Star);
            }

            if (selectContext == Query_SelectContext.StatementSelect) {
                if (ParseOptionalToken(reader, Core.TokenID.INTO) != null) {
                    n_Into = ParseName(reader);
                }
            }

            if (ParseOptionalToken(reader, Core.TokenID.FROM) != null) {
                n_From = AddChild(new TableSource(reader));
            }

            if (ParseOptionalToken(reader, Core.TokenID.WHERE) != null) {
                n_Where = ParseExpression(reader);
            }

            if (reader.CurrentToken.isToken(Core.TokenID.GROUP)) {
                n_GroupBy = AddChild(new Query_Select_GroupBy(reader));
            }

            if (ParseOptionalToken(reader, Core.TokenID.HAVING) != null) {
                n_Having = ParseExpression(reader);
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            var c = new Transpile.ContextRowSets(context);
            n_From?.TranspileNode(c);
            n_Top?.TranspileNode(c);
            context.RowSets.AddRange(c.RowSets);
            n_Columns?.TranspileNode(context);
            n_Where?.TranspileNode(context);
            n_GroupBy?.TranspileNode(context);
            n_Having?.TranspileNode(context);
        }
    }
}
