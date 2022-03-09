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

        public                      DataModel.ColumnListResult      ResultColumnList        { get; private set; }

        public                                                      Query_Select_ColumnList(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            var columns = new List<Query_Select_Column>();

            do {
                if (selectContext == Query_SelectContext.ExpressionResponseObject ||
                    selectContext == Query_SelectContext.ExpressionResponseValue)
                    columns.Add(AddChild(new Query_Select_ColumnResponse(reader, selectContext)));
                else
                if ((selectContext == Query_SelectContext.StatementSelect ||
                     selectContext == Query_SelectContext.StatementReceive) &&
                     Query_Select_ColumnVariableAssign.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnVariableAssign(reader)));
                else
                if (selectContext != Query_SelectContext.StatementReceive &&
                    selectContext != Query_SelectContext.StatementStore &&
                    Query_Select_ColumnWildcard.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnWildcard(reader, selectContext)));
                else
                    columns.Add(AddChild(new Query_Select_ColumnExpression(reader, selectContext)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Columns = columns.ToArray();
        }

        public      override        void                            TranspileNode(Transpile.Context context)
        {
            ResultColumnList = null;

            n_Columns.TranspileNodes(context);

            bool                        variableAssigment = false;
            List<DataModel.Column>      columnList        = null;

            foreach(var column in n_Columns) {
                if (column.isVariableAssign) {
                    variableAssigment = true;
                }
                else {
                    if (columnList == null)
                        columnList = new List<DataModel.Column>();

                    column.AddColumnToList(context, columnList);
                }

                if (variableAssigment && columnList != null) { 
                    throw new TranspileException(column, "Variable assignment and resultset not possible.");
                }
            }

            if (columnList != null) {
                ResultColumnList = new DataModel.ColumnListResult(columnList.ToArray());
            }
        }
    }
}
