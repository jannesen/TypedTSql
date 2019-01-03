using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms179859.aspx
    public class Expr_Operator_LIKE: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr;
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                       n_Pattern;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID) {
            case Core.TokenID.LIKE:
                return true;
            case Core.TokenID.NOT:
                return reader.NextPeek().isToken(Core.TokenID.LIKE);

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_LIKE(Core.ParserReader reader, IExprNode expr, ParseCallback parser)
        {
            n_Expr = AddChild(expr);

            ParseOptionalToken(reader, Core.TokenID.NOT);
            ParseToken(reader, Core.TokenID.LIKE);

            n_Pattern = AddChild(parser(reader));

            if (ParseOptionalToken(reader, Core.TokenID.ESCAPE) != null)
                ParseToken(reader, Core.TokenID.String);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);
                n_Pattern.TranspileNode(context);
                Validate.Value(n_Expr);
                Validate.Value(n_Pattern);
                Validate.ValueStringOrText(n_Expr);
                Validate.ValueString(n_Pattern);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
