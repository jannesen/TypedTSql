using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SUSER_SNAME: Func_Scalar
    {
        internal                                            SUSER_SNAME(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 0, 1);

            if (arguments.Length > 0) {
                Validate.ValueBinary(arguments[0]);
            }

            return DataModel.SqlTypeNative.NVarChar_128;
        }
    }
}
