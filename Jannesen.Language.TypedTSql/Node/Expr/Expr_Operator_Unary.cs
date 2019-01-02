using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_OperatorUnary
    //      : ('~'|'+'|'-') Expression
    public class Expr_Operator_Unary: ExprCalculation
    {
        public      readonly    Core.Token                      n_Operator;
        public      readonly    IExprNode                       n_Expr;

        public      override    DataModel.ValueFlags            ValueFlags      { get { return _valueFlags;                                   } }
        public      override    DataModel.ISqlType              SqlType         { get { return TypeHelpers.ReturnStrictType(n_Expr.SqlType);  } }

        private                 DataModel.ValueFlags            _valueFlags;

        public                                                  Expr_Operator_Unary(Core.ParserReader reader, ParseCallback parser)
        {
            n_Operator = ParseToken(reader);
            n_Expr     = AddChild(parser(reader));
        }

        public      override    object                          ConstValue()
        {
            var constValue = n_Expr.ConstValue();

            if (constValue == null || constValue is Exception)
                return constValue;

            try {
                if (constValue == null)         return null;
                if (constValue is Int32)        return _calculate((Int32)constValue);
                if (constValue is Int64)        return _calculate((Int64)constValue);
                if (constValue is decimal)      return _calculate((decimal)constValue);
                if (constValue is double)       return _calculate((double)constValue);

                return new TranspileException(this, "Not a numeric expression.");
            }
            catch(TranspileException err) {
                return err;
            }
            catch(Exception err) {
                return new TranspileException(this, "Constante calculation failed.", err);
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr.TranspileNode(context);

                _valueFlags = LogicStatic.ComputedValueFlags(n_Expr.ValueFlags);

                if (_valueFlags.isValid()) {
                    _transpileOperationPosible(context, n_Expr.SqlType);
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                context.AddError(this, err);
            }
        }

        public                  void                            _transpileOperationPosible(Transpile.Context context, DataModel.ISqlType sqlType)
        {
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return;

            var nativeType = sqlType.NativeType;

            switch(nativeType.SystemType) {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
                switch(n_Operator.ID) {
                case Core.TokenID.Minus:
                case Core.TokenID.Plus:
                case Core.TokenID.BitNot:
                    return;
                }
                break;

            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
            case DataModel.SystemType.Float:
            case DataModel.SystemType.Real:
                switch(n_Operator.ID) {
                case Core.TokenID.Minus:
                case Core.TokenID.Plus:
                    return ;
                }
                break;

            case DataModel.SystemType.SqlVariant:
                return ;
            }

            throw new TranspileException(this, "Unary operation '" + n_Operator + "' not posible on type '" + nativeType.ToString() + "'.");
        }
        public                  Int32                           _calculate(Int32 value)
        {
            switch(n_Operator.ID) {
            case Core.TokenID.Minus:        return -value;
            case Core.TokenID.Plus:         return value;
            case Core.TokenID.BitNot:       return ~value;
            default:                        throw new TranspileException(this, "Can't calculate '" + n_Operator + "'.");
            }
        }
        public                  Int64                           _calculate(Int64 value)
        {
            switch(n_Operator.ID) {
            case Core.TokenID.Minus:        return -value;
            case Core.TokenID.Plus:         return value;
            case Core.TokenID.BitNot:       return ~value;
            default:                        throw new TranspileException(this, "Can't calculate '" + n_Operator + "'.");
            }
        }
        public                  decimal                         _calculate(decimal value)
        {
            switch(n_Operator.ID) {
            case Core.TokenID.Minus:        return -value;
            case Core.TokenID.Plus:         return value;
            default:                        throw new TranspileException(this, "Can't calculate '" + n_Operator + "'.");
            }
        }
        public                  double                          _calculate(double value)
        {
            switch(n_Operator.ID) {
            case Core.TokenID.Minus:        return -value;
            case Core.TokenID.Plus:         return value;
            default:                        throw new TranspileException(this, "Can't calculate '" + n_Operator + "'.");
            }
        }
    }
}
