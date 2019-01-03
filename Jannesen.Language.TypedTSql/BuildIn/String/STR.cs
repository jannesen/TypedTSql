using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class STR: Func_Scalar
    {
        internal                                            STR(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 3);
            Validate.ValueNumber(arguments[0]);

            object olength = null;

            if (arguments.Length >= 2)
                olength = Validate.ValueInt(arguments[1], 1, 30);

            if (arguments.Length >= 3)
                Validate.ValueInt(arguments[2], 1, 16);

            return new DataModel.SqlTypeNative(DataModel.SystemType.VarChar, maxLength: (olength != null ? (int)olength : 8000));
        }
    }
}
