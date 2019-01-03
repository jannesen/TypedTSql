using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class HAS_PERMS_BY_NAME: Func_Scalar
    {
        internal                                            HAS_PERMS_BY_NAME(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3, 5);
            Validate.ValueString(arguments[0]);
            Validate.ValueString(arguments[1]);
            Validate.ValueString(arguments[2]);

            if (arguments.Length >= 4)
                Validate.ValueString(arguments[3]);
            if (arguments.Length >= 5)
                Validate.ValueString(arguments[4]);

            return DataModel.SqlTypeNative.Int;
        }
    }
}
