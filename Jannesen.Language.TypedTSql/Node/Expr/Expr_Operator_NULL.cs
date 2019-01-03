using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177682.aspx
    // https://msdn.microsoft.com/en-us/library/ms179859.aspx
    public class Expr_Operator_NULL: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID) {
            case Core.TokenID.IS:
                return true;
            case Core.TokenID.NOT:
                return reader.NextPeek().isToken(Core.TokenID.IS);

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_NULL(Core.ParserReader reader, IExprNode expr)
        {
            n_Expr = AddChild(expr);

            ParseToken(reader, Core.TokenID.IS);
            ParseOptionalToken(reader, Core.TokenID.NOT);
            ParseToken(reader, Core.TokenID.NULL);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);
                Validate.Value(n_Expr);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
