using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_OperatorAddSub
    //      : Expression ('+' | '-' | '&' | '^' | '|') Expression
    public class Expr_Operator_Calculation: ExprCalculation
    {
        public      readonly    IExprNode                       n_Expr1;
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                       n_Expr2;

        public      override    DataModel.ValueFlags            ValueFlags      { get { return _valueFlags;          } }
        public      override    DataModel.ISqlType              SqlType         { get { return _sqlType;             } }
        public      override    string                          CollationName   { get { return _collationName;       } }

        private                 DataModel.ValueFlags            _valueFlags;
        private                 DataModel.ISqlType              _sqlType;
        private                 string                          _collationName;

        private                                                 Expr_Operator_Calculation(Core.ParserReader reader, IExprNode expr1, ParseCallback parser)
        {
            n_Expr1    = AddChild(expr1);
            n_Operator = ParseToken(reader);
            n_Expr2    = AddChild(parser(reader));
        }

        public      static      IExprNode                       Parse(Core.ParserReader reader, ParseCallback parser, TestCallback test)
        {
            var expr = parser(reader);

            while (test(reader.CurrentToken.ID))
                expr = new Expr_Operator_Calculation(reader, expr, parser);

            return expr;
        }

        public      override    object                          ConstValue()
        {
            var constValue1 = n_Expr1.ConstValue();
            if (constValue1 == null || constValue1 is Exception)
                return constValue1;

            var constValue2 = n_Expr2.ConstValue();
            if (constValue2 == null || constValue2 is Exception)
                return constValue1;

            try {
                return Calculator.Calculate(n_Operator.ID, constValue1, constValue2);
            }
            catch(InvalidOperationException) {
                throw new TranspileException(this, constValue1.GetType().Name + " " + n_Operator + " " + constValue2.GetType().Name + " not possible.");
            }
            catch(TranspileException) {
                throw;
            }
            catch(Exception err) {
                throw new TranspileException(this, "Constant calculation failed.", err);
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _valueFlags    = DataModel.ValueFlags.Error;
            _sqlType       = null;
            _collationName = null;

            try {
                n_Expr1.TranspileNode(context);
                n_Expr2.TranspileNode(context);

                _valueFlags = LogicStatic.ComputedValueFlags(n_Expr1.ValueFlags | n_Expr2.ValueFlags);

                if (_valueFlags.isValid()) {
                    Validate.Value(n_Expr1);
                    Validate.Value(n_Expr2);

                    _sqlType = TypeHelpers.OperationCalculation(n_Operator, n_Expr1, n_Expr2);
                }
            }
            catch(Exception err) {
                _valueFlags    = DataModel.ValueFlags.Error;
                _sqlType       = null;
                _collationName = null;
                context.AddError(this, err);
            }
        }
    }
}
