using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class QUOTENAME: Func_Scalar
    {
        internal                                            QUOTENAME(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);

            Validate.ValueString(arguments[0]);

            if (arguments.Length >= 2)
                Validate.ValueString(arguments[1]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return DataModel.SqlTypeNative.NVarChar_258;

            switch (arguments[0].SqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
            case DataModel.SystemType.NChar:
            case DataModel.SystemType.NVarChar:
                if (arguments[0].SqlType.NativeType.MaxLength <= 256)
                    return DataModel.SqlTypeNative.NVarChar_258;
                return null;
            default:
                return null;
            }
        }
    }
}
