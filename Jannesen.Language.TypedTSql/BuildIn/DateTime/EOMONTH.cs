using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class EOMONTH: Func_Scalar
    {
        internal                                            EOMONTH(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);
            Validate.ValueDateTime(arguments[0], DatePartMode.Date);

            if (arguments.Length >= 2)
                Validate.ValueInt(arguments[1]);

            return DataModel.SqlTypeNative.Int;
        }
    }
}
