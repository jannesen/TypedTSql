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
        public                  IDataTarget                         n_Target                { get; private set; }
        public                  TableSource                         n_From                  { get; private set; }
        public                  Node_CursorName                     n_WhereCursor           { get; private set; }
        public                  IExprNode                           n_WhereExpression       { get; private set; }
        public                  Node_QueryOptions                   n_QueryOptions          { get; private set; }

        protected               void                                ParseTarget(Core.ParserReader reader, DataModel.SymbolUsageFlags usage)
        {
            switch(reader.CurrentToken.validateToken(Core.TokenID.Name, Core.TokenID.QuotedName, Core.TokenID.LocalName)) {
            case Core.TokenID.Name:
            case Core.TokenID.QuotedName:
                n_Target = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Unknown, usage));
                break;

            case Core.TokenID.LocalName:
                n_Target = AddChild(new Node_TableVariable(reader, usage));
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
                    n_WhereCursor = AddChild(new Node_CursorName(reader, DataModel.SymbolUsageFlags.Reference));
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
        protected               void                                TranspileFromWhereExpression(Transpile.ContextStatementQuery contextStatement, Transpile.ContextRowSets contextRowSet, DataModel.SymbolUsageFlags usage)
        {
            if (n_From != null) {
                var contextFrom = new Transpile.ContextRowSets(contextStatement);
                n_From.TranspileNode(contextFrom);

                if (n_Target is Node_EntityNameReference entityNameReference) {
                    entityNameReference.TranspileAliasTarget(contextFrom, n_From, usage);
                }
                else {
                    contextStatement.AddError(n_From, "Target needs to be a alias.");
                }

                contextStatement.SetTarget(n_Target);
                contextRowSet.RowSets.AddRange(contextFrom.RowSets);
            }
            else {
                n_Target.TranspileNode(contextRowSet);
                contextStatement.SetTarget(n_Target);
                contextRowSet.RowSets.Add(new DataModel.RowSet(DataModel.RowSetFlags.Target, n_Target.Columns,
                                                               source: n_Target.Table));
            }

            if (n_WhereCursor != null)
                n_WhereCursor.TranspileNode(contextRowSet);

            if (n_WhereExpression != null)
                n_WhereExpression.TranspileNode(contextRowSet);
        }
    }
}
