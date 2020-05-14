using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/receive-transact-sql
    [StatementParser(Core.TokenID.Name, prio:3)]
    public class Statement_RECEIVE: Statement
    {
        private readonly static DataModel.ColumnList                _queue_columns  = new DataModel.ColumnList(
                                                                                          new DataModel.Column[] {
                                                                                              new DataModel.ColumnDS("status",                  DataModel.SqlTypeNative.TinyInt),
                                                                                              new DataModel.ColumnDS("priority",	              DataModel.SqlTypeNative.TinyInt),
                                                                                              new DataModel.ColumnDS("queuing_order",	          DataModel.SqlTypeNative.BigInt),
                                                                                              new DataModel.ColumnDS("conversation_group_id",   DataModel.SqlTypeNative.UniqueIdentifier),
                                                                                              new DataModel.ColumnDS("conversation_handle",	  DataModel.SqlTypeNative.UniqueIdentifier),
                                                                                              new DataModel.ColumnDS("message_sequence_number", DataModel.SqlTypeNative.BigInt),
                                                                                              new DataModel.ColumnDS("service_name",	          DataModel.SqlTypeNative.NVarChar_512),
                                                                                              new DataModel.ColumnDS("service_id",	          DataModel.SqlTypeNative.Int),
                                                                                              new DataModel.ColumnDS("service_contract_name",	  DataModel.SqlTypeNative.NVarChar_256),
                                                                                              new DataModel.ColumnDS("service_contract_id",	  DataModel.SqlTypeNative.Int),
                                                                                              new DataModel.ColumnDS("message_type_name",	      DataModel.SqlTypeNative.NVarChar_256),
                                                                                              new DataModel.ColumnDS("message_type_id",	      DataModel.SqlTypeNative.Int),
                                                                                              new DataModel.ColumnDS("validation",	          DataModel.SqlTypeNative.NChar_2),
                                                                                              new DataModel.ColumnDS("message_body",	          DataModel.SqlTypeNative.VarBinary_MAX),
                                                                                          });

        public      readonly    IExprNode                           n_Top;
        public      readonly    Query_Select_ColumnList             n_Columns;
        public      readonly    Node_EntityNameReference            n_Queue;
        public      readonly    ITableSource                        n_Into;
        public      readonly    IExprNode                           n_Where;

        public                  DataModel.IColumnList               Resultset               { get; private set; }

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("RECEIVE");
        }
        public                                                      Statement_RECEIVE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, "RECEIVE");
            if (ParseOptionalToken(reader, Core.TokenID.TOP) != null) {
                ParseToken(reader, Core.TokenID.LrBracket);
                n_Top = ParseExpression(reader);
                ParseToken(reader, Core.TokenID.RrBracket);
            }

            n_Columns = AddChild(new Query_Select_ColumnList(reader, Query_SelectContext.StatementReceive));

            ParseToken(reader, Core.TokenID.FROM);
            n_Queue = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Queue));

            if (ParseOptionalToken(reader, Core.TokenID.INTO) != null) {
                if (Node_TableVarVariable.CanParse(reader)) {
                    n_Into = AddChild(new Node_TableVarVariable(reader));
                }
                else {
                    n_Into = AddChild(new Node_TableVariable(reader));
                }
            }

            if (ParseOptionalToken(reader, Core.TokenID.WHERE) != null) {
                n_Where = ParseExpression(reader);
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            Resultset = null;
            var contextRowSet = new Transpile.ContextRowSets(context, _queue_columns);
            n_Queue?.TranspileNode(contextRowSet);
            n_Top?.TranspileNode(contextRowSet);
            n_Columns?.TranspileNode(contextRowSet);
            n_Where?.TranspileNode(contextRowSet);
            n_Into?.TranspileNode(context);

            Resultset = n_Columns.GetResultSet(contextRowSet);

            if (n_Into is Node_TableVariable tableVariable) {
                var variable = tableVariable.Variable;
                if (variable != null) {
                    if (!variable.isReadonly) { 
                        Logic.Validate.IntoUnnamed(n_Into, variable, Resultset);
                        variable.setAssigned();
                    }
                    else
                        context.AddError(n_Into, "Not allowed to assign a readonly variable.");
                }
            }

            if (n_Into is Node_TableVarVariable tableVarVariable) {
                tableVarVariable.TranspileInsert(context, Resultset.GetUniqueNamedList());
            }
        }
    }
}
