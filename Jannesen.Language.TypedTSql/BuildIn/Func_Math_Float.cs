using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn
{
    public abstract class Func_Math: Func_Scalar
    {
        internal                                                    Func_Math(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType                  TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return DataModel.SqlTypeNative.Float;

            switch (sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                return TypeHelpers.ReturnStrictType(sqlType);
            default:
                return null;
            }
        }
    }

    public abstract class Func_Math_Float: Func_Scalar
    {
        internal                                                    Func_Math_Float(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType                  TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return DataModel.SqlTypeNative.Float;

            switch (sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                return DataModel.SqlTypeNative.Float;
            default:
                return null;
            }
        }
    }
}
