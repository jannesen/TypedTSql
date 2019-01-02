using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class ROUND: Func_Scalar
    {
        internal                                            ROUND(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2, 3);
            Validate.Value(arguments[0]);
            Validate.ValueInt(arguments[1]);

            if (arguments.Length >= 3)
                Validate.ValueInt(arguments[2], 0, 1);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return sqlType;

            var nativeType = sqlType.NativeType;

            switch (nativeType.SystemType) {
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
                Validate.ValueInt(arguments[1], -10, 0);
                return TypeHelpers.ReturnStrictType(sqlType);

            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
                Validate.ValueInt(arguments[1], -6, 3);
                return TypeHelpers.ReturnStrictType(sqlType);

            case DataModel.SystemType.Real:
            case DataModel.SystemType.Float:
                Validate.ValueInt(arguments[1], -14, 14);
                return TypeHelpers.ReturnStrictType(sqlType);

            case DataModel.SystemType.Numeric:
            case DataModel.SystemType.Decimal:
                object v = Validate.ValueInt(arguments[1], -30, nativeType.Scale);

                if (v != null) {
                    int s = Math.Max(0, (int)v);
                    return new DataModel.SqlTypeNative(nativeType.SystemType, precision:(byte)(nativeType.Precision - nativeType.Scale + s), scale:(byte)s);
                }
                else
                    return nativeType;

            default:
                return null;
            }
        }
    }
}
