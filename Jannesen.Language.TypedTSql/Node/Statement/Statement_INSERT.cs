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

            public                  DataModel.Column[]      TranspileProcess(Transpile.Context context, DataModel.IColumnList targetColumList)
            {
                var rtn = new DataModel.Column[n_InsertColumns.Length];

                for(int i = 0 ; i < n_InsertColumns.Length ; ++i) {
                    var     columnNode = n_InsertColumns[i];
                    var     column     = targetColumList.FindColumn(columnNode.ValueString, out bool ambiguous);

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

        public      readonly    ITableSource                        n_Target;
        public      readonly    Node_TableHints                     n_TargetWith;
        public      readonly    Core.AstParseNode                   n_TargetColumns;
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
                if (Node_TableVarVariable.CanParse(reader)) {
                    n_Target = AddChild(new Node_TableVarVariable(reader));
                }
                else { 
                    n_Target = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.TableOrView));

                    if (reader.CurrentToken.isToken(Core.TokenID.WITH))
                        n_TargetWith = AddChild(new Node_TableHints(reader));
                }
                break;
            }

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                n_TargetColumns = AddChild(reader.NextPeek().isToken(Core.TokenID.Star) ? (Core.AstParseNode)new TargetNamedBy(reader) : (Core.AstParseNode)new TargetColumnNames(reader));
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
                n_Select = AddChild(new Query_Select(reader, n_TargetColumns is TargetNamedBy ? Query_SelectContext.StatementInsertTargetNamed : Query_SelectContext.StatementInsert));

                if (reader.CurrentToken.isToken(Core.TokenID.OPTION)) {
                    n_QueryOptions = AddChild(new Node_QueryOptions(reader));
                }
                break;

            case Core.TokenID.EXEC:
            case Core.TokenID.EXECUTE:
                if (Statement_EXECUTE_expression.CanParse(reader, parseContext)) {
                    n_Execute = AddChild(new Statement_EXECUTE_expression(reader, parseContext));
                }
                else
                if (Statement_EXECUTE_procedure.CanParse(reader, parseContext)) {
                    n_Execute = AddChild(new Statement_EXECUTE_procedure(reader, parseContext));
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
            var contextStatement = new Transpile.ContextStatementQuery(context);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Target.TranspileNode(contextStatement);
            n_TargetWith?.TranspileNode(contextStatement);

            if (n_TargetColumns is TargetNamedBy) {
                if (n_Target is Node_TableVarVariable) {
                    context.AddError(n_TargetColumns, "column names with var variable not allowed.");
                    return;
                }

                contextStatement.SetTarget(new DataModel.RowSet("", n_Target.getColumnList(context), source: n_Target.getDataSource()));
            }

            n_Values?.TranspileNodes(contextStatement);
            n_Select?.TranspileNode(contextStatement);
            n_Execute?.TranspileNode(contextStatement);

            if (n_Target is Node_TableVariable tableVariable) {
                var variable = tableVariable.Variable;
                if (variable != null) {
                    if (!variable.isReadonly) {
                        if (n_TargetColumns == null) {
                            Logic.Validate.IntoUnnamed(n_Target, variable, _transpileProcess_SelectResultset(context));
                        }
                        variable.setAssigned();
                    }
                    else
                        context.AddError(n_Target, "Not allowed to assign a readonly variable.");
                }
            }

            if (n_Target is Node_TableVarVariable tableVarVariable) {
                tableVarVariable.TranspileInsert(context, _transpileProcess_SelectResultset(context));
            }

            if (n_TargetColumns != null) {
                if (n_Target is Node_TableVarVariable) {
                    context.AddError(n_TargetColumns, "Column-names with variable declaration not allowed.");
                }
                else {
                    if (n_TargetColumns is TargetColumnNames targetColumnNamesNode) {
                        _transpileProcess_ColumnNames(context, targetColumnNamesNode);
                    }

                    if (n_TargetColumns is TargetNamedBy targetNamedByNode) {
                        if (n_Select != null) {
                            _transpileProcess_TargetNames(context, targetNamedByNode);
                        }
                        else {
                            context.AddError(n_TargetColumns, "Missing select after named insert.");
                        }
                    }
                }
            }
            else {
                if (!(n_Target is Node_TableVarVariable)) {
                    if (!(n_Target is Node_TableVariable)) {
                        context.AddError(this, "Insert into table without columns specification is not allowed.");
                    }

                    _transpileProcess_Unnamed(context);
                }
            }

            _transpileProcess_ScopeIndentityType(context);
        }

        private                 void                                _transpileProcess_Unnamed(Transpile.Context context)
        {
            var columnList = n_Target.getColumnList(context);
            var targetColumns = new DataModel.Column[columnList.Count];

            for(int i = 0 ; i < columnList.Count ; ++i)
                targetColumns[i] = columnList[i];

            if (n_Values != null)
                _transpileProcess_ValidateColumnsValues(context, targetColumns);

            if (n_Select != null || n_Execute != null)
                _transpileProcess_ValidateColumnsSelect(context, targetColumns, null);
        }
        private                 void                                _transpileProcess_ColumnNames(Transpile.Context context, TargetColumnNames targetColumnNamesNode)
        {
            var targetColumns = targetColumnNamesNode.TranspileProcess(context, n_Target.getColumnList(context));

            if (n_Values != null)
                _transpileProcess_ValidateColumnsValues(context, targetColumns);

            if (n_Select != null || n_Execute != null)
                _transpileProcess_ValidateColumnsSelect(context, targetColumns, targetColumnNamesNode.n_InsertColumns);
        }
        private                 void                                _transpileProcess_TargetNames(Transpile.Context context, TargetNamedBy targetNamedByNode)
        {
            var resultSet = n_Select.Resultset;

            if (resultSet != null) {
                targetNamedByNode.TranssileSetResultset(resultSet);
            }
        }
        private                 void                                _transpileProcess_ScopeIndentityType(Transpile.Context context)
        {
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
        private                 void                                _transpileProcess_ValidateColumnsValues(Transpile.Context context, DataModel.Column[] targetColumns)
        {
            foreach(var value in n_Values) {
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
        private                 void                                _transpileProcess_ValidateColumnsSelect(Transpile.Context context, DataModel.Column[] targetColumns, Core.Token[] targetColumnNames)
        {
            var resultset = _transpileProcess_SelectResultset(context);

            if (resultset != null) {
                if (targetColumns.Length != resultset.Count) {
                    context.AddError(n_Target, (targetColumns.Length < resultset.Count ? "Missing columns" : "Tomany columns"));
                    return;
                }

                for(int i = 0 ; i < targetColumns.Length ; ++i) {
                    if (targetColumns[i] != null) {
                        try {
                            Validate.Assign(context, targetColumns[i], resultset[i]);
                        }
                        catch(Exception err) {
                            if (!(err is TranspileException))
                                err = new ErrorException("Assignment column#" + (i+1) + " '" + targetColumns[i].Name + "' failed.", err);

                            context.AddError((targetColumnNames != null) ? (Core.IAstNode)targetColumnNames[i] : (Core.IAstNode)this, err);
                        }
                    }
                }
            }
        }
        private                 DataModel.IColumnList               _transpileProcess_SelectResultset(Transpile.Context context)
        {
            if (n_Select != null) { 
                return n_Select.Resultset;
            }

            if (n_Execute is Statement_EXECUTE_procedure execproc && execproc.n_ProcedureReference.Entity is DataModel.EntityObjectCode entityObject) {
                var resulsets = entityObject.Resultsets;
                if (resulsets == null || resulsets.Count == 0) {
                    throw new ErrorException("Procedure '" + entityObject.EntityName.ToString() + "' don't returns a dataset.");
                }

                if (resulsets.Count == 1) {
                    return resulsets[0];
                }
            }

            return null;
        }
    }
}
