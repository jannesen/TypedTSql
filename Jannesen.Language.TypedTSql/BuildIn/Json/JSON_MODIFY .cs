using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/json-modify-transact-sql
    public class JSON_MODIFY: Func_Scalar
    {
        internal                                            JSON_MODIFY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.ValueString(arguments[0]);
            Validate.ConstString(arguments[1]);
            Validate.Value(arguments[2]);

            return DataModel.SqlTypeNative.NVarChar_MAX;
        }
    }
}
