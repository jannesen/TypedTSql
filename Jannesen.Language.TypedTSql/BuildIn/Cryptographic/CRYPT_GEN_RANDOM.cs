using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CRYPT_GEN_RANDOM: Func_Scalar
    {
        internal                                                    CRYPT_GEN_RANDOM(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType                  TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);

            object olength = Validate.ValueInt(arguments[0], 1, 8000);

            if (arguments.Length >= 2)
                Validate.ValueBinary(arguments[1]);

            return (olength != null)
                ? new DataModel.SqlTypeNative(DataModel.SystemType.Binary, maxLength:(int)olength)
                : new DataModel.SqlTypeNative(DataModel.SystemType.VarBinary, maxLength:8000);
        }
    }
}
