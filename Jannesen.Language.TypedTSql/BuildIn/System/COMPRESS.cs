using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/compress-transact-sql
    public class COMPRESS: Func_Scalar_TODO
    {
        internal                                            COMPRESS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (!(sqlType == null || sqlType is DataModel.SqlTypeAny)) {
                switch(sqlType.NativeType.SystemType) {
                case DataModel.SystemType.Char:
                case DataModel.SystemType.NChar:
                case DataModel.SystemType.VarChar:
                case DataModel.SystemType.NVarChar:
                case DataModel.SystemType.Binary:
                case DataModel.SystemType.VarBinary:
                    break;

                default:
                    throw new TranspileException(arguments[0], "Not a string/binary value.");
                }
            }

            return DataModel.SqlTypeNative.VarBinary_MAX;
        }
    }
}
