using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177682.aspx
    // Expr_Operator2_IN
    //      : test_expression  (subquery | expression [ ,...n ] )
    public class Expr_Operator_IN: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr;
        public      readonly    Core.AstParseNode               n_In;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID)
            {
            case Core.TokenID.IN:
                return true;
            case Core.TokenID.NOT:
                return reader.NextPeek().isToken(Core.TokenID.IN);

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_IN(Core.ParserReader reader, IExprNode expr)
        {
            n_Expr = AddChild(expr);

            ParseOptionalToken(reader, Core.TokenID.NOT);
            ParseToken(reader, Core.TokenID.IN);

            if (Expr_Subquery.CanParse(reader)) {
                n_In = AddChild(new Expr_Subquery(reader));
            }
            else {
                n_In = AddChild(new Expr_Collection(reader, true));
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);
                n_In.TranspileNode(context);

                if (n_In is Expr_Collection)
                    _transpileNode_IN_SET(context);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }

        private                 void                            _transpileNode_IN_SET(Transpile.Context context)
        {
            foreach(var in_expr in ((Expr_Collection)n_In).n_Expressions) {
                try {
                    TypeHelpers.OperationCompare(context, null, n_Expr, in_expr);
                }
                catch(Exception err) {
                    context.AddError(this, err);
                }
            }
        }
    }
}
