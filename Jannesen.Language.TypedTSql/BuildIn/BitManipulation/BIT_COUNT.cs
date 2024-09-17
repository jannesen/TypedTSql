using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class BIT_COUNT: Func_Scalar
    {
        internal                                            BIT_COUNT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.ValueIntBinary(arguments[0]);

            switch(arguments[0].SqlType.NativeType.SystemType) {
            default:
                return DataModel.SqlTypeNative.Int;
            case DataModel.SystemType.VarBinary:
                return DataModel.SqlTypeNative.BigInt;
            }
        }
    }
}
