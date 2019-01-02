using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_TableVariable: Core.AstParseNode, ITableSource
    {
        public      readonly    Core.TokenWithSymbol            n_Name;

        public                  DataModel.Variable              Variable            { get; private set; }

        public                                                  Node_TableVariable(Core.ParserReader reader)
        {
            n_Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            var variable = context.VariableGet(n_Name);
            if (variable != null) {
                if ((variable.SqlType.TypeFlags & DataModel.SqlTypeFlags.Table) == 0) {
                    context.AddError(n_Name, "Variable is not a table variable.");
                    return;
                }

                Variable = variable;
            }
        }

        public                  DataModel.ISymbol               getDataSource()
        {
            return Variable;
        }
        public                  DataModel.IColumnList           getColumnList(Transpile.Context context)
        {
            var sqlType = Variable?.SqlType;

            if (sqlType != null) {
                if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.Table) != 0) {
                    if (sqlType.Columns != null)
                        return sqlType.Columns;
                }
                else
                    context.AddError(n_Name, "Variable " + n_Name.Text + " is not a table variable.");
            }

            return new DataModel.ColumnListErrorStub();
        }
    }
}
