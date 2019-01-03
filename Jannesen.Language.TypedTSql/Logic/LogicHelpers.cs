using System;

namespace Jannesen.Language.TypedTSql.Logic
{
    public static class LogicHelpers
    {
        public  static  bool                    isNullable(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isNullable();
        }
        public  static  bool                    isNullable(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & DataModel.ValueFlags.Nullable) != 0;
        }
        public  static  bool                    isNull(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isNull();
        }
        public  static  bool                    isNull(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & DataModel.ValueFlags.SourceFlags) == DataModel.ValueFlags.NULL;
        }
        public  static  bool                    isConstant(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isConstant();
        }
        public  static  bool                    isConstant(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & DataModel.ValueFlags.SourceFlags) == DataModel.ValueFlags.Const;
        }
        public  static  bool                    isConstantAndNoCast(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & (DataModel.ValueFlags.SourceFlags | DataModel.ValueFlags.Cast)) == DataModel.ValueFlags.Const;
        }
        public  static  bool                    isNullOrConstant(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isNullOrConstant();
        }
        public  static  bool                    isNullOrConstant(this DataModel.ValueFlags valueFlags)
        {
            var f = (valueFlags & DataModel.ValueFlags.SourceFlags);
            return f == DataModel.ValueFlags.NULL  ||
                   f == DataModel.ValueFlags.Const ||
                   f == (DataModel.ValueFlags.NULL|DataModel.ValueFlags.Const);
        }
        public  static  bool                    isComputedOrFunction(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isComputedOrFunction();
        }
        public  static  bool                    isComputedOrFunction(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & (DataModel.ValueFlags.Function|DataModel.ValueFlags.Computed)) != 0;
        }
        public  static  bool                    isBooleanExpression(this Node.IExprNode expr)
        {
            return expr.ValueFlags.isBooleanExpression();
        }
        public  static  bool                    isBooleanExpression(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & DataModel.ValueFlags.BooleanExpression) != 0;
        }
        public  static  bool                    isValid(this DataModel.IExprResult expr)
        {
            return expr.ValueFlags.isValid();
        }
        public  static  bool                    isValid(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & DataModel.ValueFlags.Error) == 0;
        }
        public  static  bool                    isComputedFunction(this DataModel.ValueFlags valueFlags)
        {
            return (valueFlags & (DataModel.ValueFlags.Computed|DataModel.ValueFlags.Function)) != 0;
        }

        public  static  object                  getConstValue(this Node.IExprNode expr, DataModel.ISqlType sqlType = null)
        {
            object value = expr.ConstValue();

            if (value is Exception)
                throw (Exception)value;

            if (sqlType != null && value != null)
                Validate.ConstValue(value, sqlType, expr);

            return value;
        }
        public  static  Token.String            ConstString(this Node.IExprNode expr)
        {
            if (expr is Node.Expr_Constant && expr.Children != null && expr.Children.Count == 1) {
                return expr.Children[0] as Token.String;
            }

            return null;
        }
    }
}
