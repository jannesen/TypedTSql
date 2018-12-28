using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177682.aspx
    // https://msdn.microsoft.com/en-us/library/ms179859.aspx
    // Expression_OperatorCompare
    //      : Expression_OperatorAddSub ('<' | '<=' | '=' | '<>' | '>=' | '>') Expression_OperatorAddSub
    public class Expr_Operator_Compare: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr1;
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                       n_Expr2;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID)
            {
            case Core.TokenID.Equal:
            case Core.TokenID.NotEqual:
            case Core.TokenID.Less:
            case Core.TokenID.Greater:
            case Core.TokenID.LessEqual:
            case Core.TokenID.GreaterEqual:
                return true;

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_Compare(Core.ParserReader reader, IExprNode expr1, ParseCallback parser)
        {
            n_Expr1    = AddChild(expr1);
            n_Operator = ParseToken(reader, Core.TokenID.Equal, Core.TokenID.NotEqual, Core.TokenID.Less, Core.TokenID.Greater, Core.TokenID.LessEqual, Core.TokenID.GreaterEqual);
            n_Expr2    = AddChild(parser(reader));
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr1.TranspileNode(context);
                n_Expr2.TranspileNode(context);
                TypeHelpers.OperationCompare(context, n_Operator, n_Expr1, n_Expr2);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
