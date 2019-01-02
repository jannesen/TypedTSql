using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/string-escape-transact-sql
    public class STRING_ESCAPE: Func_Scalar
    {
        internal                                            STRING_ESCAPE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2);
            Validate.ValueString(arguments[0]);
            Validate.ConstString(arguments[1]);

            var sqlType = arguments[0].SqlType;

            if (sqlType is DataModel.SqlTypeAny)
                return sqlType;

            switch(sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
                return DataModel.SqlTypeNative.VarChar_MAX;

            default:
                return DataModel.SqlTypeNative.NVarChar_MAX;
            }
        }
    }
}
