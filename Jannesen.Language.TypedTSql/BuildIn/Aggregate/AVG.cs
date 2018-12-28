using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class AVG: Func_Aggragate
    {
        internal                                                    AVG(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override            DataModel.ISqlType          TranspileReturnType(DataModel.ISqlType sqlType)
        {
            switch(sqlType.NativeType.SystemType)
            {
            case DataModel.SystemType.TinyInt:
            case DataModel.SystemType.SmallInt:
            case DataModel.SystemType.Int:
            case DataModel.SystemType.BigInt:
            case DataModel.SystemType.SmallMoney:
            case DataModel.SystemType.Money:
            case DataModel.SystemType.Decimal:
            case DataModel.SystemType.Numeric:
                return sqlType.NativeType;

            case DataModel.SystemType.Real:
                return DataModel.SqlTypeNative.Float;

            case DataModel.SystemType.Float:
                return sqlType;

            default:
                return null;
            }
        }
    }
}
