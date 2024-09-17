using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/isjson-transact-sql
    public class ISJSON: Func_Scalar
    {
        internal                                            ISJSON(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);
            Validate.ValueString(arguments[0]);
            if (arguments.Length > 1) {
                switch(Validate.ConstString(arguments[1])?.ToUpperInvariant()) {
                case "VALUE":
                case "ARRAY":
                case "OBJECT":
                case "SCALAR":
                    break;

                default:
                    throw new TranspileException(arguments[1], "Invalid value expect VALUE, ARRAY, OBJECT, SCALAR");
                }
            }
            return DataModel.SqlTypeNative.Int;
        }
    }
}
