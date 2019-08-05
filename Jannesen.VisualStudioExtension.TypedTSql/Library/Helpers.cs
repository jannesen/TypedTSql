using System;
using System.Text;
using LTTS = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Library
{
    public static class Helpers
    {
        private     static      char[]                  nibbleToHex = new char[] { '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' };

        public static       T           GetService<T>(this IServiceProvider seviceProvider, Type type) where T: class
        {
            var rtn = seviceProvider.GetService(type) as T;

            if (rtn == null) { 
                throw new InvalidOperationException("Can't get service '" + type.FullName + "'.");
            }

            return rtn;
        }

        public  static      string      SymbolTypeToString(LTTS.DataModel.SymbolType type)
        {
            switch(type) {
            case LTTS.DataModel.SymbolType.Assembly:                            return "assembly";
            case LTTS.DataModel.SymbolType.TypeUser:                            return "user-defined-type";
            case LTTS.DataModel.SymbolType.TypeExternal:                        return "clr-type";
            case LTTS.DataModel.SymbolType.TypeTable:                           return "table-type";
            case LTTS.DataModel.SymbolType.Default:                             return "default";
            case LTTS.DataModel.SymbolType.Rule:                                return "rule";
            case LTTS.DataModel.SymbolType.TableInternal:                       return "internal-table";
            case LTTS.DataModel.SymbolType.TableSystem:                         return "system-table";
            case LTTS.DataModel.SymbolType.TableUser:                           return "table";
            case LTTS.DataModel.SymbolType.Constraint_ForeignKey:               return "foreignkey-constraint";
            case LTTS.DataModel.SymbolType.Constraint_PrimaryKey:               return "primarykey-constraint";
            case LTTS.DataModel.SymbolType.Constraint_Check:                    return "check-constaint";
            case LTTS.DataModel.SymbolType.Constraint_Unique:                   return "unique-constraint";
            case LTTS.DataModel.SymbolType.View:                                return "view";
            case LTTS.DataModel.SymbolType.Function:                            return "function";
            case LTTS.DataModel.SymbolType.FunctionScalar:                      return "scalar-function";
            case LTTS.DataModel.SymbolType.FunctionScalar_clr:                  return "scalar-function(clr)";
            case LTTS.DataModel.SymbolType.FunctionInlineTable:                 return "inline-table-function";
            case LTTS.DataModel.SymbolType.FunctionMultistatementTable:         return "table-function";
            case LTTS.DataModel.SymbolType.FunctionMultistatementTable_clr:     return "table-function(clr)";
            case LTTS.DataModel.SymbolType.FunctionAggregateFunction_clr:       return "aggregate-function(clr)";
            case LTTS.DataModel.SymbolType.StoredProcedure:                     return "stored-procedure";
            case LTTS.DataModel.SymbolType.StoredProcedure_clr:                 return "stored-procedure(clr)";
            case LTTS.DataModel.SymbolType.StoredProcedure_extended:            return "stored-procedure(native)";
            case LTTS.DataModel.SymbolType.Trigger:                             return "trigger";
            case LTTS.DataModel.SymbolType.Trigger_clr:                         return "trigger(clr)";
            case LTTS.DataModel.SymbolType.PlanGuide:                           return "plan-guide";
            case LTTS.DataModel.SymbolType.ReplicationFilterProcedure:          return "replication-filter-procedure";
            case LTTS.DataModel.SymbolType.SequenceObject:                      return "sequence-object";
            case LTTS.DataModel.SymbolType.ServiceQueue:                        return "service-queue";
            case LTTS.DataModel.SymbolType.Synonym:                             return "synonym";
            case LTTS.DataModel.SymbolType.Service:                             return "service";
            case LTTS.DataModel.SymbolType.ServiceMethod:                       return "service-method";
            case LTTS.DataModel.SymbolType.Parameter:                           return "parameter";
            case LTTS.DataModel.SymbolType.Column:                              return "column";
            case LTTS.DataModel.SymbolType.Index:                               return "index";
            case LTTS.DataModel.SymbolType.DatabasePrincipal:                   return "database-principal";
            case LTTS.DataModel.SymbolType.GlobalVariable:                      return "global-variable";
            case LTTS.DataModel.SymbolType.LocalVariable:                       return "variable";
            case LTTS.DataModel.SymbolType.BuildinFunction:                     return "buildin-function";
            case LTTS.DataModel.SymbolType.Label:                               return "label";
            case LTTS.DataModel.SymbolType.Cursor:                              return "cursor";
            case LTTS.DataModel.SymbolType.TempTable:                           return "temp-table";
            case LTTS.DataModel.SymbolType.UDTValue:                            return "udt-value";
            default:                                                            return type.ToString();
            }
        }
        public  static      string      ObjectValueToString(object value)
        {
            if (value == null)      return "NULL";
            if (value is string)    return LTTS.Library.SqlStatic.QuoteString((string)value);
            if (value is int)       return ((int)value).ToString();
            if (value is decimal)   return ((decimal)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            if (value is double)    return ((int)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            if (value is byte[] v) {
                StringBuilder   str = new StringBuilder(4 + v.Length*2);

                str.Append("0x");

                for (int i=0 ; i < v.Length ; ++i) {
                    byte b = v[i];
                    str.Append(nibbleToHex[(b >> 4) & 0xF]);
                    str.Append(nibbleToHex[(b     ) & 0xF]);
                }

                return str.ToString();
            }

            return value.GetType().FullName;
        }
    }
}
