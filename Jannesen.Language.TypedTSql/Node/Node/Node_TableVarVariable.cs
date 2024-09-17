using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_TableVarVariable: Node_VarVariable, IDataTarget
    {
        public                                                  Node_TableVarVariable(Core.ParserReader reader): base(reader)
        {
        }

        public                  bool                            isVarDeclare    { get { return true;            } }
        public                  DataModel.ISymbol               Table           { get { return Variable.Symbol; } }
        public                  DataModel.IColumnList           Columns         { get { return _columns;        } }

        private                 DataModel.ColumnList            _columns;

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _columns = new DataModel.ColumnList(16);
            Variable = context.VarVariableSet(n_Name, n_Scope, new DataModel.SqlTypeTable(_columns, null));
            n_Name.SetSymbolUsage(Variable, DataModel.SymbolUsageFlags.Declaration | DataModel.SymbolUsageFlags.Insert);
        }

        public                  DataModel.Column                GetColumnForAssign(string name, DataModel.ISqlType sqlType, string collationName, DataModel.ValueFlags flags, object declaration, DataModel.ISymbol nameReference, out bool declared)
        {
            declared = false;
            if (!_columns.TryGetValue(name, out var column)) {
                _columns.Add(column = new DataModel.ColumnVarTable(name,
                                                                   sqlType,
                                                                   Variable.Symbol,
                                                                   declaration,
                                                                   collationName,
                                                                   nameReference,
                                                                   flags));
                declared = true;
            }

            return column;
        }
    }
}
