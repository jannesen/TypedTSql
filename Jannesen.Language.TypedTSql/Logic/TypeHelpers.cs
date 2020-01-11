using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Logic
{
    public enum ConversionType
    {
        NotAllowed  = 0,
        Explicit,
        Implicit,
        SaveCalculated_Implicit,
        Save
    }
    public enum CompareType
    {
        NotAllowed  = 0,
        TSql,
        Save
    }

    public struct FlagsTypeCollation: IEquatable<FlagsTypeCollation>
    {
        public      DataModel.ValueFlags        ValueFlags;
        public      DataModel.ISqlType          SqlType;
        public      string                      CollationName;

        public      void                        Clear()
        {
            ValueFlags    = DataModel.ValueFlags.Error;
            SqlType       = null;
            CollationName = null;
        }

        public  static      bool            operator == (FlagsTypeCollation p1, FlagsTypeCollation p2)
        {
            return p1.ValueFlags    == p2.ValueFlags &&
                   p1.SqlType       == p2.SqlType &&
                   p1.CollationName == p2.CollationName;
        }
        public  static      bool            operator != (FlagsTypeCollation p1, FlagsTypeCollation p2)
        {
            return !(p1 == p2);
        }
        public  override    bool            Equals(object obj)
        {
            if (obj is FlagsTypeCollation)
                return this == (FlagsTypeCollation)obj;

            return false;
        }
        public              bool            Equals(FlagsTypeCollation o)
        {
            return this == o;
        }
        public  override    int             GetHashCode()
        {
            return ValueFlags.GetHashCode() ^
                   SqlType.GetHashCode() ^
                   (CollationName != null ? CollationName.GetHashCode() : 0);
        }
    }

    public static class TypeHelpers
    {
        public      static      DataModel.SqlTypeFlags      getTypeCheckMode(this DataModel.ISqlType sqlType)
        {
            return (sqlType.TypeFlags & DataModel.SqlTypeFlags.CheckMode);
        }
        public      static      DataModel.ISqlType          ReturnStrictType(DataModel.ISqlType sqlType)
        {
            return sqlType.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict ? sqlType.NativeType : sqlType;
        }

        public      static      ConversionType              Convert(DataModel.ISqlType targetSqlType, Node.IExprNode sourceExpr, DataModel.ISqlType sourceSqlType)
        {
            if (sourceExpr.isNull())
                return ConversionType.Implicit;

            if (sourceExpr.isConstant()) {
                if (Validate.ConstByType(targetSqlType.NativeType, sourceExpr))
                    return ConversionType.Implicit;
            }

            return Conversion(sourceSqlType.NativeType, targetSqlType.NativeType);
        }
        public      static      DataModel.ISqlType          OperationCalculation(Core.Token operation, Node.IExprNode expr1, Node.IExprNode expr2)
        {
            var sqlType1 = expr1.SqlType;
            var sqlType2 = expr2.SqlType;

            if (sqlType1 == null || sqlType2 == null)   return null;
            if (sqlType1 is DataModel.SqlTypeAny)       return sqlType1;
            if (sqlType2 is DataModel.SqlTypeAny)       return sqlType2;

            if (sqlType1 == sqlType2) {
                if ((sqlType1.TypeFlags & DataModel.SqlTypeFlags.Flags) != 0 && (operation.ID == Core.TokenID.BitOr || operation.ID == Core.TokenID.BitAnd || operation.ID == Core.TokenID.BitXor))
                    return sqlType1;
            }

            var nativeType1 = sqlType1.NativeType;
            var nativeType2 = sqlType2.NativeType;

            var rtn = expr1.isConstant()
                            ? expr2.isConstant()
                                ? _operation(operation.ID, nativeType1, nativeType2)
                                : _operator_CONST(operation.ID, nativeType2, expr1)
                            : expr2.isConstant()
                                ? _operator_CONST(operation.ID, nativeType1, expr2)
                                : _operation(operation.ID, nativeType1, nativeType2);

            if (rtn != null)
                return rtn;

            throw new ErrorException("Invalid operation '" + nativeType1.ToString() + " " + operation.ToString() + " " + nativeType2.ToString() + "'.");
        }
        public      static      void                        OperationCompare(Transpile.Context context, Core.Token operation, Node.IExprNode expr1, Node.IExprNode expr2)
        {
            if (!(expr1.isValid() && expr2.isValid()))
                return ;

            Validate.Value(expr1);
            Validate.Value(expr2);

            var sqlType1 = expr1.SqlType;
            var sqlType2 = expr2.SqlType;

            if (sqlType1 == null || sqlType1 is DataModel.SqlTypeAny ||
                sqlType2 == null || sqlType2 is DataModel.SqlTypeAny)
                return;

            if ((sqlType1.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 &&
                (sqlType2.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
            {
                var checkMode1 = sqlType1.getTypeCheckMode();
                var checkMode2 = sqlType2.getTypeCheckMode();

                if (checkMode1 == DataModel.SqlTypeFlags.CheckStrict || checkMode2 == DataModel.SqlTypeFlags.CheckStrict) {
                    if (sqlType1 == sqlType2)
                        return;

                    if (operation != null && (operation.ID == Core.TokenID.Equal || operation.ID == Core.TokenID.NotEqual)) {
                        if (_operationCompareFlagsCompareZero(sqlType1, expr2) || _operationCompareFlagsCompareZero(sqlType2, expr1))
                            return ;
                    }
                }
                else
                if (checkMode1 == DataModel.SqlTypeFlags.CheckStrong || checkMode2 == DataModel.SqlTypeFlags.CheckStrong) {
                    var const1 = expr1.isNullOrConstant();
                    var const2 = expr2.isNullOrConstant();

                    if (const1 && !const2 && Validate.ConstByType(sqlType2, expr1))
                        return;

                    if (!const1 && const2 && Validate.ConstByType(sqlType1, expr2))
                        return;

                    if (sqlType1 == sqlType2)
                        return;

                    if (operation != null && (operation.ID == Core.TokenID.Equal || operation.ID == Core.TokenID.NotEqual)) {
                        if (_operationCompareFlagsCompareZero(sqlType1, expr2) || _operationCompareFlagsCompareZero(sqlType2, expr1))
                            return ;
                    }

                    if (expr1.isComputedOrFunction() || expr2.isComputedOrFunction()) {
                        switch (Compare(sqlType1.NativeType, sqlType2.NativeType)) {
                        case CompareType.Save:
                            return;
                        }
                    }
                }
                else
                if (checkMode1 == DataModel.SqlTypeFlags.CheckSafe || checkMode2 == DataModel.SqlTypeFlags.CheckSafe) {
                    var const1 = expr1.isNullOrConstant();
                    var const2 = expr2.isNullOrConstant();

                    if (const1 && !const2 && Validate.ConstByType(sqlType2, expr1))
                        return;

                    if (!const1 && const2 && Validate.ConstByType(sqlType1, expr2))
                        return;

                    if (sqlType1 == sqlType2)
                        return;

                    switch (Compare(sqlType1.NativeType, sqlType2.NativeType)) {
                    case CompareType.Save:
                        return;
                    }
                }
                else {
                    var const1 = expr1.isNullOrConstant();
                    var const2 = expr2.isNullOrConstant();

                    if (const1 && !const2 && Validate.ConstByType(sqlType2, expr1))
                        return;

                    if (!const1 && const2 && Validate.ConstByType(sqlType1, expr2))
                        return;

                    if (sqlType1 == sqlType2)
                        return;

                    switch (Compare(sqlType1.NativeType, sqlType2.NativeType)) {
                    case CompareType.Save:
                    case CompareType.TSql:
                        return;
                    }
                }

                if (sqlType1.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || sqlType1.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict)
                    QuickFixLogic.QuickFix_Expr(context, sqlType1, expr2);

                if (sqlType2.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrong || sqlType2.getTypeCheckMode() == DataModel.SqlTypeFlags.CheckStrict)
                    QuickFixLogic.QuickFix_Expr(context, sqlType2, expr1);
            }

            if (sqlType1 == sqlType2)
                return ;

            throw new ErrorException("Can't compare " + sqlType1.ToString() + " with " + sqlType2.ToString() + ".");
        }
        public      static      FlagsTypeCollation          OperationUnion(Node.IExprNode[] expressions)
        {
            FlagsTypeCollation  rtn = new FlagsTypeCollation() { ValueFlags = DataModel.ValueFlags.None };

            foreach (var expr in expressions)
                rtn.ValueFlags |= LogicStatic.ComputedValueFlags(expr.ValueFlags);

            if ((rtn.ValueFlags & DataModel.ValueFlags.Error) == 0) {
                foreach (var expr in expressions) {
                    try {
                        if (!expr.ValueFlags.isNullOrConstant() || (expr.ValueFlags & DataModel.ValueFlags.Cast) != 0)
                            rtn.SqlType = TypeHelpers._typeUnion(rtn.SqlType, expr);
                    }
                    catch(Exception err) {
                        throw new TranspileException(expr, err.Message);
                    }
                }

                foreach (var expr in expressions) {
                    try {
                        if (expr.ValueFlags.isConstant())
                            rtn.SqlType = TypeHelpers._typeUnion(rtn.SqlType, expr);
                    }
                    catch(Exception err) {
                        throw new TranspileException(expr, err.Message);
                    }
                }

                if (rtn.SqlType == null)
                    throw new ErrorException("Can't determine union type because expression is NULL without a cast.");

                foreach (var expr in expressions) {
                    try {
                        if (expr.CollationName != null) {
                            if (rtn.CollationName == null)
                                rtn.CollationName = expr.CollationName;
                            else
                            if (rtn.CollationName != expr.CollationName)
                                throw new ErrorException("Ambigous collate '" + rtn.CollationName + "' or '" + expr.CollationName + "'.");
                        }
                    }
                    catch(Exception err) {
                        throw new TranspileException(expr, err.Message);
                    }
                }
            }

            return rtn;
        }
        public      static      ConversionType              Conversion(DataModel.SqlTypeNative sourceType, DataModel.SqlTypeNative targetType)
        {
            switch(sourceType.SystemType) {
            #region sourceNativeType = Bit
            case DataModel.SystemType.Bit:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Save;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Save;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Save;
                case DataModel.SystemType.Int:                      return ConversionType.Save;
                case DataModel.SystemType.BigInt:                   return ConversionType.Save;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = TinyInt
            case DataModel.SystemType.TinyInt:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Save;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Save;
                case DataModel.SystemType.Int:                      return ConversionType.Save;
                case DataModel.SystemType.BigInt:                   return ConversionType.Save;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = SmallInt
            case DataModel.SystemType.SmallInt:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Save;
                case DataModel.SystemType.Int:                      return ConversionType.Save;
                case DataModel.SystemType.BigInt:                   return ConversionType.Save;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Int
            case DataModel.SystemType.Int:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Save;
                case DataModel.SystemType.BigInt:                   return ConversionType.Save;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = BigInt
            case DataModel.SystemType.BigInt:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Save;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = SmallMoney
            case DataModel.SystemType.SmallMoney:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Save;
                case DataModel.SystemType.Money:                    return ConversionType.Save;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Money
            case DataModel.SystemType.Money:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Save;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Numeric
            case DataModel.SystemType.Numeric:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Decimal
            case DataModel.SystemType.Decimal:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Real
            case DataModel.SystemType.Real:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Save;
                case DataModel.SystemType.Float:                    return ConversionType.Save;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Float
            case DataModel.SystemType.Float:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Save;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Char
            case DataModel.SystemType.Char:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return (targetType.MaxLength == sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return (targetType.MaxLength == sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Implicit;
                case DataModel.SystemType.Time:                     return ConversionType.Implicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Save;
                case DataModel.SystemType.NText:                    return ConversionType.Save;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Explicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = NChar
            case DataModel.SystemType.NChar:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return (targetType.MaxLength == sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Implicit;
                case DataModel.SystemType.Time:                     return ConversionType.Implicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Implicit;
                case DataModel.SystemType.NText:                    return ConversionType.Save;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Explicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = VarChar
            case DataModel.SystemType.VarChar:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Implicit;
                case DataModel.SystemType.Time:                     return ConversionType.Implicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Implicit;
                case DataModel.SystemType.NText:                    return ConversionType.Implicit;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Explicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = NVarChar
            case DataModel.SystemType.NVarChar:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Implicit;
                case DataModel.SystemType.Time:                     return ConversionType.Implicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Implicit;
                case DataModel.SystemType.NText:                    return ConversionType.Implicit;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Explicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Binary
            case DataModel.SystemType.Binary:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return (targetType.MaxLength == sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.Time:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Explicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Explicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Explicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Explicit;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Implicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = VarBinary
            case DataModel.SystemType.VarBinary:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.Time:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Explicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Explicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Explicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Explicit;
                case DataModel.SystemType.Image:                    return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Implicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Implicit;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Date
            case DataModel.SystemType.Date:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Save;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Explicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Explicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Explicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Explicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Time
            case DataModel.SystemType.Time:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Time:                     return ConversionType.Save;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = SmallDateTime
            case DataModel.SystemType.SmallDateTime:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Save;
                case DataModel.SystemType.DateTime:                 return ConversionType.Save;
                case DataModel.SystemType.DateTime2:                return ConversionType.Save;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = DateTime
            case DataModel.SystemType.DateTime:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.SaveCalculated_Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Save;
                case DataModel.SystemType.DateTime2:                return (targetType.Scale >= 3) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = DateTime2
            case DataModel.SystemType.DateTime2:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return (targetType.Scale >= sourceType.Scale) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = DateTimeOffset
            case DataModel.SystemType.DateTimeOffset:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Date:                     return ConversionType.Explicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Text
            case DataModel.SystemType.Text:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return (targetType.MaxLength == -1) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Save;
                case DataModel.SystemType.NText:                    return ConversionType.Save;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = NText
            case DataModel.SystemType.NText:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return (targetType.MaxLength == -1) ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.Text:                     return ConversionType.Implicit;
                case DataModel.SystemType.NText:                    return ConversionType.Save;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Image
            case DataModel.SystemType.Image:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Binary:                   return targetType.MaxLength == sourceType.MaxLength                                 ? ConversionType.Save : ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return (targetType.MaxLength == -1 || targetType.MaxLength >= sourceType.MaxLength) ? ConversionType.Save : ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = UniqueIdentifier
            case DataModel.SystemType.UniqueIdentifier:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Save;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = SqlVariant
            case DataModel.SystemType.SqlVariant:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Bit:                      return ConversionType.Implicit;
                case DataModel.SystemType.TinyInt:                  return ConversionType.Implicit;
                case DataModel.SystemType.SmallInt:                 return ConversionType.Implicit;
                case DataModel.SystemType.Int:                      return ConversionType.Implicit;
                case DataModel.SystemType.BigInt:                   return ConversionType.Implicit;
                case DataModel.SystemType.SmallMoney:               return ConversionType.Implicit;
                case DataModel.SystemType.Money:                    return ConversionType.Implicit;
                case DataModel.SystemType.Numeric:                  return ConversionType.Implicit;
                case DataModel.SystemType.Decimal:                  return ConversionType.Implicit;
                case DataModel.SystemType.Real:                     return ConversionType.Implicit;
                case DataModel.SystemType.Float:                    return ConversionType.Implicit;
                case DataModel.SystemType.Char:                     return ConversionType.Implicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Implicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Implicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Implicit;
                case DataModel.SystemType.Date:                     return ConversionType.Implicit;
                case DataModel.SystemType.Time:                     return ConversionType.Implicit;
                case DataModel.SystemType.SmallDateTime:            return ConversionType.Implicit;
                case DataModel.SystemType.DateTime:                 return ConversionType.Implicit;
                case DataModel.SystemType.DateTime2:                return ConversionType.Implicit;
                case DataModel.SystemType.DateTimeOffset:           return ConversionType.Implicit;
                case DataModel.SystemType.UniqueIdentifier:         return ConversionType.Implicit;
                case DataModel.SystemType.SqlVariant:               return ConversionType.Save;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Timestamp
            case DataModel.SystemType.Timestamp:
                switch(targetType.SystemType) {
                case DataModel.SystemType.BigInt:                   return ConversionType.Explicit;
                case DataModel.SystemType.Char:                     return ConversionType.Explicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Explicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Explicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Explicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Image:                    return ConversionType.Explicit;
                case DataModel.SystemType.Timestamp:                return ConversionType.Save;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Xml
            case DataModel.SystemType.Xml:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Explicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Explicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Implicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Implicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                case DataModel.SystemType.Xml:                      return ConversionType.Save;
                case DataModel.SystemType.Clr:                      return ConversionType.Implicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            #region sourceNativeType = Clr
            case DataModel.SystemType.Clr:
                switch(targetType.SystemType) {
                case DataModel.SystemType.Char:                     return ConversionType.Explicit;
                case DataModel.SystemType.NChar:                    return ConversionType.Explicit;
                case DataModel.SystemType.VarChar:                  return ConversionType.Explicit;
                case DataModel.SystemType.NVarChar:                 return ConversionType.Explicit;
                case DataModel.SystemType.Binary:                   return ConversionType.Explicit;
                case DataModel.SystemType.VarBinary:                return ConversionType.Explicit;
                default:                                            return ConversionType.NotAllowed;
                }
            #endregion
            default:                                            return ConversionType.NotAllowed;
            }
        }
        public      static      CompareType                 Compare(DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            switch(nativeType1.SystemType) {
            case DataModel.SystemType.Bit:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return CompareType.Save;
                case DataModel.SystemType.TinyInt:          return CompareType.TSql;
                case DataModel.SystemType.SmallInt:         return CompareType.TSql;
                case DataModel.SystemType.Int:              return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return CompareType.TSql;
                case DataModel.SystemType.TinyInt:          return CompareType.Save;
                case DataModel.SystemType.Int:              return CompareType.Save;
                case DataModel.SystemType.SmallInt:         return CompareType.Save;
                case DataModel.SystemType.BigInt:           return CompareType.Save;
                case DataModel.SystemType.SmallMoney:       return CompareType.TSql;
                case DataModel.SystemType.Money:            return CompareType.TSql;
                case DataModel.SystemType.Numeric:          return CompareType.TSql;
                case DataModel.SystemType.Decimal:          return CompareType.TSql;
                case DataModel.SystemType.Real:             return CompareType.TSql;
                case DataModel.SystemType.Float:            return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return CompareType.TSql;
                case DataModel.SystemType.Int:              return CompareType.TSql;
                case DataModel.SystemType.SmallInt:         return CompareType.TSql;
                case DataModel.SystemType.BigInt:           return CompareType.TSql;
                case DataModel.SystemType.SmallMoney:       return CompareType.Save;
                case DataModel.SystemType.Money:            return CompareType.Save;
                case DataModel.SystemType.Numeric:          return CompareType.Save;
                case DataModel.SystemType.Decimal:          return CompareType.Save;
                case DataModel.SystemType.Real:             return CompareType.TSql;
                case DataModel.SystemType.Float:            return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return CompareType.TSql;
                case DataModel.SystemType.Int:              return CompareType.TSql;
                case DataModel.SystemType.SmallInt:         return CompareType.TSql;
                case DataModel.SystemType.BigInt:           return CompareType.TSql;
                case DataModel.SystemType.SmallMoney:       return CompareType.TSql;
                case DataModel.SystemType.Money:            return CompareType.TSql;
                case DataModel.SystemType.Numeric:          return CompareType.TSql;
                case DataModel.SystemType.Decimal:          return CompareType.TSql;
                case DataModel.SystemType.Real:             return CompareType.Save;
                case DataModel.SystemType.Float:            return CompareType.Save;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Char:             return CompareType.Save;
                case DataModel.SystemType.VarChar:          return CompareType.Save;
                case DataModel.SystemType.NChar:            return CompareType.TSql;
                case DataModel.SystemType.NVarChar:         return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.NChar:
            case DataModel.SystemType.NVarChar:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Char:             return CompareType.TSql;
                case DataModel.SystemType.VarChar:          return CompareType.TSql;
                case DataModel.SystemType.NChar:            return CompareType.Save;
                case DataModel.SystemType.NVarChar:         return CompareType.Save;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Binary:           return CompareType.Save;
                case DataModel.SystemType.VarBinary:        return CompareType.Save;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.Date:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Date:             return CompareType.Save;
                case DataModel.SystemType.SmallDateTime:    return CompareType.TSql;
                case DataModel.SystemType.DateTime:         return CompareType.TSql;
                case DataModel.SystemType.DateTime2:        return CompareType.TSql;
                case DataModel.SystemType.DateTimeOffset:   return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Date:             return CompareType.TSql;
                case DataModel.SystemType.SmallDateTime:    return CompareType.Save;
                case DataModel.SystemType.DateTime:         return CompareType.Save;
                case DataModel.SystemType.DateTime2:        return CompareType.Save;
                case DataModel.SystemType.DateTimeOffset:   return CompareType.TSql;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.Time:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Time:             return CompareType.Save;
                default:                                    return CompareType.NotAllowed;
                }

            case DataModel.SystemType.DateTimeOffset:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Date:             return CompareType.TSql;
                case DataModel.SystemType.SmallDateTime:    return CompareType.TSql;
                case DataModel.SystemType.DateTime:         return CompareType.TSql;
                case DataModel.SystemType.DateTime2:        return CompareType.TSql;
                case DataModel.SystemType.DateTimeOffset:   return CompareType.Save;
                default:                                    return CompareType.NotAllowed;
                }
            default:                                    return CompareType.NotAllowed;
            }
        }

        private     static      DataModel.ISqlType          _typeUnion(DataModel.ISqlType prevType, Node.IExprNode expr)
        {
            if (prevType == null)
                return expr.SqlType;

            if (prevType is DataModel.SqlTypeAny || expr.ValueFlags.isNull())
                return prevType;

            var exprSqlType = expr.SqlType;

            if (prevType == null || exprSqlType == null)
                return new DataModel.SqlTypeAny();

            if (object.ReferenceEquals(prevType, exprSqlType))
                return prevType;

            if (exprSqlType is DataModel.SqlTypeAny)
                return exprSqlType;

            var prevNativeType = prevType.NativeType;
            var exprNativeType = exprSqlType.NativeType;

            if (prevNativeType == exprNativeType)
                return prevNativeType;

            var rtn = expr.ValueFlags.isConstantAndNoCast()
                        ? _operator_CONST(Core.TokenID.UNION, prevNativeType, expr)
                        : _operation(Core.TokenID.UNION, prevNativeType, exprNativeType);

            if (rtn == null)
                throw new ErrorException("Can't unify '" + prevNativeType.ToString() + " and " + exprNativeType.ToString() + "'.");

            return rtn;
        }
        private     static      DataModel.SqlTypeNative     _operation(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            switch(nativeType1.SystemType) {
            case DataModel.SystemType.Bit:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return _operation_UNION(operation, nativeType1);
                case DataModel.SystemType.TinyInt:          return _operation_UNION(operation, nativeType2);
                case DataModel.SystemType.SmallInt:         return _operation_UNION(operation, nativeType2);
                case DataModel.SystemType.Int:              return _operation_UNION(operation, nativeType2);;
                case DataModel.SystemType.BigInt:           return _operation_UNION(operation, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.TinyInt:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return _operation_UNION(operation, nativeType1);
                case DataModel.SystemType.TinyInt:          return nativeType1;
                case DataModel.SystemType.SmallInt:         return nativeType2;
                case DataModel.SystemType.Int:              return nativeType2;
                case DataModel.SystemType.BigInt:           return nativeType2;
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.SmallInt:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return _operation_UNION(operation, nativeType1);
                case DataModel.SystemType.TinyInt:          return nativeType1;
                case DataModel.SystemType.SmallInt:         return nativeType1;
                case DataModel.SystemType.Int:              return nativeType2;
                case DataModel.SystemType.BigInt:           return nativeType2;
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Int:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Bit:              return _operation_UNION(operation, nativeType1);
                case DataModel.SystemType.TinyInt:          return nativeType1;
                case DataModel.SystemType.SmallInt:         return nativeType1;
                case DataModel.SystemType.Int:              return nativeType1;
                case DataModel.SystemType.BigInt:           return nativeType2;
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.BigInt:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return nativeType1;
                case DataModel.SystemType.SmallInt:         return nativeType1;
                case DataModel.SystemType.Int:              return nativeType1;
                case DataModel.SystemType.BigInt:           return nativeType1;
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.SmallMoney:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Int:              return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.SmallInt:         return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.BigInt:           return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Money:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Int:              return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.SmallInt:         return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.BigInt:           return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.SmallMoney:       return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_MONEY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Numeric:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Int:              return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.SmallInt:         return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.BigInt:           return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.SmallMoney:       return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Decimal:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.TinyInt:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Int:              return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.SmallInt:         return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.BigInt:           return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.SmallMoney:       return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Money:            return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Numeric:          return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType1, nativeType2);
                case DataModel.SystemType.Decimal:          return _operation_NUMERIC(operation, DataModel.SystemType.Decimal, nativeType1, nativeType2);
                case DataModel.SystemType.Real:             return _operation_FLOAT(operation, nativeType1, nativeType2);
                case DataModel.SystemType.Float:            return _operation_FLOAT(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Real:                 return _operation_FLOAT(operation, nativeType1, nativeType2);

            case DataModel.SystemType.Float:                return _operation_FLOAT(operation, nativeType1, nativeType2);

            case DataModel.SystemType.Char:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NVarChar:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Char:             return _operation_CHAR(operation, nativeType1, nativeType2);
                case DataModel.SystemType.NChar:            return _operation_CHAR(operation, nativeType1, nativeType2);
                case DataModel.SystemType.VarChar:          return _operation_CHAR(operation, nativeType1, nativeType2);
                case DataModel.SystemType.NVarChar:         return _operation_CHAR(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Binary:           return _operation_BINARY(operation, nativeType1, nativeType2);
                case DataModel.SystemType.VarBinary:        return _operation_BINARY(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            case DataModel.SystemType.Date:
            case DataModel.SystemType.Time:
            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
            case DataModel.SystemType.DateTimeOffset:
                switch(nativeType2.SystemType) {
                case DataModel.SystemType.Date:
                case DataModel.SystemType.Time:
                case DataModel.SystemType.SmallDateTime:
                case DataModel.SystemType.DateTime:
                case DataModel.SystemType.DateTime2:
                case DataModel.SystemType.DateTimeOffset:   return _operation_DATETIME(operation, nativeType1, nativeType2);
                default:                                    return null;
                }

            default:
                return null;
            }
        }
        private     static      DataModel.SqlTypeNative     _operation_MONEY(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            switch(operation) {
            case Core.TokenID.Plus:
            case Core.TokenID.Minus:
            case Core.TokenID.Star:
            case Core.TokenID.Divide:
            case Core.TokenID.UNION:
                if (nativeType1.SystemType == DataModel.SystemType.Money)       return nativeType1;
                if (nativeType2.SystemType == DataModel.SystemType.Money)       return nativeType2;
                if (nativeType1.SystemType == DataModel.SystemType.SmallMoney)  return nativeType1;
                if (nativeType2.SystemType == DataModel.SystemType.SmallMoney)  return nativeType2;
                break;
            }

            return null;
        }
        private     static      DataModel.SqlTypeNative     _operation_NUMERIC(Core.TokenID operation, DataModel.SystemType systemtype, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            _precisionScale ps1 = new _precisionScale(nativeType1);
            _precisionScale ps2 = new _precisionScale(nativeType2);
            int digits;
            int scale;

            switch(operation) {
            case Core.TokenID.Plus:
            case Core.TokenID.Minus:
                digits = Math.Max(ps1.Digits, ps2.Digits) + 1;
                scale  = Math.Max(ps1.Scale,  ps2.Scale);
                break;

            case Core.TokenID.Star:
                digits = ps1.Digits + ps2.Digits + 1;
                scale  = ps1.Scale + ps2.Scale;
                break;

            case Core.TokenID.Divide:
                digits = ps1.Digits + ps2.Scale;
                scale  = ps1.Scale + ps2.Precision + 1;
                break;

            case Core.TokenID.UNION:
                digits = Math.Max(ps1.Digits, ps2.Digits);
                scale  = Math.Max(ps1.Scale,  ps2.Scale);
                break;

            default:
                return null;
            }
            if (digits+scale > 38)
                scale = 38-digits;

            return new DataModel.SqlTypeNative(systemtype, precision:(byte)(digits+scale), scale:(byte)scale);
        }
        private     static      DataModel.SqlTypeNative     _operation_FLOAT(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            switch(operation) {
            case Core.TokenID.Plus:
            case Core.TokenID.Minus:
            case Core.TokenID.Star:
            case Core.TokenID.Divide:
            case Core.TokenID.UNION:
                if (nativeType1.SystemType == DataModel.SystemType.Float && nativeType1.Precision == 53)        return nativeType1;
                if (nativeType2.SystemType == DataModel.SystemType.Float && nativeType1.Precision == 53)        return nativeType2;
                if (nativeType1.SystemType == DataModel.SystemType.Real)                                        return nativeType1;
                if (nativeType2.SystemType == DataModel.SystemType.Real)                                        return nativeType2;

                return DataModel.SqlTypeNative.Float;
            }

            return null;
        }
        private     static      DataModel.SqlTypeNative     _operation_CHAR(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            if (operation == Core.TokenID.Plus || operation == Core.TokenID.UNION) {
                return DataModel.SqlTypeNative.NewString(nativeType1.isUnicode   || nativeType2.isUnicode,
                                                         nativeType1.isVarLength || nativeType2.isUnicode,
                                                         (nativeType1.MaxLength < 0 || nativeType2.MaxLength < 0)
                                                            ? -1
                                                            : (operation == Core.TokenID.Plus)
                                                                ? nativeType1.MaxLength + nativeType2.MaxLength
                                                                : Math.Max(nativeType1.MaxLength, nativeType2.MaxLength));
            }

            return null;
        }
        private     static      DataModel.SqlTypeNative     _operation_BINARY(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            if (operation == Core.TokenID.Plus || operation == Core.TokenID.UNION) {
                bool var   = nativeType1.SystemType == DataModel.SystemType.VarBinary || nativeType2.SystemType == DataModel.SystemType.VarBinary;
                int length = (nativeType1.MaxLength < 0 || nativeType2.MaxLength < 0)
                                ? -1
                                : (operation == Core.TokenID.Plus)
                                    ? nativeType1.MaxLength + nativeType2.MaxLength
                                    : Math.Max(nativeType1.MaxLength, nativeType2.MaxLength);

                if (length > 8000) {
                    var    = true;
                    length = -1;
                }

                return new DataModel.SqlTypeNative(var ? DataModel.SystemType.VarBinary : DataModel.SystemType.Binary,
                                                   maxLength:(short)length);
            }

            return null;
        }
        private     static      DataModel.SqlTypeNative     _operation_DATETIME(Core.TokenID operation, DataModel.SqlTypeNative nativeType1, DataModel.SqlTypeNative nativeType2)
        {
            if (operation == Core.TokenID.UNION) {
                switch(nativeType1.SystemType) {
                case DataModel.SystemType.Time:
                    switch(nativeType2.SystemType) {
                    case DataModel.SystemType.Time:             return nativeType1;
                    default:                                    return null;
                    }

                case DataModel.SystemType.Date:
                    switch(nativeType2.SystemType) {
                    case DataModel.SystemType.Date:             return nativeType1;
                    default:                                    return null;
                    }

                case DataModel.SystemType.SmallDateTime:
                    switch(nativeType2.SystemType) {
                    case DataModel.SystemType.SmallDateTime:    return nativeType1;
                    case DataModel.SystemType.DateTime:         return nativeType2;
                    case DataModel.SystemType.DateTime2:        return nativeType2;
                    default:                                    return null;
                    }
                case DataModel.SystemType.DateTime:
                    switch(nativeType2.SystemType) {
                    case DataModel.SystemType.SmallDateTime:    return nativeType1;
                    case DataModel.SystemType.DateTime:         return nativeType1;
                    case DataModel.SystemType.DateTime2:        return nativeType2;
                    default:                                    return null;
                    }

                case DataModel.SystemType.DateTime2:
                    switch(nativeType2.SystemType) {
                    case DataModel.SystemType.SmallDateTime:    return nativeType2;
                    case DataModel.SystemType.DateTime:         return nativeType2;
                    case DataModel.SystemType.DateTime2:        return nativeType2;
                    default:                                    return null;
                    }
                }
            }

            return null;
        }
        private     static      DataModel.SqlTypeNative     _operation_UNION(Core.TokenID operation, DataModel.SqlTypeNative nativeType)
        {
            return (operation == Core.TokenID.UNION) ? nativeType : null;
        }
        private     static      DataModel.SqlTypeNative     _operator_CONST(Core.TokenID operation, DataModel.SqlTypeNative nativeType, Node.IExprNode expr)
        {
            switch(nativeType.SystemType) {
            case DataModel.SystemType.Bit:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Int:
                    Validate.ConstByType(nativeType, expr);
                    return nativeType;

                default:
                    return null;
                }

            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.TinyInt:
                case DataModel.SystemType.SmallInt:
                case DataModel.SystemType.Int:
                    Validate.ConstByType(nativeType, expr);
                    return nativeType;

                case DataModel.SystemType.BigInt:       return null;
                case DataModel.SystemType.Numeric:      return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.Float:        return expr.SqlType.NativeType;
                default:                                return null;
                }

            case DataModel.SystemType.Int:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Int:          return nativeType;
                case DataModel.SystemType.BigInt:       return null;
                case DataModel.SystemType.Numeric:      return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.Float:        return expr.SqlType.NativeType;
                default:                                return null;
                }

            case DataModel.SystemType.BigInt:
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Int:          return nativeType;
                case DataModel.SystemType.BigInt:       return nativeType;
                case DataModel.SystemType.Numeric:      return _operation_NUMERIC(operation, DataModel.SystemType.Numeric, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.Float:        return expr.SqlType.NativeType;
                default:                                return null;
                }

            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Int:          return nativeType;
                case DataModel.SystemType.BigInt:       return nativeType;
                case DataModel.SystemType.Numeric:      return nativeType;
                case DataModel.SystemType.Float:        return nativeType;
                default:                                return null;
                }

            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.NVarChar:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Char:         return _operation_CHAR(operation, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.VarChar:      return _operation_CHAR(operation, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.NChar:        return _operation_CHAR(operation, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.NVarChar:     return _operation_CHAR(operation, nativeType, expr.SqlType.NativeType);
                default:                                return null;
                }

            case DataModel.SystemType.Binary:
            case DataModel.SystemType.VarBinary:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Binary:       return _operation_BINARY(operation, nativeType, expr.SqlType.NativeType);
                case DataModel.SystemType.VarBinary:    return _operation_BINARY(operation, nativeType, expr.SqlType.NativeType);
                default:                                return null;
                }

            case DataModel.SystemType.Date:
            case DataModel.SystemType.Time:
            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
            case DataModel.SystemType.DateTimeOffset:
                switch(expr.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Char:
                case DataModel.SystemType.VarChar:
                case DataModel.SystemType.NChar:
                case DataModel.SystemType.NVarChar:
                    Validate.ConstByType(nativeType, expr);
                    return nativeType;

                default:
                    return null;
                }

            default:
                return null;
            }
        }
        private     static      bool                        _operationCompareFlagsCompareZero(DataModel.ISqlType sqlType, Node.IExprNode expr)
        {
            if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.Flags) != 0) {
                if (expr.isConstant()) {
                    var v =  expr.ConstValue();

                    if (v is int && (int)v == 0)
                        return true;
                }
            }

            return false;
        }

        private struct _precisionScale
        {
            public      int         Precision;
            public      int         Scale;
            public      int         Digits              { get { return Precision - Scale; } }

            public                  _precisionScale(DataModel.SqlTypeNative nativeType)
            {
                switch(nativeType.SystemType) {
                case DataModel.SystemType.TinyInt:      Precision =  3; Scale = 0; break;
                case DataModel.SystemType.SmallInt:     Precision =  5; Scale = 0; break;
                case DataModel.SystemType.Int:          Precision = 10; Scale = 0; break;
                case DataModel.SystemType.BigInt:       Precision = 19; Scale = 0; break;
                case DataModel.SystemType.SmallMoney:   Precision = 10; Scale = 4; break;
                case DataModel.SystemType.Money:        Precision = 19; Scale = 4; break;
                case DataModel.SystemType.Numeric:      Precision = nativeType.Precision; Scale = nativeType.Scale;     break;
                case DataModel.SystemType.Decimal:      Precision = nativeType.Precision; Scale = nativeType.Scale;     break;
                default:                                throw new InvalidOperationException("Internal error invalid system_type " + nativeType.SystemType);
                }
            }
        }
    }
}
