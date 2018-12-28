using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class TRIM : Func_Scalar
    {
        internal                                            TRIM(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return sqlType;

            switch(sqlType.NativeType.SystemType)
            {
            case DataModel.SystemType.VarChar:  return sqlType;
            case DataModel.SystemType.NVarChar: return sqlType;
            case DataModel.SystemType.Char:     return new DataModel.SqlTypeNative(DataModel.SystemType.VarChar, maxLength: sqlType.NativeType.MaxLength);
            case DataModel.SystemType.NChar:    return new DataModel.SqlTypeNative(DataModel.SystemType.NVarChar, maxLength: sqlType.NativeType.MaxLength);
            default:                            return null;
            }
        }
    }
}
