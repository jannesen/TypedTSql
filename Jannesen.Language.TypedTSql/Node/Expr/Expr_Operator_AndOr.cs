using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_OperatorAndOr
    //      : Expression_OperatorCompare ('AND' | 'OR') Expression_OperatorCompare
    public class Expr_Operator_AndOr: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr1;
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                       n_Expr2;

        public                                                  Expr_Operator_AndOr(Core.ParserReader reader, IExprNode node, ParseCallback parser)
        {
            n_Expr1    = AddChild(node);
            n_Operator = ParseToken(reader);
            n_Expr2    = AddChild(parser(reader));
        }
        public      static      IExprNode                       Parse(Core.ParserReader reader, ParseCallback parser, TestCallback test)
        {
            var expr = parser(reader);

            while (test(reader.CurrentToken.ID))
                expr = new Expr_Operator_AndOr(reader, expr, parser);

            return expr;
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr1.TranspileNode(context);
                n_Expr2.TranspileNode(context);

                Validate.BooleanExpression(n_Expr1);
                Validate.BooleanExpression(n_Expr2);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
