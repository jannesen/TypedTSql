using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.Library
{
    public static class SqlStatic
    {
        public  static          string                  QuoteName(string s)
        {
            return "[" + s.Replace("]", "[]") + "]";
        }
        public  static          string                  QuoteNameIfNeeded(string s)
        {
            return ValidName(s) ? s : QuoteName(s);
        }
        public  static          string                  QuoteString(string s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }
        public  static          string                  QuoteNString(string s)
        {
            return "N'" + s.Replace("'", "''") + "'";
        }

        public  static          bool                    ValidName(string s)
        {
            if (s.Length == 0)
                return false;

            for (int i = 0 ; i < s.Length ; ++i) {
                char c = s[i];

                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||  (c >= '0' && c <= '9' && i > 0) || c == '_'))
                    return false;
            }

            return true;
        }

        internal    static      DataModel.SymbolType    ParseObjectType(string stype)
        {
            switch(stype) {
            case "AF":          return DataModel.SymbolType.FunctionAggregateFunction_clr;
            case "C ":          return DataModel.SymbolType.Constraint_Check;
            case "D ":          return DataModel.SymbolType.Default;
            case "F ":          return DataModel.SymbolType.Constraint_ForeignKey;
            case "FN":          return DataModel.SymbolType.FunctionScalar;
            case "FS":          return DataModel.SymbolType.FunctionScalar_clr;
            case "FT":          return DataModel.SymbolType.FunctionMultistatementTable_clr;
            case "IF":          return DataModel.SymbolType.FunctionInlineTable;
            case "IT":          return DataModel.SymbolType.TableInternal;
            case "P ":          return DataModel.SymbolType.StoredProcedure;
            case "PC":          return DataModel.SymbolType.StoredProcedure_clr;
            case "PG":          return DataModel.SymbolType.PlanGuide;
            case "PK":          return DataModel.SymbolType.Constraint_PrimaryKey;
            case "R ":          return DataModel.SymbolType.Rule;
            case "RF":          return DataModel.SymbolType.ReplicationFilterProcedure;
            case "S ":          return DataModel.SymbolType.TableSystem;
            case "SN":          return DataModel.SymbolType.Synonym;
            case "SO":          return DataModel.SymbolType.SequenceObject;
            case "SQ":          return DataModel.SymbolType.ServiceQueue;
            case "TA":          return DataModel.SymbolType.Trigger_clr;
            case "TF":          return DataModel.SymbolType.FunctionMultistatementTable;
            case "TR":          return DataModel.SymbolType.Trigger;
            case "TT":          return DataModel.SymbolType.TypeTable;
            case "U ":          return DataModel.SymbolType.TableUser;
            case "UQ":          return DataModel.SymbolType.Constraint_Unique;
            case "V ":          return DataModel.SymbolType.View;
            case "X ":          return DataModel.SymbolType.StoredProcedure_extended;
            default:            throw new NotSupportedException("Unknown object-type '" + stype + "'.");
            }
        }
    }
}
