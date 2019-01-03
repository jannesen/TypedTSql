using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class IS_SRVROLEMEMBER: IS_MEMBER
    {
        internal                                            IS_SRVROLEMEMBER(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);
            Validate.ValueString(arguments[0]);
            if (arguments.Length > 1)
                Validate.ValueString(arguments[1]);
            return DataModel.SqlTypeNative.Int;
        }
    }
}
