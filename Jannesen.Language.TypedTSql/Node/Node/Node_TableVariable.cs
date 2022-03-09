using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_TableVariable: Core.AstParseNode, IDataTarget
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public                  DataModel.SymbolUsageFlags      Usage           { get; private set; }
        public                  DataModel.Variable              Variable        { get; private set; }

        public                  bool                            isVarDeclare    { get { return false;                       } }
        public                  DataModel.ISymbol               Table           { get { return Variable;                    } }
        public                  DataModel.IColumnList           Columns         { get { return Variable?.SqlType?.Columns;  } }

        public                                                  Node_TableVariable(Core.ParserReader reader, DataModel.SymbolUsageFlags usage)
        {
            n_Name  = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
            Usage = usage;
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            Variable = null;

            var variable = context.VariableGet(n_Name);
            if (variable != null) {
                if ((variable.SqlType.TypeFlags & DataModel.SqlTypeFlags.Table) == 0) {
                    context.AddError(n_Name, "Variable is not a table variable.");
                    return;
                }

                n_Name.SetSymbolUsage(variable, Usage);
                Variable = variable;
            }
        }
        public                  void                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            Usage = usage;
            n_Name.SymbolData?.UpdateSymbolUsage(Variable, usage);
        }

        public                  DataModel.Column                GetColumnForAssign(string name, DataModel.ISqlType sqlType, string collationName, DataModel.ValueFlags flags, object declaration, DataModel.ISymbol nameReference, out bool declared)
        {
            declared = false;
            return Variable?.SqlType?.Columns?.FindColumn(name, out var _);
        }
    }
}
