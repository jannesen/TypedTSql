using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CONCAT: Func_Scalar
    {
        internal                                            CONCAT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2, 256);

            bool    var    = false;
            bool    n      = false;
            int     length = 0;

            for (int i = 0 ; i < arguments.Length ; ++i) {
                Validate.ValueString(arguments[i]);

                var sqlType = arguments[i].SqlType;
                if (sqlType is DataModel.SqlTypeAny)
                    return sqlType;
                var nativeType  = sqlType.NativeType;

                switch(nativeType.SystemType) {
                case DataModel.SystemType.Char:                                 break;
                case DataModel.SystemType.NChar:                    n = true;   break;
                case DataModel.SystemType.VarChar:      var = true;             break;
                case DataModel.SystemType.NVarChar:     var = true; n = true;   break;
                }

                length = (nativeType.MaxLength == -1) ? -1 : length + nativeType.MaxLength;
            }

            return DataModel.SqlTypeNative.NewString(n, var, length);
        }
    }
}
