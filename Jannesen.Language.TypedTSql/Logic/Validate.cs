using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Logic
{
    public enum DatePartMode
    {
        Date,
        Time
    }

    public static class Validate
    {
        private     static      System.Globalization.GregorianCalendar      _gregorianCalendar = new System.Globalization.GregorianCalendar();

        public      static      void                        Schema(Transpile.Context context, Core.TokenWithSymbol node)
        {
            if (node != null) {
                var name   = node.ValueString;
                var schema = context.Catalog.GetSchema(name);
                if (schema == null) {
                    context.AddError(node, "Unknown schema '" + name + "'.");
                    return;
                }
                node.SetSymbol(schema);
            }
        }
        public      static      void                        BooleanExpression(Node.IExprNode expr)
        {
            if (!expr.ValueFlags.isBooleanExpression())
                throw new TranspileException(expr, "Expression is not a boolean expression.");
        }
        public      static      void                        Value(Node.IExprNode expr)
        {
            var flags = expr.ValueFlags;

            if (flags.isNull())
                throw new TranspileException(expr, "Expression is NULL.");

            if (flags.isBooleanExpression())
                throw new TranspileException(expr, "Expression is a boolean expression.");
        }
        public      static      void                        ValueOrNull(Node.IExprNode expr)
        {
            var flags = expr.ValueFlags;

            if (flags.isBooleanExpression())
                throw new TranspileException(expr, "Expression is a boolean expression.");
        }
        public      static      void                        ValueInt(Node.IExprNode expr)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
                return;
            }

            throw new TranspileException(expr, "Not a integer value.");
        }
        public      static      object                      ValueInt(Node.IExprNode expr, int minValue, int maxValue)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return null;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
                if (expr.isConstant()) {
                    object v = expr.ConstValue();

                    if (v is int) {
                        if (!(minValue <= (int)v && (int)v <= maxValue))
                            throw new TranspileException(expr, "Integer value out of range. Value must by between " + minValue + " and " + maxValue + ".");

                        return v;
                    }
                }
                return null;
            }

            throw new TranspileException(expr, "Not a integer value.");
        }
        public      static      object                      ConstInt(Node.IExprNode expr, int minValue, int maxValue)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return null;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
                if (!expr.isConstant())
                    throw new TranspileException(expr, "Expect const value.");

                object v = expr.ConstValue();

                if (!(v is int))
                    throw new TranspileException(expr, "Expect const int-value.");

                if (!(minValue <= (int)v && (int)v <= maxValue))
                    throw new TranspileException(expr, "Integer value out of range. Value must by between " + minValue + " and " + maxValue + ".");

                return v;
            }

            throw new TranspileException(expr, "Not a integer value.");
        }
        public      static      string                      ConstString(Node.IExprNode expr)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return null;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NVarChar:
                if (!expr.isConstant())
                    throw new TranspileException(expr, "Expect const value.");

                object v = expr.ConstValue();

                if (!(v is string))
                    throw new TranspileException(expr, "Expect const string-value.");

                return (string)v;
            }

            throw new TranspileException(expr, "Not a string value.");
        }
        public      static      void                        ValueNumber(Node.IExprNode expr)
        {
            Value(expr);

            var sqlType = expr.SqlType;

            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                return;

            default:
                throw new TranspileException(expr, sqlType.NativeType.ToString() + " is not a number.");
            }
        }
        public      static      void                        ValueString(Node.IExprNode expr)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NVarChar:
                return;
            }

            throw new TranspileException(expr, "Not a string value.");
        }
        public      static      void                        ValueStringOrText(Node.IExprNode expr)
        {
            Value(expr);

            var sqlType = expr.SqlType;
            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NVarChar:
            case DataModel.SystemType.Text:
            case DataModel.SystemType.NText:
                return;
            }

            throw new TranspileException(expr, "Not a string value.");
        }
        public      static      DataModel.ISqlType          ValueDateTime(Node.IExprNode expr, DatePartMode mode)
        {
            var valueFlags = expr.ValueFlags;
            var sqlType    = expr.SqlType;

            Value(expr);

            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return sqlType;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Date:
                if (mode == DatePartMode.Time)
                    throw new TranspileException(expr, "Not allowed to add time to date.");
                return sqlType;

            case DataModel.SystemType.Time:
                if (mode == DatePartMode.Date)
                    throw new TranspileException(expr, "Not allowed to add date to time.");
                return sqlType;

            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
            case DataModel.SystemType.DateTimeOffset:
                return sqlType;

            case DataModel.SystemType.Char:
                if (expr.isConstant()) {
                    if (expr.ConstValue() is string constValue) {
                        if (constValue.Length >= 18 && constValue.Length <= 23 && IsDateTimeString(constValue))                             return new DataModel.SqlTypeNative(DataModel.SystemType.DateTime);
                        if (constValue.Length == 10                            && IsDateString(constValue) && mode == DatePartMode.Date)    return new DataModel.SqlTypeNative(DataModel.SystemType.Date);
                        if (constValue.Length >=  8 && constValue.Length <= 12 && IsTimeString(constValue) && mode == DatePartMode.Time)    return new DataModel.SqlTypeNative(DataModel.SystemType.Time);
                    }
                }

                throw new TranspileException(expr, "Constante is not a date/time.");

            default:
                throw new TranspileException(expr, sqlType.NativeType.ToString() + " is not a date/time value.");
            }
        }
        public      static      void                        ValueBinary(Node.IExprNode expr)
        {
            var valueFlags = expr.ValueFlags;
            var sqlType    = expr.SqlType;

            Value(expr);

            if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                return ;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                return;

            default:
                throw new TranspileException(expr, sqlType.NativeType.ToString() + " is not a binary value.");
            }
        }
        public      static      void                        NumberOfArguments(Node.IExprNode[] arguments, int expected)
        {
            var actual = (arguments == null ? 0 : arguments.Length);

            if (actual != expected)
                throw new ErrorException("Invalid number of arguments. Expect " + expected + " got " + actual + ".");
        }
        public      static      void                        NumberOfArguments(Node.IExprNode[] arguments, int minExpected, int maxExpected)
        {
            var actual = (arguments == null ? 0 : arguments.Length);

            if (!(minExpected <= actual && actual <= maxExpected))
                throw new ErrorException("Invalid number of arguments.");
        }
        public      static      bool                        ConstByType(DataModel.ISqlType sqlType, DataModel.IExprResult expr)
        {
            if (expr != null && sqlType != null && !(sqlType is DataModel.SqlTypeAny))
                return expr.ValidateConst(sqlType);

            return false;
        }
        public      static      DatePartMode                DatePart(Core.Token datepart)
        {
            Core.TokenWithSymbol.SetNoSymbol(datepart);

            switch(datepart.Text.ToUpperInvariant()) {
            case "YEAR":
            case "YY":
            case "QUARTER":
            case "QQ":
            case "MONTH":
            case "MM":
            case "DAYOFYEAR":
            case "DY":
            case "DAY":
            case "DD":
            case "WEEK":
            case "WK":
            case "WEEKDAY":
            case "DW":
                return DatePartMode.Date;

            case "HOUR":
            case "HH":
            case "MINUTE":
            case "MI":
            case "SECOND":
            case "SS":
            case "MILLISECOND":
            case "MS":
            case "MICROSECOND":
            case "MCS":
            case "NANOSECOND":
            case "NS":
                return DatePartMode.Time;

            default:
                throw new TranspileException(datepart, "Invalid datepart '" + datepart.Text + "'.");
            }
        }
        public      static      ConversionType              CastConvert(Node.Node_Datatype cast, Node.IExprNode expr, Node.IExprNode style=null)
        {
            var     castSqlType    = cast.SqlType;
            var     exprSqlType    = expr.SqlType;

            if (exprSqlType is DataModel.SqlTypeAny)
                return ConversionType.Explicit;

            if ((castSqlType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) == 0)
                throw new TranspileException(cast, "Not a native type.");

            var rtn = TypeHelpers.Convert(castSqlType, expr, exprSqlType);
            if (rtn == ConversionType.NotAllowed)
                throw new ErrorException("Conversion not allowed.");

            if (style != null) {
                ValueInt(style);

                if (style.isConstant()) {
                    var styleValue = style.ConstValue();

                    if (styleValue is int) {
                        if (!(Validate.ConvertStyle(castSqlType.NativeType, (int)styleValue) || Validate.ConvertStyle(exprSqlType.NativeType, (int)styleValue)))
                            throw new ErrorException("Invalid style in CONVERT(" + castSqlType.NativeType.ToString() + ", " + exprSqlType.NativeType.ToString() + ", " + styleValue.ToString() + ").");
                    }
                }
            }

            return rtn;
        }
        public      static      bool                        ConvertStyle(DataModel.SqlTypeNative nativeType, int style)
        {
            switch(nativeType.SystemType) {
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
                return (style == 0 || style == 1 || style == 2);

            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                return (style == 0 || style == 1 || style == 2 || style == 3);

            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                return (style == 0 || style == 1 || style == 2);

            case DataModel.SystemType.Date:
            case DataModel.SystemType.Time:
            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
            case DataModel.SystemType.DateTimeOffset:
                return ((  1 <= style && style <=  14) ||
                        (100 <= style && style <= 114) ||
                        style ==  20 || style == 120 || style ==  21 || style == 121 ||
                        style == 126 || style == 127 || style == 130 || style == 131);

            default:
                return false;
            }
        }
        public      static      void                        Assign(Transpile.Context context, Core.IAstNode targetNode, DataModel.Variable target, DataModel.IExprResult expr, bool output=false)
        {
            var targetType = target.SqlType;

            if (_assign(targetType, expr, output, target.isSaveCast))
                return ;

            if ((targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict) && expr is Node.IExprNode)
                QuickFixLogic.QuickFix_Expr(context, targetType, (Node.IExprNode)expr);

            if ((targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict) && expr is DataModel.ColumnExpr)
                QuickFixLogic.QuickFix_Expr(context, targetType, ((DataModel.ColumnExpr)expr).Expr);

            if ((expr.SqlType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || expr.SqlType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict)  &&
                (expr.SqlType.TypeFlags & DataModel.SqlTypeFlags.UserType) != 0  &&
                (targetType.TypeFlags & (DataModel.SqlTypeFlags.SimpleType|DataModel.SqlTypeFlags.UserType)) == DataModel.SqlTypeFlags.SimpleType &&
                (DataModel.SqlTypeNative)targetType == expr.SqlType.NativeType)
                QuickFixLogic.QuickFix_VariableType(context, targetNode, target, expr.SqlType);

            throw new Exception("Not allowed to assign a " + expr.SqlType.ToString() + " to " + targetType.ToString() + ".");
        }
        public      static      void                        Assign(Transpile.Context context, DataModel.Column target, DataModel.IExprResult expr)
        {
            var targetType = target.SqlType;

            if (_assign(targetType, expr, false))
                return ;

            if ((targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict) && expr is Node.IExprNode)
                QuickFixLogic.QuickFix_Expr(context, targetType, (Node.IExprNode)expr);

            if ((targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || targetType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict) && expr is DataModel.ColumnExpr)
                QuickFixLogic.QuickFix_Expr(context, targetType, ((DataModel.ColumnExpr)expr).Expr);

            throw new Exception("Not allowed to assign a " + expr.SqlType.ToString() + " to " + targetType.ToString() + ".");
        }
        public      static      void                        Assign(Transpile.Context context, DataModel.ISqlType target, DataModel.IExprResult expr)
        {
            if (_assign(target, expr, false))
                return ;

            throw new Exception("Not allowed to assign a " + expr.SqlType.ToString() + " to " + target.ToString() + ".");
        }
        public      static      bool                        Assign(DataModel.ISqlType targetType, DataModel.ISqlType sourceType)
        {
            if ((sourceType == null || sourceType is DataModel.SqlTypeAny) ||
                (targetType == null || targetType is DataModel.SqlTypeAny))
                return true;

            if (object.ReferenceEquals(sourceType, targetType))
                return true;

            if ((sourceType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 &&
                (targetType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
            {
                var targetCheckMode = targetType.getTypeCheckMode();
                var sourceCheckMode = sourceType.getTypeCheckMode();

                if (targetCheckMode == DataModel.SqlTypeFlags.CheckStrict || sourceCheckMode == DataModel.SqlTypeFlags.CheckStrict) {
                    if (targetType == sourceType)
                        return true;
                }
                else
                if (targetCheckMode == DataModel.SqlTypeFlags.CheckStrong || sourceCheckMode == DataModel.SqlTypeFlags.CheckStrong) {
                    if (targetType == sourceType)
                        return true;
                }
                else
                if (targetCheckMode >= DataModel.SqlTypeFlags.CheckSafe || sourceCheckMode >= DataModel.SqlTypeFlags.CheckSafe) {
                    if (targetType.NativeType == sourceType.NativeType)
                        return true;

                    switch(TypeHelpers.Conversion(sourceType.NativeType, targetType.NativeType)) {
                    case ConversionType.Save:
                        return true;
                    }
                }
                else {
                    if (targetType.NativeType == sourceType.NativeType)
                        return true;

                    switch(TypeHelpers.Conversion(sourceType.NativeType, targetType.NativeType)) {
                    case ConversionType.Save:
                    case ConversionType.SaveCalculated_Implicit:
                    case ConversionType.Implicit:
                        return true;
                    }
                }
            }

            return false;
        }

        public      static      void                        FunctionArguments(Transpile.Context context, Core.AstParseNode functionNode, DataModel.EntityObjectCode function, Node.IExprNode[] arguments)
        {
            if (function != null) {
                DataModel.ParameterList parameters = function.Parameters;

                if (arguments.Length == (parameters != null ? parameters.Count : 0)) {
                    for (int i=0 ; i < arguments.Length ; ++i) {
                        try {
                            Assign(context, arguments[i], parameters[i], arguments[i]);
                        }
                        catch(Exception err) {
                            context.AddError(arguments[i], err);
                        }
                    }
                }
                else
                    context.AddError(functionNode, (parameters != null && arguments.Length < parameters.Count) ? "Argument missing." : "Tomay arguments.");
            }
        }
        public      static      DataModel.ISqlType          Property(DataModel.InterfaceList interfaceList, bool @static, Core.TokenWithSymbol name)
        {
            var nameValue = name.ValueString;

            if (interfaceList != null) {
                foreach(var intf in interfaceList) {
                    if (intf.Name == nameValue) {
                        if (@static) {
                            if (intf.Type != DataModel.SymbolType.ExternalStaticProperty)
                                throw new ErrorException("Invalid property access, property is not static.");
                        }
                        else {
                            if (intf.Type != DataModel.SymbolType.ExternalProperty)
                                throw new ErrorException("Invalid property access, property is static.");
                        }

                        name.SetSymbol(intf);
                        return intf.Returns;
                    }
                }
            }

            throw new ErrorException("Unknown property '" + nameValue + "'.");
        }
        public      static      DataModel.ISqlType          Method(DataModel.InterfaceList interfaceList, bool @static, Core.TokenWithSymbol name, Node.IExprNode[] arguments)
        {
            var         nameValue = name.ValueString;

            if (interfaceList != null) {
                Exception   error = null;

                foreach(var intf in interfaceList) {
                    if (intf.Name == nameValue) {
                        if (@static) {
                            if (intf.Type != DataModel.SymbolType.ExternalStaticMethod)
                                throw new ErrorException("Invalid method access, method is not static.");
                        }
                        else {
                            if (intf.Type != DataModel.SymbolType.ExternalMethod)
                                throw new ErrorException("Invalid method access, method is static.");
                        }

                        var err = _methodArguments(intf.Parameters, arguments);
                        if (err == null) {
                            name.SetSymbol(intf);
                            return intf.Returns;
                        }

                        if (error == null)
                            error = err;
                        else
                            error = new ErrorException("No matching call signature found.");
                    }
                }

                if (error != null)
                    throw error;
            }

            throw new ErrorException("Unknown method '" + nameValue + "'.");
        }
        public      static      void                        ConstValue(object constValue, DataModel.ISqlType sqlType, Core.IAstNode errorNode)
        {
            var nativeType = sqlType.NativeType;

            switch(nativeType.SystemType) {
            case DataModel.SystemType.Bit:
                if (!(constValue is Int32 && ((Int32)constValue == 0 || (Int32)constValue == 1)))
                    throw new TranspileException(errorNode, "Expect boolean const.");
                break;

            case DataModel.SystemType.TinyInt:
                if (!(constValue is Int32 && (0 <= (Int32)constValue && (Int32)constValue <= 255)))
                    throw new TranspileException(errorNode, "Expect tinyint const.");
                break;

            case DataModel.SystemType.SmallInt:
                if (!(constValue is Int32 && (-32768 <= (Int32)constValue && (Int32)constValue <= 32767)))
                    throw new TranspileException(errorNode, "Expect smallint const.");
                break;

            case DataModel.SystemType.Int:
                if (!(constValue is Int32))
                    throw new TranspileException(errorNode, "Expect int const.");
                break;

            case DataModel.SystemType.BigInt:
                if (!(constValue is Int32 || constValue is Int64))
                    throw new TranspileException(errorNode, "Expect bigint const.");
                break;

            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
                if (!(constValue is Int32 || constValue is decimal))
                    throw new TranspileException(errorNode, "Expect decimal constant.");
                break;

            case DataModel.SystemType.Float:
            case DataModel.SystemType.Real:
                if (!(constValue is Int32 || constValue is decimal || constValue is double))
                    throw new TranspileException(errorNode, "Expect float constant.");
                break;

            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                if (!(constValue is byte[]))
                    throw new TranspileException(errorNode, "Expect binary constant.");

                if (nativeType.MaxLength > 0 && ((byte[])constValue).Length > nativeType.MaxLength)
                    throw new TranspileException(errorNode, "Binary to long for " + nativeType.ToString() + ".");
                break;

            case DataModel.SystemType.Char:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NVarChar:
                if (!(constValue is string))
                    throw new TranspileException(errorNode, "Expect string constant.");

                if (nativeType.MaxLength > 0 && ((string)constValue).Length > nativeType.MaxLength)
                    throw new TranspileException(errorNode, "String to long for " + nativeType.ToString() + ".");
                break;

            case DataModel.SystemType.Date:
                if (!(constValue is string))
                    throw new TranspileException(errorNode, "Expect date constant.");

                if (!Validate.IsDateString(constValue as string))
                    throw new TranspileException(errorNode, "Invalid date constant.");
                break;

            case DataModel.SystemType.Time:
                if (!(constValue is string))
                    throw new TranspileException(errorNode, "Expect time constant.");

                if (!Validate.IsTimeString(constValue as string))
                    throw new TranspileException(errorNode, "Invalid time constant.");
                break;

            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime2:
            case DataModel.SystemType.DateTimeOffset:
                if (!(constValue is string))
                    throw new TranspileException(errorNode, "Expect datetime constant.");

                if (!(Validate.IsDateTimeString(constValue as string)))
                    throw new TranspileException(errorNode, "Invalid datetime constant.");
                break;

            case DataModel.SystemType.Image:
                if (!(constValue is byte[]))
                    throw new TranspileException(errorNode, "Expect binary constant.");
                break;

            case DataModel.SystemType.Text:
            case DataModel.SystemType.NText:
                if (constValue != null && !(constValue is string))
                    throw new TranspileException(errorNode, "Expect string constant.");
                break;

            case DataModel.SystemType.UniqueIdentifier:
                if (constValue != null && !(constValue is string))
                    throw new TranspileException(errorNode, "Expect string constant.");
                break;

            case DataModel.SystemType.Xml:
                if (constValue != null && !(constValue is string))
                    throw new TranspileException(errorNode, "Expect string constant.");
                break;

            case DataModel.SystemType.Clr:
                if (constValue != null && !(constValue is string))
                    throw new TranspileException(errorNode, "Expect string constant.");
                break;

            default:
                throw new TranspileException(errorNode, nativeType.ToString() + " constant not posible.");
            }
        }

        public      static      bool                        IsDateString(string svalue, int offset=0)
        {
            if (svalue == null || svalue.Length < offset + 10 || svalue[offset + 4] != '-' || svalue[offset + 7] != '-')
                return false;

            int year    = _atoi(svalue,  offset + 0, 4);
            int month   = _atoi(svalue,  offset + 5, 2);
            int day     = _atoi(svalue,  offset + 8, 2);

            if (year  < 1753 ||
                month < 1 || month > 12 ||
                day   < 1 || day   > 31 ||
                day > _gregorianCalendar.GetDaysInMonth(year, month))
                return false;

            return true;
        }
        public      static      bool                        IsTimeString(string svalue, int offset=0)
        {
            if (svalue == null || svalue.Length < offset + 8 || svalue[offset + 2] != ':' || svalue[offset + 5] != ':')
                return false;

            int hours   = _atoi(svalue, offset + 0, 2);
            int minutes = _atoi(svalue, offset + 3, 2);
            int seconds = _atoi(svalue, offset + 6, 2);

            if (hours   < 0 || hours   >= 24 ||
                minutes < 0 || minutes >= 60 ||
                seconds < 0 || seconds >= 60)
                return false;

            return true;
        }
        public      static      bool                        IsDateTimeString(string svalue)
        {
            if (svalue == null || svalue.Length < 19 || svalue[10] != 'T')
                return false;

            return IsDateString(svalue, 0) && IsTimeString(svalue, 11);
        }

        public      static      Exception                   _methodArguments(DataModel.ParameterList parameters, Node.IExprNode[] arguments)
        {
            if (parameters.Count > arguments.Length)
                throw new ErrorException("Not enough argument, expect " + parameters.Count);
            if (parameters.Count < arguments.Length)
                throw new ErrorException("Tomany arguments, expect " + parameters.Count);

            for (int narg = 0 ; narg < arguments.Length ; ++narg) {
                var parameter  = parameters[narg];
                var argument   = arguments[narg];
                var targetType = parameter.SqlType;

                if (!_assign(targetType, argument, false, parameter.isSaveCast))
                    return new Exception("Not allowed to assign a " + argument.SqlType.ToString() + " to " + targetType.ToString() + ".");
            }

            return null;
        }
        public      static      bool                        _assign(DataModel.ISqlType targetType, DataModel.IExprResult sourceExpr, bool output=false, bool saveCast=false)
        {
            var sourceFlags = sourceExpr.ValueFlags;

            if (!(sourceFlags.isValid()))
                return true;

            var sourceType = sourceExpr.SqlType;

            if ((sourceType == null || sourceType is DataModel.SqlTypeAny) ||
                (targetType == null || targetType is DataModel.SqlTypeAny))
                return true;

            if (object.ReferenceEquals(sourceType, targetType))
                return true;

            if ((sourceType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 &&
                (targetType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
            {
                if (sourceFlags.isNull())
                    return true;

                var targetCheckMode = targetType.getTypeCheckMode();
                var sourceCheckMode = sourceType.getTypeCheckMode();

                if ((targetCheckMode == DataModel.SqlTypeFlags.CheckStrict || sourceCheckMode == DataModel.SqlTypeFlags.CheckStrict) && !saveCast) {
                    if (targetType == sourceType)
                        return true;
                }
                else
                if ((targetCheckMode == DataModel.SqlTypeFlags.CheckStrong || sourceCheckMode == DataModel.SqlTypeFlags.CheckStrong) && !saveCast) {
                    if (sourceFlags.isNullOrConstant() && Validate.ConstByType(targetType.NativeType, sourceExpr))
                        return true;

                    if (targetType == sourceType)
                        return true;

                    if (sourceFlags.isComputedFunction() && !output) {
                        switch(TypeHelpers.Conversion(sourceType.NativeType, targetType.NativeType)) {
                        case ConversionType.Save:
                        case ConversionType.SaveCalculated_Implicit:
                            return true;
                        }
                    }
                }
                else
                if (targetCheckMode >= DataModel.SqlTypeFlags.CheckSafe || sourceCheckMode >= DataModel.SqlTypeFlags.CheckSafe) {
                    if (sourceFlags.isNullOrConstant() && Validate.ConstByType(targetType.NativeType, sourceExpr))
                        return true;

                    if (targetType.NativeType == sourceType.NativeType)
                        return true;

                    if (!output) {
                        switch(TypeHelpers.Conversion(sourceType.NativeType, targetType.NativeType)) {
                        case ConversionType.Save:
                            return true;

                        case ConversionType.SaveCalculated_Implicit:
                            if (sourceFlags.isComputedFunction())
                                return true;
                            break;
                        }
                    }
                }
                else {
                    if (sourceFlags.isNullOrConstant() && Validate.ConstByType(targetType.NativeType, sourceExpr))
                        return true;

                    if (targetType.NativeType == sourceType.NativeType)
                        return true;

                    if (!output) {
                        switch(TypeHelpers.Conversion(sourceType.NativeType, targetType.NativeType)) {
                        case ConversionType.Save:
                        case ConversionType.SaveCalculated_Implicit:
                        case ConversionType.Implicit:
                            return true;
                        }
                    }
                }
            }

            if (object.Equals(sourceType, DataModel.SqlTypeNative.Int) &&
                sourceFlags == (DataModel.ValueFlags.Nullable | DataModel.ValueFlags.NULL) &&
                targetType  is DataModel.EntityTypeExternal)
                return true;

            return false;
        }
        private     static      int                         _atoi(string s, int start, int length)
        {
            int     rtn = 0;

            while (length > 0) {
                char c = s[start];

                if ('0' > c || c > '9')
                    return -1;

                rtn = (rtn * 10) + (int)(c - '0');

                ++start;
                --length;
            }

            return rtn;
        }
    }
}
