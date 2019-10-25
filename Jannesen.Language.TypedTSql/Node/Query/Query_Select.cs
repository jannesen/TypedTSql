using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum Query_SelectContext
    {
        StatementSelect         = 1,
        StatementInsert,
        StatementDeclareCursor,
        StatementView,
        FunctionInlineTable,
        TableSourceSubquery,
        ExpressionSubquery,
        ExpressionEXISTS,
        ExpressionResponseObject,
        ExpressionResponseValue
    }

    // https://msdn.microsoft.com/en-us/library/ms189499.aspx
    public class Query_Select: Core.AstParseNode
    {
        public      readonly    Query_Select_SELECT[]       n_Selects;
        public      readonly    Query_Select_OrderBy        n_OrderBy;
        public      readonly    Query_Select_FOR            n_For;

        public                  DataModel.IColumnList       Resultset               { get; private set; }
        public                  bool                        VariableAssigment       { get; private set; }

        public                                              Query_Select(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            {
                var selects = new List<Query_Select_SELECT> {  { AddChild(new Query_Select_SELECT(reader, selectContext)) }  };

                if (selectContext != Query_SelectContext.ExpressionEXISTS         &&
                    selectContext != Query_SelectContext.ExpressionResponseObject &&
                    selectContext != Query_SelectContext.ExpressionResponseObject)
                {
                    while (ParseOptionalToken(reader, Core.TokenID.UNION) != null) {
                        ParseOptionalToken(reader, Core.TokenID.ALL);
                        selects.Add(AddChild(new Query_Select_SELECT(reader, selectContext)));
                    }
                }

                n_Selects = selects.ToArray();
            }

            if (selectContext != Query_SelectContext.ExpressionEXISTS) {
                if (reader.CurrentToken.isToken(Core.TokenID.ORDER)) {
                    n_OrderBy = AddChild(new Query_Select_OrderBy(reader));
                }
            }

            if (selectContext == Query_SelectContext.StatementSelect || selectContext == Query_SelectContext.ExpressionSubquery) {
                if (reader.CurrentToken.isToken(Core.TokenID.FOR)) {
                    n_For = AddChild(new Query_Select_FOR(reader));
                }
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            Resultset         = null;
            VariableAssigment = false;

            try {
                Resultset = (n_Selects.Length == 1) ? _transpileNode_Single(context) : _transpileNode_Union(context);

                n_For?.TranspileNode(context);

                if (n_For != null) {
                    if (Resultset != null)
                        Resultset = new DataModel.ColumnListResult(new DataModel.Column[] { new DataModel.ColumnDS("", n_For.ResultType) } );
                    else
                        context.AddError(n_For, "FOR not allowed.");
                }
            }
            catch(Exception err) {
                Resultset         = null;
                VariableAssigment = false;
                context.AddError(this, err);
            }
        }

        private                 DataModel.IColumnList       _transpileNode_Single(Transpile.Context context)
        {
            var select        = n_Selects[0];
            var contextRowSet = new Transpile.ContextRowSets(context, true);


            select.TranspileNode(contextRowSet);

            var resultset = _transpileNode_Single_ResultSet(contextRowSet);
            if (resultset != null) {
                n_OrderBy?.TranspileNode(new Transpile.ContextRowSets(contextRowSet, resultset));

                if (select.n_Into != null) {
                    if (!context.GetDeclarationObjectCode().Entity.TempTableAdd(select.n_Into.ValueString, select.n_Into, resultset.GetUniqueNamedList(), null, out var tempTable)) {
                        context.AddError(select.n_Into, "Temp table already defined at a differend location.");
                    }
                    select.n_Into.SetSymbol(tempTable);
                }
            }
            else {
                if (select.n_Into != null)
                    throw new TranspileException(n_Selects[0].n_Into, "Into not possible with variable assignment.");

                if (n_OrderBy != null) {
                    n_OrderBy.TranspileNode(contextRowSet);

                    if (n_OrderBy.n_Offset != null) {
                        if (n_OrderBy.n_Rows != null) {
                            Validate.ConstInt(n_OrderBy.n_Rows, 1, 1);
                        }
                        else
                            context.AddWarning(n_OrderBy, "Missing FETCH ROWS.");
                    }
                    else
                    if (n_Selects[0].n_Top != null) {
                        Validate.ConstInt(n_Selects[0].n_Top, 1, 1);
                    }
                    else
                        context.AddWarning(n_OrderBy, "Missing top(1).");
                }
            }

            if (n_OrderBy != null && select.n_Top != null && n_OrderBy.n_Offset != null)
                context.AddError(n_OrderBy.n_Offset, "Top with ORDER BY OFFSET not allowed.");

            return resultset;
        }
        private                 DataModel.ColumnListResult  _transpileNode_Single_ResultSet(Transpile.Context context)
        {
            var     columns = n_Selects[0].n_Columns?.n_Columns;
            if (columns == null)
                return null;

            List<DataModel.Column>      columnList = null;

            foreach(var column in columns) {
                if (column is Query_Select_ColumnAssign) {
                    VariableAssigment = true;
                }
                else {
                    if (columnList == null)
                        columnList = new List<DataModel.Column>();

                    column.AddColumnToList(context, columnList);
                }

                if (VariableAssigment && columnList != null)
                    throw new TranspileException(column, "Variable assignment and resultset not possible.");
            }

            return (columnList != null) ? new DataModel.ColumnListResult(columnList.ToArray()) : null;
        }
        private                 DataModel.IColumnList       _transpileNode_Union(Transpile.Context context)
        {
            foreach (var select in n_Selects)
                select.TranspileNode(new Transpile.ContextRowSets(context, true));

            foreach(var select in n_Selects) {
                if (select.n_Columns == null)
                    throw new TranspileException(select, "Select has no columns");

                if (select.n_Into != null)
                    throw new TranspileException(select.n_Into, "INTO not possible in UNION select.");

                foreach (var column in select.n_Columns.n_Columns) {
                    if (column is Query_Select_ColumnExpression) {
                        if (!((Query_Select_ColumnExpression)column).n_Expression.isValid())
                            return null;
                    }
                    else
                        throw new TranspileException(column, (column is Query_Select_ColumnWildcard ? "Wildcard not possible in a select union." : "Column assignment not possible in a select union."));
                }
            }

            var resultset = _transpileNode_Union_ResultSet(context);

            n_OrderBy?.TranspileNode(new Transpile.ContextRowSets(context, resultset));

            return resultset;
        }
        private                 DataModel.ColumnListResult  _transpileNode_Union_ResultSet(Transpile.Context context)
        {
            int     columnCount = n_Selects[0].n_Columns.n_Columns.Length;

            for (int i = 1 ; i < n_Selects.Length ; ++i) {
                var l = n_Selects[i].n_Columns.n_Columns.Length;
                if (l != columnCount)
                    throw new TranspileException(n_Selects[i].n_Columns, "Select returned '" + l + "' columns, needed '" + columnCount + "' columns.");
            }

            var columns = new DataModel.Column[columnCount];

            for (int colidx = 0 ; colidx < columnCount ; ++colidx) {
                var firstcol    = (Query_Select_ColumnExpression)n_Selects[0].n_Columns.n_Columns[colidx];
                var expressions = new IExprNode[n_Selects.Length];
                var typeResult  = new FlagsTypeCollation();

                try {
                    for (int i = 0 ; i < n_Selects.Length ; ++i)
                        expressions[i] = ((Query_Select_ColumnExpression)n_Selects[i].n_Columns.n_Columns[colidx]).n_Expression;

                    typeResult = TypeHelpers.OperationUnion(expressions);
                }
                catch(Exception err) {
                    typeResult.Clear();
                    context.AddError(firstcol, err);
                }

                DataModel.ColumnUnion       column;

                if (firstcol.n_ColumnName != null)
                    column = new DataModel.ColumnUnion(firstcol.n_ColumnName.ValueString, expressions, typeResult,
                                                       declaration: firstcol.n_ColumnName);
                else
                if (firstcol.n_Expression is Expr_PrimativeValue primativeData && primativeData.Referenced is DataModel.Column expressionColumn)
                    column = new DataModel.ColumnUnion(expressionColumn.Name, expressions, typeResult,
                                                       nameReference:   expressionColumn,
                                                       declaration:     expressionColumn.Declaration);
                else
                    column = new DataModel.ColumnUnion("", expressions, typeResult);

                if (column.Name.Length > 0) {
                    for (int i = 0 ; i < n_Selects.Length ; ++i)
                        ((Query_Select_ColumnExpression)n_Selects[i].n_Columns.n_Columns[colidx]).n_ColumnName?.SetSymbol(column);
                }

                columns[colidx] = column;
            }

            return new DataModel.ColumnListResult(columns);
        }
    }
}
