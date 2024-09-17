using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/translate-transact-sql
    public class TRANSLATE: Func_Scalar
    {
        internal                                            TRANSLATE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.ValueString(arguments[0]);
            Validate.ValueString(arguments[1]);
            Validate.ValueString(arguments[2]);

            return arguments[0].SqlType.NativeType;
        }
    }
}
