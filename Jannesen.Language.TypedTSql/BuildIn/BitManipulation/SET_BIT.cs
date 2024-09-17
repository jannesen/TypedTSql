using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SET_BIT: Func_Scalar
    {
        internal                                            SET_BIT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2, 3);
            Validate.ValueIntBinary(arguments[0]);
            Validate.ValueInt(arguments[1]);

            if (arguments.Length > 2) {
                Validate.ValueBit(arguments[1]);
            }

            return arguments[0].SqlType.NativeType;
         }
    }
}
