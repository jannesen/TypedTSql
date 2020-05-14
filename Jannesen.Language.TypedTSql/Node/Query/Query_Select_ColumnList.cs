using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms176104.aspx
    //  Data_SelectList::=  { Data_SelectItem } [,...n]
    public class Query_Select_ColumnList: Core.AstParseNode
    {
        public      readonly        Query_Select_Column[]           n_Columns;

        public                                                      Query_Select_ColumnList(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            var columns = new List<Query_Select_Column>();

            do {
                if (selectContext == Query_SelectContext.ExpressionResponseObject || selectContext == Query_SelectContext.ExpressionResponseValue)
                    columns.Add(AddChild(new Query_Select_ColumnResponse(reader, selectContext)));
                else
                if (selectContext == Query_SelectContext.StatementInsertTargetNamed)
                    columns.Add(AddChild(new Query_Select_ColumnTargetNamed(reader)));
                else
                if ((selectContext == Query_SelectContext.StatementSelect || selectContext == Query_SelectContext.StatementReceive) && Query_Select_ColumnAssign.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnAssign(reader)));
                else
                if (selectContext != Query_SelectContext.StatementReceive && Query_Select_ColumnWildcard.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnWildcard(reader)));
                else
                    columns.Add(AddChild(new Query_Select_ColumnExpression(reader)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Columns = columns.ToArray();
        }

        public                      DataModel.ColumnListResult      GetResultSet(Transpile.Context context)
        {
            bool                        variableAssigment = false;
            List<DataModel.Column>      columnList        = null;

            foreach(var column in n_Columns) {
                if (column is Query_Select_ColumnAssign) {
                    variableAssigment = true;
                }
                else {
                    if (columnList == null)
                        columnList = new List<DataModel.Column>();

                    column.AddColumnToList(context, columnList);
                }

                if (variableAssigment && columnList != null)
                    throw new TranspileException(column, "Variable assignment and resultset not possible.");
            }

            return columnList != null ? new DataModel.ColumnListResult(columnList.ToArray()) : null;
        }
        public      override        void                            TranspileNode(Transpile.Context context)
        {
            n_Columns.TranspileNodes(context);
        }
    }
}
