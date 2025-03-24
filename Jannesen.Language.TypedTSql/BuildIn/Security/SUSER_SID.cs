using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SUSER_SID: Func_Scalar
    {
        internal                                            SUSER_SID(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 0, 2);

            if (arguments.Length > 0) {
                Validate.ValueString(arguments[0]);
            }
            if (arguments.Length > 1) {
                Validate.ValueInt(arguments[1]);
            }

            return DataModel.SqlTypeNative.VarBinary_85;
        }
    }
}
