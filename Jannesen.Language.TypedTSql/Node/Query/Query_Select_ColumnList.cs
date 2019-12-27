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
                if (selectContext == Query_SelectContext.StatementSelect && Query_Select_ColumnAssign.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnAssign(reader)));
                else
                if (Query_Select_ColumnWildcard.CanParse(reader))
                    columns.Add(AddChild(new Query_Select_ColumnWildcard(reader)));
                else
                    columns.Add(AddChild(new Query_Select_ColumnExpression(reader)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Columns = columns.ToArray();
        }

        public      override        void                            TranspileNode(Transpile.Context context)
        {
            n_Columns.TranspileNodes(context);
        }
    }
}
