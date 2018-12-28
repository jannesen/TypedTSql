using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class DATEFROMPARTS: Func_Scalar
    {
        internal                                            DATEFROMPARTS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.ValueInt(arguments[0]);
            Validate.ValueInt(arguments[1]);
            Validate.ValueInt(arguments[2]);

            return DataModel.SqlTypeNative.Date;
        }
    }
}
