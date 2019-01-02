using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_OperatorAddSub
    //      : Expression COLLATE name
    public class Expr_Operator_Collate: ExprCalculation
    {
        public      readonly    IExprNode                       n_Expr;
        public      readonly    Core.Token                      n_Collate;

        public      override    DataModel.ValueFlags            ValueFlags      { get { return _valueFlags;            } }
        public      override    DataModel.ISqlType              SqlType         { get { return n_Expr.SqlType;         } }
        public      override    string                          CollationName   { get { return n_Collate.ValueString;  } }

        private                 DataModel.ValueFlags            _valueFlags;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.COLLATE);
        }
        public                                                  Expr_Operator_Collate(Core.ParserReader reader, IExprNode expr)
        {
            n_Expr = AddChild(expr);
            ParseToken(reader, Core.TokenID.COLLATE);
            n_Collate = ParseToken(reader, Core.TokenID.Name);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);

                _valueFlags = LogicStatic.ComputedValueFlags(n_Expr.ValueFlags) | DataModel.ValueFlags.Collate;

                if (_valueFlags.isValid()) {
                    Validate.Value(n_Expr);

                    if ((n_Expr.ValueFlags & DataModel.ValueFlags.Collate) != 0)
                        context.AddError(this, "Collate already specified in expression.");
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
