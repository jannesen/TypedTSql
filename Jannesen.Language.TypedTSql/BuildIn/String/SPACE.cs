using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SPACE: Func_Scalar
    {
        internal                                            SPACE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);

            var olength = Validate.ValueInt(arguments[0], 1, 8000);

            return new DataModel.SqlTypeNative(DataModel.SystemType.VarChar,
                                                maxLength:(Int16)(olength != null ? (int)olength : 8000));
        }
    }
}
