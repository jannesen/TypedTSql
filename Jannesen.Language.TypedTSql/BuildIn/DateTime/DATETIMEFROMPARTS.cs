using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class DATETIMEFROMPARTS: Func_Scalar
    {
        internal                                            DATETIMEFROMPARTS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 7);
            Validate.ValueInt(arguments[0]);
            Validate.ValueInt(arguments[1]);
            Validate.ValueInt(arguments[2]);
            Validate.ValueInt(arguments[3]);
            Validate.ValueInt(arguments[4]);
            Validate.ValueInt(arguments[5]);
            Validate.ValueInt(arguments[6]);

            return DataModel.SqlTypeNative.DateTime;
        }
    }
}
