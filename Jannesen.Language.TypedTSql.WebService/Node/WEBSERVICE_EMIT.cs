using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class WEBSERVICE_EMIT: LTTSQL.Core.AstParseNode
    {
        public      readonly    WEBSERVICE                      WebService;
        public      readonly    string                          n_Index;
        public      readonly    DataModel.EntityName            n_IndexProcedure;
        public      readonly    WEBSERVICE_EMITOR[]             n_Emitors;

        public                                                  WEBSERVICE_EMIT(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext, WEBSERVICE webService)
        {
            WebService  = webService;
            var emitors = new List<WEBSERVICE_EMITOR>();

            ParseToken(reader, "EMIT");
            ParseToken(reader, Core.TokenID.LrBracket);

            while(!reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                switch(reader.CurrentToken.Text.ToUpper()) {
                case "INDEX":
                    ParseToken(reader, "INDEX");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Index          = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    n_IndexProcedure = new DataModel.EntityName(WebService.n_Name.n_EntitiyName.Schema, WebService.n_Name.n_EntitiyName.Name + "/" + n_Index + ":GET");
                    break;

                case "WEBSERVICECONFIG":
                    emitors.Add(AddChild(new WEBSERVICE_EMITOR_WEBSERVICECONFIG(reader, parseContext)));
                    break;

                case "JC_PROXY":
                    emitors.Add(AddChild(new WEBSERVICE_EMITOR_JC_PROXY(reader, parseContext)));
                    break;

                case "OPENAPI":
                    emitors.Add(AddChild(new WEBSERVICE_EMITOR_OPENAPI(reader, parseContext)));
                    break;

                default:
                    throw new ParseException(reader.CurrentToken, "Except INDEX,WEBSERVICECONFIG,JC_PROXY,OPENAPI got " + reader.CurrentToken.Text.ToString() + ".");
                }
            }

            ParseToken(reader, Core.TokenID.RrBracket);
            n_Emitors = emitors.ToArray();
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Emitors.TranspileNodes(context);
        }

        public                  void                            EmitDrop(StringWriter stringWriter)
        {
            if (n_IndexProcedure != null) {
                stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                    stringWriter.Write(SqlStatic.QuoteString(n_IndexProcedure.Fullname));
                    stringWriter.WriteLine(") AND [type] in ('P'))");
                stringWriter.Write("    DROP PROCEDURE ");
                    stringWriter.WriteLine(n_IndexProcedure.Fullname);
            }
        }
        public                  bool                            EmitCode(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_Index != null) {
                emitContext.Database.Print("# create webmethod-index                " + n_IndexProcedure.Fullname);

                StringBuilder sqlStatement = new StringBuilder();

                sqlStatement.Append("CREATE PROCEDURE ");
                    sqlStatement.Append(n_IndexProcedure.Fullname);
                    sqlStatement.Append("\n");

                sqlStatement.Append("AS\n");
                sqlStatement.Append("BEGIN\n");
                sqlStatement.Append("    SET NOCOUNT,ANSI_NULLS,ANSI_PADDING,ANSI_WARNINGS,ARITHABORT,CONCAT_NULL_YIELDS_NULL,XACT_ABORT ON;\n");
                sqlStatement.Append("    SET NUMERIC_ROUNDABORT OFF;\n");
                sqlStatement.Append("    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;\n");
                sqlStatement.Append("    SELECT [*]=[name]+':'+[method]\n");
                sqlStatement.Append("      FROM (\n");

                int i = 0;

                foreach (var entiry in emitContext.Transpiler.EntityDeclarations) {
                    if (entiry.Declaration is WEBMETHOD webMethod && webMethod.DeclarationService == WebService) {
                        foreach (var method in webMethod.n_Declaration.n_Methods) {
                            sqlStatement.Append(' ', 17);
                            sqlStatement.Append(i++ == 0 ? "         " : "UNION ALL");
                            sqlStatement.Append(" SELECT [procname]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(webMethod.n_Declaration.n_EntityName.Fullname));
                                sqlStatement.Append(", [name]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(webMethod.n_Declaration.n_ServiceMethodName.n_Name.ValueString));
                                sqlStatement.Append(", [method]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(method));
                                sqlStatement.Append("\n");
                        }
                    }
                }

                sqlStatement.Append("           ) x\n");
                sqlStatement.Append("     WHERE (PERMISSIONS(OBJECT_ID([procname])) & 32) = 32\n");
                sqlStatement.Append("  ORDER BY [name]\n");
                sqlStatement.Append("   FOR XML PATH('value'),ROOT('root'),TYPE\n");
                sqlStatement.Append("END");

                if (emitContext.Database.ExecuteStatement(sqlStatement.ToString(), null, emitContext.AddEmitError) != 0)
                    return false;
            }

            return true;
        }
        public                  void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_Index != null) {
                emitContext.Database.ExecuteStatement("GRANT EXECUTE ON OBJECT::" + n_IndexProcedure.Fullname + " TO [public];", null, emitContext.AddEmitError);
            }
        }
        public                  void                            EmitServiceFiles(EmitContext emitContext, LTTSQL.Node.DeclarationServiceMethod[] methods, bool rebuild)
        {
            var emitors = new Emit.FileEmitor[n_Emitors.Length];
            for (int i = 0 ;  i < n_Emitors.Length ; ++i) {
                emitors[i] = n_Emitors[i].ConstructEmitor(emitContext.EmitOptions.BaseDirectory);
            }

            if (rebuild) {
                for (int i = 0 ;  i < n_Emitors.Length ; ++i) {
                    try {
                        emitors[i].CleanTarget();
                    }
                    catch(Exception err) {
                        emitContext.AddEmitError(new EmitError("failed to clean target." + err.Message));
                        return;
                    }
                }
            }


            foreach (Node.WEBMETHOD webMethod in methods) {
                for (int i = 0 ; i < emitors.Length ;  ++i) {
                    try {
                        emitors[i].AddWebMethod(webMethod);
                    }
                    catch(Exception err) {
                        emitContext.AddEmitError(_exceptionToEmitErro(emitContext, err));
                    }
                }
            }

            if (n_Index != null) {
                for (int i = 0 ; i < emitors.Length ;  ++i) {
                    try {
                        emitors[i].AddIndexMethod(n_Index, n_IndexProcedure.Fullname);
                    }
                    catch(Exception err) {
                        emitContext.AddEmitError(_exceptionToEmitErro(emitContext, err));
                    }
                }
            }

            for (int i = 0 ; i < emitors.Length ;  ++i) {
                emitors[i].Emit(emitContext);
            }
        }

        private                 EmitError          _exceptionToEmitErro(EmitContext emitContext, Exception err)
        {
            if (err is EmitError) {
                return (EmitError)err;
            }

            if (err is EmitException emitException) {
                if (emitException.Declaration is LTTSQL.DataModel.DocumentSpan documentSpan) {
                    return new EmitError(documentSpan.Filename,
                                         documentSpan.Beginning.Lineno,
                                         documentSpan.Beginning.Linepos,
                                         "Emit proxy error: " + err.Message);
                }

                else
                if (emitException.Declaration is LTTSQL.Core.IAstNode astNode) {
                    var token = astNode.GetFirstToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);
                    if (token != null) {
                        return new EmitError(emitContext.Transpiler.GetSourceFile(token).Filename,
                                             token.Beginning.Lineno,
                                             token.Beginning.Linepos,
                                             "Emit proxy error: " + err.Message);
                    }
                }
            }

            return new EmitError(WebService.EntityName.Fullname + ": Emit proxy error: " + err.Message);
        }
    }
}
