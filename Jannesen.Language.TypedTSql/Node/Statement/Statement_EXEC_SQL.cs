using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188332.aspx
    //      { EXEC | EXECUTE }
    //          [ @return_status = ]
    //          Objectname
    //          [ [ @parameter = ] { value
    //                             | @variable [ OUTPUT ]
    //                             | [ DEFAULT ]
    //                             }
    //          ] [ ,...n ]
    //      { EXEC | EXECUTE }
    //      ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    //      [ AS { LOGIN | USER } = ' name ' ]
    [StatementParser(Core.TokenID.Name, prio:1)]
    public class Statement_EXEC_SQL: Statement
    {
        private     readonly    Core.Token                      _n_exec_sql;
        public      readonly    ISetVariable                    n_ProcedureReturn;
        public      readonly    IExprNode                       n_Statement;
        public      readonly    Node_EXEC_Parameter[]           n_Parameters;

        public      static      bool                            CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("EXEC_SQL");
        }
        public                                                  Statement_EXEC_SQL(Core.ParserReader reader, IParseContext parseContext)
        {
            _n_exec_sql = ParseToken(reader, "EXEC_SQL");

            if (reader.CurrentToken.isToken(Core.TokenID.LocalName) && reader.NextPeek().isToken(Core.TokenID.Equal)) {
                n_ProcedureReturn = ParseSetVariable(reader);
                ParseToken(reader, Core.TokenID.Equal);
            }

            n_Statement = ParseExpression(reader);

            var     parameters = new List<Node_EXEC_Parameter>();

            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                parameters.Add(AddChild(new Node_EXEC_Parameter(reader, true)));
            }

            n_Parameters = parameters.ToArray();

            ParseStatementEnd(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Statement.TranspileNode(context);
            n_Parameters.TranspileNodes(context);
            if (n_Parameters.Length == 0)
                context.AddError(this, "EXEC_SQL has no parameters");

            if (n_ProcedureReturn != null) {
                context.VariableSetInt(n_ProcedureReturn);
            }

            foreach(var p in n_Parameters) {
                p.TranspileParameter(context, null);
            }
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            foreach(var node in this.Children) {
                if (object.ReferenceEquals(node, _n_exec_sql)) {
                    emitWriter.WriteText("EXECUTE");
                    continue;
                }

                if (object.ReferenceEquals(node, n_Statement)) {
                    _customExecuteSql(emitWriter);
                    continue;
                }

                node.Emit(emitWriter);
            }
        }
        private                 void                            _customExecuteSql(Core.EmitWriter emitWriter)
        {
            emitWriter.WriteText(" sys.sp_executesql");

            n_Statement.Emit(emitWriter);

            var     prms = new StringBuilder();

            prms.Append(", N'");
            for (int i = 0 ; i < n_Parameters.Length ; ++i) {
                var param = n_Parameters[i];

                if (i > 0)
                    prms.Append(",");

                prms.Append(param.n_Name.Text);
                prms.Append(" ");
                prms.Append(param.n_Expression.SqlType.NativeType.ToSql());

                if (param.n_Output)
                    prms.Append(" OUT");
            }

            prms.Append("'");
            emitWriter.WriteText(prms.ToString());
        }
    }
}
