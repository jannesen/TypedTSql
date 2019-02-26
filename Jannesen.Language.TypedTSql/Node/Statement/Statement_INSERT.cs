using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-US/library/ms174335.aspx
    //  INSERT INTO Object [ WITH ( <Table_Hint_Limited> [ ...n ] ) ]
    //      [ ( column_list ) ]
    //      { VALUES ( { DEFAULT | NULL | expression } [ ,...n ] ) [ ,...n     ]
    //      | derived_table
    [StatementParser(Core.TokenID.INSERT)]
    public class Statement_INSERT: Statement
    {
        public class ValueList: Core.AstParseNode
        {
            public      readonly    IExprNode[]             n_Expressions;

            public                                          ValueList(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.LrBracket);

                var expressions = new List<IExprNode>();

                do {
                    expressions.Add(ParseExpression(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                n_Expressions = expressions.ToArray();

                ParseToken(reader, Core.TokenID.RrBracket);
            }

            public      override    void                    TranspileNode(Transpile.Context context)
            {
                n_Expressions.TranspileNodes(context);
            }
        }

        public      readonly    ITableSource                        n_Target;
        public      readonly    Node_TableHints                     n_TargetWith;
        public      readonly    Core.TokenWithSymbol[]              n_InsertColumns;
        public      readonly    ValueList[]                         n_Values;
        public      readonly    Query_Select                        n_Select;
        public      readonly    Statement                           n_Execute;
        public      readonly    Node_QueryOptions                   n_QueryOptions;

        public                                                      Statement_INSERT(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.INSERT);
            ParseOptionalToken(reader, Core.TokenID.INTO);

            switch(reader.CurrentToken.validateToken(Core.TokenID.LocalName, Core.TokenID.Name, Core.TokenID.QuotedName)) {
            case Core.TokenID.LocalName:
                n_Target = AddChild(new Node_TableVariable(reader));
                break;

            case Core.TokenID.Name:
            case Core.TokenID.QuotedName:
                n_Target = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.TableOrView));

                if (reader.CurrentToken.isToken(Core.TokenID.WITH))
                    n_TargetWith = AddChild(new Node_TableHints(reader));
                break;
            }

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                ParseToken(reader, Core.TokenID.LrBracket);

                var columns = new List<Core.TokenWithSymbol>();

                do {
                    columns.Add(ParseName(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);

                n_InsertColumns = columns.ToArray();
            }

            switch(reader.CurrentToken.validateToken(Core.TokenID.VALUES, Core.TokenID.SELECT, Core.TokenID.EXEC, Core.TokenID.EXECUTE)) {
            case Core.TokenID.VALUES: {
                    ParseToken(reader, Core.TokenID.VALUES);

                    var values = new List<ValueList>();

                    do {
                        values.Add(AddChild(new ValueList(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_Values = values.ToArray();
                }
                break;

            case Core.TokenID.SELECT:
                n_Select = AddChild(new Query_Select(reader, Query_SelectContext.StatementInsert));

                if (reader.CurrentToken.isToken(Core.TokenID.OPTION)) {
                    n_QueryOptions = AddChild(new Node_QueryOptions(reader));
                }
                break;

            case Core.TokenID.EXEC:
            case Core.TokenID.EXECUTE:
                if (Statement_EXECUTE_procedure.CanParse(reader, parseContext)) {
                    n_Execute = AddChild(new Statement_EXECUTE_procedure(reader, parseContext));
                }
                else
                if (Statement_EXECUTE_expression.CanParse(reader, parseContext)) {
                    n_Execute = AddChild(new Statement_EXECUTE_expression(reader, parseContext));
                }
                else
                    throw new ParseException(reader.CurrentToken, "Expect EXECUTE_procedure or EXECUTE_expression.");

                break;
            }

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.ScopeIndentityType = null;
            var contextStatement = new Transpile.ContextStatementQuery(context);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Target.TranspileNode(contextStatement);
            n_TargetWith?.TranspileNode(contextStatement);
            n_Values?.TranspileNodes(contextStatement);
            n_Select?.TranspileNode(contextStatement);
            n_Execute?.TranspileNode(contextStatement);

            if (n_Target is Node_TableVariable tableVariable) {
                var variable = tableVariable.Variable;
                if (variable != null) {
                    if (!variable.isReadonly)
                        variable.setAssigned();
                    else
                        context.AddError(n_Target, "Not allowed to assign a readonly variable.");
                }
            }

            var targetColumns = _transpileProcess_TargetColumns(context);

            if (targetColumns != null) {
                if (n_Values != null)
                    _transpileProcess_Values(context, targetColumns);

                if (n_Select != null)
                    _transpileProcess_Select(context, targetColumns);
            }

            try {
                var columns = n_Target.getColumnList(context);

                if (columns != null) {
                    foreach(var c in columns) {
                        if (c.isIdentity) {
                            context.ScopeIndentityType = c.SqlType;
                            break;
                        }
                    }
                }
            }
            catch(Exception err) {
                context.AddError(n_Target, err);
            }
        }

        private                 DataModel.Column[]                  _transpileProcess_TargetColumns(Transpile.Context context)
        {
            var columnList = n_Target.getColumnList(context);

            if (n_InsertColumns != null) {
                var rtn = new DataModel.Column[n_InsertColumns.Length];

                for(int i = 0 ; i < n_InsertColumns.Length ; ++i) {
                    var     columnNode = n_InsertColumns[i];
                    var     column     = columnList.FindColumn(columnNode.ValueString, out bool ambiguous);

                    if (column != null) {
                        columnNode.SetSymbol(column);
                        context.CaseWarning(columnNode, column.Name);
                        column.SetUsed();
                    }
                    else
                        context.AddError(columnNode, "Unknown column '" + columnNode.ValueString + "'.");

                    rtn[i] = column;
                }

                return rtn;
            }
            else {
                var rtn = new DataModel.Column[columnList.Count];

                for(int i = 0 ; i < columnList.Count ; ++i)
                    rtn[i] = columnList[i];

                return rtn;
            }
        }
        private                 void                                _transpileProcess_Values(Transpile.Context context, DataModel.Column[] targetColumns)
        {
            foreach(var value in n_Values) {
                try {
                    if (targetColumns.Length != value.n_Expressions.Length)
                        throw new Exception(targetColumns.Length > value.n_Expressions.Length ? "Missing columns" : "Tomany columns");

                    for(int i = 0 ; i < targetColumns.Length ; ++i) {
                        if (targetColumns[i] != null) {
                            try {
                                Validate.Assign(context, targetColumns[i], value.n_Expressions[i]);
                            }
                            catch(Exception err) {
                                context.AddError(value.n_Expressions[i], err);
                            }
                        }
                    }
                }
                catch(Exception err) {
                    context.AddError(value, err);
                }
            }
        }
        private                 void                                _transpileProcess_Select(Transpile.Context context, DataModel.Column[] targetColumns)
        {
            if (targetColumns.Length != n_Select.Resultset.Count) {
                context.AddError(n_Select.n_Selects[0].n_Columns, (targetColumns.Length > n_Select.Resultset.Count ? "Missing columns" : "Tomany columns"));
                return;
            }

            for(int i = 0 ; i < targetColumns.Length ; ++i) {
                if (targetColumns[i] != null) {
                    try {
                        Validate.Assign(context, targetColumns[i], n_Select.Resultset[i]);
                    }
                    catch(Exception err) {
                        if (!(err is TranspileException))
                            err = new ErrorException("Assignment column#" + (i+1) + " '" + targetColumns[i].Name + "' failed.", err);

                        context.AddError((n_InsertColumns != null) ? (Core.IAstNode)n_InsertColumns[i] : (Core.IAstNode)this, err);
                    }
                }
            }
        }
    }
}
