using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class FORMATMESSAGE: Func_Scalar
    {
        internal                                            FORMATMESSAGE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 21);
            Validate.ValueString(arguments[0]);

            for (int i=1 ; i < arguments.Length ; ++i)
                Validate.Value(arguments[i]);

            return DataModel.SqlTypeNative.NVarChar_4000;
        }
    }
}
