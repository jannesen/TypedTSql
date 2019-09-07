using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_TableVarVariable: Node_VarVariable, ITableSource
    {
        public                  DataModel.Variable              Variable            { get; private set; }

        public                                                  Node_TableVarVariable(Core.ParserReader reader): base(reader)
        {
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
        }
        public                  void                            TranspileInsert(Transpile.Context context, DataModel.IColumnList columns)
        {
            if (columns == null) {
                context.AddError(this, "Can't determin columns.");
                columns = new DataModel.ColumnListErrorStub();
            }

            Variable = context.VarVariableSet(n_Name, new DataModel.SqlTypeTable(columns, null));
        }

        public                  DataModel.ISymbol               getDataSource()
        {
            return Variable;
        }
        public                  DataModel.IColumnList           getColumnList(Transpile.Context context)
        {
            return Variable.SqlType.Columns;
        }
    }
}
