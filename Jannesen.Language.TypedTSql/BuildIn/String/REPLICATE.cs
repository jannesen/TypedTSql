using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class REPLICATE: Func_Scalar
    {
        internal                                            REPLICATE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2);
            Validate.ValueString(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return sqlType;

            var nativeType = sqlType.NativeType;
            var olength = Validate.ValueInt(arguments[1], 1, 8000);

            if (nativeType.MaxLength == -1 || nativeType.MaxLength == DataModel.SqlTypeNative.SystemTypeMaxLength(nativeType.SystemType))
                return nativeType;

            return new DataModel.SqlTypeNative((nativeType.isUnicode ? DataModel.SystemType.NVarChar : DataModel.SystemType.VarChar),
                                               maxLength:(olength != null
                                                            ? nativeType.MaxLength * ((int)olength)
                                                            : DataModel.SqlTypeNative.SystemTypeMaxLength(nativeType.SystemType)));
        }
    }
}
