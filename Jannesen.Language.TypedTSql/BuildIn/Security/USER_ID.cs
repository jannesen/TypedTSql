using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class USER_ID: Func_Scalar
    {
        internal                                            USER_ID(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 0, 1);

            if (arguments.Length >= 1)
                Validate.ValueString(arguments[0]);

            return DataModel.SqlTypeNative.Int;
        }
    }
}
