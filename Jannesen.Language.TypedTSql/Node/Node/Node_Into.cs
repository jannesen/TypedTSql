using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_Into: AstParseNode, IDataTarget
    {
        public      readonly    Core.TokenWithSymbol            n_Into;

        public                                                  Node_Into(Core.ParserReader reader)
        {
            n_Into = ParseName(reader);
        }

        public                  bool                            isVarDeclare    { get { return false;           } }
        public                  DataModel.ISymbol               Table           { get { return _tempTable;      } }
        public                  DataModel.IColumnList           Columns         { get { return _columns;        } }

        private                 DataModel.ColumnList            _columns;
        private                 DataModel.TempTable             _tempTable;

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _columns   = new DataModel.ColumnList(16);
            _tempTable = null;

            if (!context.GetDeclarationObjectCode().Entity.TempTableAdd(n_Into.ValueString, n_Into, _columns, null, out var tempTable)) {
                context.AddError(n_Into, "Temp table already defined at a differend location.");
            }

            _tempTable = tempTable;

            n_Into.SetSymbolUsage(tempTable, DataModel.SymbolUsageFlags.Declaration | DataModel.SymbolUsageFlags.Insert);
        }

        public                  DataModel.Column                GetColumnForAssign(string name, DataModel.ISqlType sqlType, string collationName, DataModel.ValueFlags flags, object declaration, DataModel.ISymbol nameReference, out bool declared)
        {
            declared = false;
            if (!_columns.TryGetValue(name, out var column)) {
                _columns.Add(column = new DataModel.ColumnVarTable(name,
                                                                   sqlType,
                                                                   _tempTable,
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
