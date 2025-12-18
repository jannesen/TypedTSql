using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_with_COLLATE: Core.AstParseNode
    {
        public      readonly    IExprNode                       n_Expression;
        public      readonly    Core.Token                      n_Collate;

        public                                                  Expr_with_COLLATE(Core.ParserReader reader)
        {
            n_Expression = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.COLLATE) != null) {
                n_Collate = ParseName(reader);
            }

            ParseOptionalToken(reader, Core.TokenID.ASC, Core.TokenID.DESC);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
        }
    }
}
