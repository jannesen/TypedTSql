using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnExpression: Query_Select_Column
    {
        public      readonly        Core.TokenWithSymbol    n_ColumnName;
        public      readonly        IExprNode               n_Expression;

        public                                              Query_Select_ColumnExpression(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(2);

            if (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Equal) {
                n_ColumnName = ParseName(reader);
                ParseToken(reader, Core.TokenID.Equal);
                n_Expression = ParseExpression(reader);
            }
            else {
                n_Expression = ParseExpression(reader);

                if (ParseOptionalToken(reader, Core.TokenID.AS) != null) {
                    n_ColumnName = ParseName(reader);
                }
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (n_ColumnName != null) {
                var column = new DataModel.ColumnExpr(n_ColumnName,
                                                      n_Expression,
                                                      declaration: n_ColumnName);
                n_ColumnName.SetSymbol(column);

                columns.Add(column);
                return ;
            }

            if (n_Expression is Expr_PrimativeValue primativeData) {
                if (primativeData.Referenced is DataModel.Column expressionColumn) {
                    columns.Add(expressionColumn);
                    return;
                }
            }

            columns.Add(new DataModel.ColumnExpr(n_Expression));
        }
    }
}
