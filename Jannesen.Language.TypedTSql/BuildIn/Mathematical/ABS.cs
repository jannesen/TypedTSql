using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class ABS: Func_Scalar
    {
        internal                                            ABS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return sqlType;

            switch (sqlType.NativeType.SystemType) {
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
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
}
