using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CHARINDEX: Func_Scalar
    {
        internal                                            CHARINDEX(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2, 3);
            Validate.ValueString(arguments[0]);
            Validate.ValueString(arguments[1]);

            if (arguments.Length > 2)
                Validate.ValueInt(arguments[2]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return DataModel.SqlTypeNative.Int;

            switch (sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
                return DataModel.SqlTypeNative.Int;
            default:
                return null;
            }

        }
    }
}
