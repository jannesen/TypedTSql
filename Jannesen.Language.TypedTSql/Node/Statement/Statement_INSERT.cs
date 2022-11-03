using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
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
        public class TargetColumnNames: Core.AstParseNode
        {
            public      readonly    Core.TokenWithSymbol[]  n_InsertColumns;

            public                                          TargetColumnNames(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.LrBracket);

                var columns = new List<Core.TokenWithSymbol>();

                do {
                    columns.Add(ParseName(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);

                n_InsertColumns = columns.ToArray();
            }

            public      override    void                    TranspileNode(Transpile.Context context)
            {
            }

            public                  DataModel.Column[]      TranspileTargetColumns(Transpile.Context context, IDataTarget target)
            {
                if (target.isVarDeclare) {
                    context.AddError(target, "Syntax error var @var with column list.");
                    return null;
                }

                var targetColumList = target.Columns;
                if (targetColumList == null) {
                    return null;
                }

                var rtn = new DataModel.Column[n_InsertColumns.Length];

                for(int i = 0 ; i < n_InsertColumns.Length ; ++i) {
                    var     columnNode = n_InsertColumns[i];
                    var     column     = targetColumList.FindColumn(columnNode.ValueString, out bool ambiguous);

                    if (column != null) {
                        columnNode.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Write);
                        context.CaseWarning(columnNode, column.Name);
                        column.SetUsed();
                    }
                    else
                        context.AddError(columnNode, "Unknown column '" + columnNode.ValueString + "'.");

                    rtn[i] = column;
                }

                return rtn;
            }
        }
        public class TargetNamedBy: Core.AstParseNode
        {
            private                 DataModel.IColumnList   _resultset;

            public                                          TargetNamedBy(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.LrBracket);
                ParseToken(reader, Core.TokenID.Star);
                ParseToken(reader, Core.TokenID.RrBracket);
            }

            public      override    void                    TranspileNode(Transpile.Context context)
            {
                _resultset = null;
            }
            public                  void                    TranssileSetResultset(DataModel.IColumnList resultset)
            {
                _resultset = resultset;
            }
            public      override    void                    Emit(EmitWriter emitWriter)
            {
                foreach (var n in Children) {
                    if (n is Core.Token token && token.ID == Core.TokenID.Star) {
                        bool next = false;

                        foreach(var c in _resultset) {
                            if (next) {
                                emitWriter.WriteText(",");
                            }

                            emitWriter.WriteText(SqlStatic.QuoteName(c.Name));

                            next = true;
                        }
                    }
                    else {
                        n.Emit(emitWriter);
                    }
                }
            }
        }

        public      readonly    IDataTarget                         n_Target;
        public      readonly    Node_TableHints                     n_TargetWith;
        public      readonly    Core.AstParseNode                   n_TargetColumns;
        public      readonly    object                              n_SelectValuesExecute;
        public      readonly    Node_QueryOptions                   n_QueryOptions;

        public                                                      Statement_INSERT(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.INSERT);
            ParseOptionalToken(reader, Core.TokenID.INTO);

            var selectContext = Query_SelectContext.StatementInsert;

            switch(reader.CurrentToken.validateToken(Core.TokenID.LocalName, Core.TokenID.Name, Core.TokenID.QuotedName)) {
            case Core.TokenID.LocalName:
                n_Target = AddChild(new Node_TableVariable(reader, DataModel.SymbolUsageFlags.Insert));
                break;

            case Core.TokenID.Name:
            case Core.TokenID.QuotedName:
                if (Node_TableVarVariable.CanParse(reader)) {
                    n_Target = AddChild(new Node_TableVarVariable(reader));
                    selectContext = Query_SelectContext.StatementInsertTargetVarVariable;
                }
                else { 
                    n_Target = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.TableOrView, DataModel.SymbolUsageFlags.Insert));

                    if (reader.CurrentToken.isToken(Core.TokenID.WITH))
                        n_TargetWith = AddChild(new Node_TableHints(reader));
                }
                break;
            }

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                if (reader.NextPeek().isToken(Core.TokenID.Star)) {
                    n_TargetColumns = AddChild(new TargetNamedBy(reader));
                    if (selectContext == Query_SelectContext.StatementInsert) {
                        selectContext = Query_SelectContext.StatementInsertTargetNamed;
                    }
                }
                else {
                    n_TargetColumns = AddChild(new TargetColumnNames(reader));
                }
            }

            switch(reader.CurrentToken.validateToken(Core.TokenID.VALUES, Core.TokenID.SELECT, Core.TokenID.EXEC, Core.TokenID.EXECUTE)) {
            case Core.TokenID.SELECT:
                n_SelectValuesExecute = AddChild(new Query_Select(reader, selectContext));

                if (reader.CurrentToken.isToken(Core.TokenID.OPTION)) {
                    n_QueryOptions = AddChild(new Node_QueryOptions(reader));
                }
                break;

            case Core.TokenID.VALUES: {
                    ParseToken(reader, Core.TokenID.VALUES);

                    var values = new List<ValueList>();

                    do {
                        values.Add(AddChild(new ValueList(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_SelectValuesExecute = values.ToArray();
                }
                break;

            case Core.TokenID.EXEC:
            case Core.TokenID.EXECUTE:
                if (Statement_EXECUTE_expression.CanParse(reader, parseContext)) {
                    n_SelectValuesExecute = AddChild(new Statement_EXECUTE_expression(reader, parseContext, false));
                }
                else
                if (Statement_EXECUTE_procedure.CanParse(reader, parseContext)) {
                    n_SelectValuesExecute = AddChild(new Statement_EXECUTE_procedure(reader, parseContext, false));
                }
                else
                    throw new ParseException(reader.CurrentToken, "Expect EXECUTE_procedure or EXECUTE_expression.");

                break;
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.ScopeIndentityType = null;
            var contextSelect = new Transpile.ContextStatementQuery(context);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextSelect);
                contextSelect.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Target.TranspileNode(contextSelect);
            n_TargetWith?.TranspileNode(contextSelect);

            if (n_SelectValuesExecute is Query_Select select) {
                if (n_Target is Node_TableVarVariable tableVarVariable) {
                    // insert into var @variable select
                    if (n_TargetColumns == null) {
                        _transpileProcess_InsertSelect_VarVariable(contextSelect, tableVarVariable, select);
                    }
                    else {
                        context.AddError(n_TargetColumns, "Column-names with variable declaration not allowed.");
                    }
                }
                else if (n_TargetColumns is TargetColumnNames targetColumnNames) {
                    _transpileProcess_InsertSelect_TargetColumnNames(contextSelect, targetColumnNames, select);
                }
                else if (n_TargetColumns is TargetNamedBy targetNamedBy) {
                    // insert into table(*)  select
                    _transpileProcess_InsertSelect_TargetNamedBy(contextSelect, targetNamedBy, select);
                }
                else {
                    // insert into table select 
                    context.AddError(n_Target, "Expect column name insert.");
                }
            }
            else if (n_SelectValuesExecute is ValueList[] valueList) {
                // insert into [table!@var](col1,...) values
                valueList.TranspileNodes(contextSelect);

                if (n_TargetColumns is TargetColumnNames targetColumnNamesNode) {
                    _transpileProcess_InsertValues(context, targetColumnNamesNode, valueList);
                }
                else {
                    context.AddError(n_Target, "Expect column name list.");
                }
            }
            else if (n_SelectValuesExecute is Statement_EXECUTE_expression || n_SelectValuesExecute is Statement_EXECUTE_procedure) {
                // insert into [table!@var](col1,...) exec
                ((AstParseNode)n_SelectValuesExecute).TranspileNode(contextSelect);
                if (n_TargetColumns is TargetColumnNames targetColumnNamesNode) {
                    _transpileProcess_InsertExecute(context, targetColumnNamesNode);
                }
                else {
                    context.AddError(n_Target, "Expect column name list.");
                }
            }
            else {
                throw new InvalidOperationException("Internal error invalid n_SelectValuesExecute type.");
            }

            if (n_Target is Node_TableVariable tableVariable) {
                var variable = tableVariable.Variable;
                if (variable != null) {
                    if (!variable.isReadonly) {
                        variable.setAssigned();
                    }
                    else
                        context.AddError(n_Target, "Not allowed to assign a readonly variable.");
                }
            }

            _transpileProcess_ScopeIndentityType(context);
        }

        private                 void                                _transpileProcess_InsertSelect_VarVariable(Transpile.ContextStatementQuery contextSelect, Node_TableVarVariable tableVarVariable, Query_Select querySelect)
        {
            contextSelect.SetTarget(n_Target);
            querySelect.TranspileNode(contextSelect);
        }
        private                 void                                _transpileProcess_InsertSelect_TargetColumnNames(Transpile.ContextStatementQuery contextSelect, TargetColumnNames targetColumnNames, Query_Select querySelect)
        {
            querySelect.TranspileNode(contextSelect);
            _transpileProcess_TargetColumnNames(contextSelect, targetColumnNames, querySelect.Resultset);
        }
        private                 void                                _transpileProcess_InsertSelect_TargetNamedBy(Transpile.ContextStatementQuery contextSelect, TargetNamedBy targetNamedBy, Query_Select querySelect)
        {
            contextSelect.SetTarget(n_Target);
            querySelect.TranspileNode(contextSelect);

            var resultSet = querySelect.Resultset;

            if (resultSet != null) {
                targetNamedBy.TranssileSetResultset(resultSet);
            }
        }
        private                 void                                _transpileProcess_InsertValues(Transpile.Context context, TargetColumnNames targetColumnNamesNode,  ValueList[] valueList)
        {
            var targetColumns = targetColumnNamesNode.TranspileTargetColumns(context, n_Target);

            if (targetColumns != null) {
                foreach(var value in valueList) {
                    try {
                        if (targetColumns.Length != value.n_Expressions.Length)
                            throw new Exception(targetColumns.Length > value.n_Expressions.Length ? "Missing columns" : "Tomany columns");

                        for(int i = 0 ; i < targetColumns.Length ; ++i) {
                            var targetColumn = targetColumns[i];

                            if (targetColumn != null) {
                                if ((targetColumn.ValueFlags & (DataModel.ValueFlags.NULL      |
                                                                DataModel.ValueFlags.Const     |
                                                                DataModel.ValueFlags.Variable  |
                                                                DataModel.ValueFlags.Computed  |
                                                                DataModel.ValueFlags.Identity)) != 0) {
                                    context.AddError(this, "Can't insert data into column [" + targetColumn.Name + "].");
                                    continue;
                                }

                                try {
                                    Validate.Assign(context, targetColumn, value.n_Expressions[i]);
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
        }
        private                 void                                _transpileProcess_InsertExecute(Transpile.Context context, TargetColumnNames targetColumnNamesNode)
        {
            DataModel.IColumnList   columnList = null;

            if (n_SelectValuesExecute is Statement_EXECUTE_procedure execproc &&
                execproc.n_ProcedureReference is Node_EntityNameReference procedureName &&
                procedureName.Entity is DataModel.EntityObjectCode entityObject) {
                var resulsets = entityObject.Resultsets;
                if (resulsets == null || resulsets.Count == 0) {
                    context.AddError(execproc, "Procedure '" + entityObject.EntityName.ToString() + "' don't returns a dataset.");
                }

                if (resulsets.Count == 1) {
                    columnList = resulsets[0];
                }
            }

            _transpileProcess_TargetColumnNames(context, targetColumnNamesNode, columnList);
        }
        private                 void                                _transpileProcess_TargetColumnNames(Transpile.Context context, TargetColumnNames targetColumnNamesNode, DataModel.IColumnList columnList)
        {
            var targetColumns = targetColumnNamesNode.TranspileTargetColumns(context, n_Target);

            if (targetColumns != null && columnList != null) {
                if (targetColumns.Length != columnList.Count) {
                    context.AddError(n_Target, (targetColumns.Length < columnList.Count ? "Missing columns" : "Tomany columns"));
                    return;
                }

                for(int i = 0 ; i < targetColumns.Length ; ++i) {
                    if (targetColumns[i] != null) {
                        try {
                            Validate.Assign(context, targetColumns[i], columnList[i]);
                        }
                        catch(Exception err) {
                            if (!(err is TranspileException))
                                err = new ErrorException("Assignment column#" + (i+1) + " '" + targetColumns[i].Name + "' failed.", err);

                            var targetColumnNames = targetColumnNamesNode.n_InsertColumns;
                            context.AddError((targetColumnNames != null) ? (Core.IAstNode)targetColumnNames[i] : (Core.IAstNode)this, err);
                        }
                    }
                }
            }
        }
        private                 void                                _transpileProcess_ScopeIndentityType(Transpile.Context context)
        {
            try {
                var columns = n_Target.Columns;
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

    }
}
