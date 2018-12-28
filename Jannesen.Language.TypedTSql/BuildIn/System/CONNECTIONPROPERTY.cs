using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CONNECTIONPROPERTY: Func_Scalar
    {
        internal                                            CONNECTIONPROPERTY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.ValueString(arguments[0]);

            if (arguments[0].isConstant()) {
                var property = arguments[0].ConstValue() as string;

                switch(property)
                {
                case "net_transport":           return DataModel.SqlTypeNative.NVarChar_40;
                case "protocol_type":           return DataModel.SqlTypeNative.NVarChar_40;
                case "auth_scheme":             return DataModel.SqlTypeNative.NVarChar_40;
                case "local_net_address":       return DataModel.SqlTypeNative.VarChar_48;
                case "local_tcp_port":          return DataModel.SqlTypeNative.Int;
                case "client_net_address":      return DataModel.SqlTypeNative.VarChar_48;
                case "physical_net_transport":  return DataModel.SqlTypeNative.NVarChar_40;
                case null:                      return new DataModel.SqlTypeAny();
                default:                        throw new ErrorException("Unknown property '" + property + "'.");
                }
            }

            return new DataModel.SqlTypeAny();
        }
    }
}
