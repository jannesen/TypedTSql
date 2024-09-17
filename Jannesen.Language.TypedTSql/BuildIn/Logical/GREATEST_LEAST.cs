using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class GREATEST_LEAST: Func_Scalar
    {
        internal                                            GREATEST_LEAST(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    FlagsTypeCollation          TranspileResult(IExprNode[] arguments)
        {
            if (arguments.Length == 0) {
                throw new ErrorException("Invalid number of arguments. Expect at least 1 argument.");
            }

            foreach (var arg in arguments) {
                Validate.ValueNumber(arg);
            }

            return TypeHelpers.OperationUnion(arguments);
        }
    }
}
