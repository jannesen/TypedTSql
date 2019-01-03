using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-US/library/ms189835.aspx
    //      DELETE [FROM] { { { table_alias | <object> } [ WITH ( <Table_Hint_Limited> [ ...n ] ) ] } | @table_variable }
    //      [ FROM{ <table_source> } ]
    //      [ WHERE { <search_condition>
    //            | { CURRENT OF { [ GLOBAL ] cursor_name } | cursor_variable_name } }
    //      [ OPTION ( <query_hint> [ ,...n ] ) ]
    public abstract class Statement_DELETE_UPDATE: Statement
    {
        public                  ITableSource                        n_Target                { get; private set; }
        public                  TableSource                         n_From                  { get; private set; }
        public                  Node_CursorName                     n_WhereCursor           { get; private set; }
        public                  IExprNode                           n_WhereExpression       { get; private set; }
        public                  Node_QueryOptions                   n_QueryOptions          { get; private set; }

        protected               void                                ParseTarget(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.validateToken(Core.TokenID.Name, Core.TokenID.QuotedName, Core.TokenID.LocalName)) {
            case Core.TokenID.Name:
            case Core.TokenID.QuotedName:
                n_Target = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Unknown));
                break;

            case Core.TokenID.LocalName:
                n_Target = AddChild(new Node_TableVariable(reader));
                break;
            }
        }
        protected               void                                ParseFromWhereOption(Core.ParserReader reader)
        {
            if (ParseOptionalToken(reader, Core.TokenID.FROM) != null)
                n_From = AddChild(new TableSource(reader));

            if (n_Target is Node_EntityNameReference)
                ((Node_EntityNameReference)n_Target).UpdateType(reader, (n_From != null ? EntityReferenceType.FromReference : EntityReferenceType.TableOrView));

            if (ParseOptionalToken(reader, Core.TokenID.WHERE) != null) {
                if (reader.CurrentToken.isToken(Core.TokenID.CURRENT)) {
                    ParseToken(reader, Core.TokenID.CURRENT);
                    ParseToken(reader, Core.TokenID.OF);
                    n_WhereCursor = AddChild(new Node_CursorName(reader));
                }
                else
                    n_WhereExpression = ParseExpression(reader);
            }

            if (reader.CurrentToken.isToken(Core.TokenID.OPTION)) {
                n_QueryOptions = AddChild(new Node_QueryOptions(reader));
            }
        }
        protected               void                                TranspileOptions(Transpile.ContextStatementQuery contextStatement)
        {
            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }
        }
        protected               void                                TranspileFromWhereExpression(Transpile.ContextStatementQuery contextStatement, Transpile.ContextRowSets contextRowSet)
        {
            if (n_From != null) {
                var contextFrom = new Transpile.ContextRowSets(contextStatement, false);
                n_From?.TranspileNode(contextFrom);
                n_Target.TranspileNode(contextFrom);

                var rowset = contextFrom.RowSets.FindRowSet(((Node_EntityNameReference)n_Target).n_Name.ValueString);
                if (rowset != null) {
                    contextStatement.SetTarget(rowset);

                    if (rowset.Source == null)
                        contextRowSet.AddError(n_Target, "Can't use rowset as target.");
                }
                else
                    contextRowSet.AddError(n_Target, "Unknown rowset alias.");

                contextRowSet.RowSets.AddRange(contextFrom.RowSets);
            }
            else {
                n_Target.TranspileNode(contextRowSet);

                var rowset = new DataModel.RowSet("", n_Target.getColumnList(contextStatement), source: n_Target.getDataSource());
                contextStatement.SetTarget(rowset);
                contextRowSet.RowSets.Add(rowset);
            }

            if (n_WhereCursor != null)
                n_WhereCursor.TranspileNode(contextRowSet);

            if (n_WhereExpression != null)
                n_WhereExpression.TranspileNode(contextRowSet);
        }
    }
}
