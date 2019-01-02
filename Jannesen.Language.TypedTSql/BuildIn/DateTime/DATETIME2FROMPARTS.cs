using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class DATETIME2FROMPARTS: Func_Scalar
    {
        internal                                            DATETIME2FROMPARTS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 8);
            Validate.ValueInt(arguments[0]);
            Validate.ValueInt(arguments[1]);
            Validate.ValueInt(arguments[2]);
            Validate.ValueInt(arguments[3]);
            Validate.ValueInt(arguments[4]);
            Validate.ValueInt(arguments[5]);
            Validate.ValueInt(arguments[6]);
            Validate.ValueInt(arguments[7]);

            return DataModel.SqlTypeNative.DateTime2;
        }
    }
}
