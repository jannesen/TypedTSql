using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class RIGHT: Func_Scalar
    {
        internal                                            RIGHT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2);
            Validate.ValueString(arguments[0]);

            var sqlType = arguments[0].SqlType;

            if (sqlType is DataModel.SqlTypeAny) {
                Validate.ValueInt(arguments[1]);
                return sqlType;
            }

            var nativeType = sqlType.NativeType;

            var olength = Validate.ValueInt(arguments[1], 1, (nativeType.MaxLength > 0 ? nativeType.MaxLength : int.MaxValue));

            if (olength is int)
                return new DataModel.SqlTypeNative(sqlType.NativeType.SystemType, maxLength:(int)olength);

            return sqlType.NativeType;
        }
    }
}
