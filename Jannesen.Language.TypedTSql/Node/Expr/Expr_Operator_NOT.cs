using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_OperatorUnary
    //      : ('~'|'+'|'-') Expression
    public class Expr_Operator_NOT: ExprBoolean
    {
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                           n_Expr;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID) {
            case Core.TokenID.NOT:
                return true;

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_NOT(Core.ParserReader reader, ParseCallback parser)
        {
            n_Operator = ParseToken(reader);
            n_Expr     = AddChild(parser(reader));
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);
                Validate.BooleanExpression(n_Expr);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
