using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms187922.aspx
    public class Expr_Operator_BETWEEN: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr1;
        public      readonly    IExprNode                       n_ExprBegin;
        public      readonly    IExprNode                       n_ExprEnd;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID)
            {
            case Core.TokenID.BETWEEN:
                return true;
            case Core.TokenID.NOT:
                return reader.NextPeek().isToken(Core.TokenID.BETWEEN);

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_BETWEEN(Core.ParserReader reader, IExprNode expr, ParseCallback parser)
        {
            n_Expr1     = AddChild(expr);
            ParseOptionalToken(reader, Core.TokenID.NOT);
            ParseToken(reader, Core.TokenID.BETWEEN);
            n_ExprBegin = AddChild(parser(reader));
            ParseToken(reader, Core.TokenID.AND);
            n_ExprEnd   = AddChild(parser(reader));
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr1.TranspileNode(context);
                n_ExprBegin.TranspileNode(context);
                n_ExprEnd.TranspileNode(context);
                TypeHelpers.OperationCompare(context, null, n_Expr1, n_ExprBegin);
                TypeHelpers.OperationCompare(context, null, n_Expr1, n_ExprEnd);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
