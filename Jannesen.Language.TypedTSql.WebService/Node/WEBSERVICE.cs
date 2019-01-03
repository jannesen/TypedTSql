using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    [LTTSQL.Library.DeclarationParser("WEBSERVICE")]
    public class WEBSERVICE: LTTSQL.Node.DeclarationService
    {
        public class TypeMap: LTTSQL.Core.AstParseNode
        {
            public      readonly    TypeMapEntry[]                                          n_Entrys;

            public                  Emit.TypeMapDictionary                                  TypeDictionary      { get; private set; }

            public                                                                          TypeMap(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, "TYPEMAP");
                ParseToken(reader, Core.TokenID.LrBracket);

                var entries = new List<TypeMapEntry>();

                do {
                    entries.Add(AddChild(new TypeMapEntry(reader)));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);
                n_Entrys = entries.ToArray();
            }

            public      override    void                                                    TranspileNode(LTTSQL.Transpile.Context context)
            {
                TypeDictionary = null;

                n_Entrys.TranspileNodes(context);

                var typeDictionary = new Emit.TypeMapDictionary();

                foreach (var entry in n_Entrys) {
                    var sqlType = entry.n_Datatype.SqlType;

                    if (sqlType != null) {
                        if (!typeDictionary.TryGetValue(sqlType, out var found)) {
                            try {
                                typeDictionary.Add(sqlType, new Emit.FromExpression(entry.n_TypeScriptType.ValueString));
                            }
                            catch(Exception err) {
                                context.AddError(entry.n_TypeScriptType, err);
                            }
                        }
                        else
                            context.AddError(entry, "Duplicate declaration of '" + sqlType.ToString() + "'.");
                    }
                }

                TypeDictionary = typeDictionary;
            }
        }

        public class TypeMapEntry: LTTSQL.Core.AstParseNode
        {
            public      readonly    LTTSQL.Node.Node_Datatype           n_Datatype;
            public      readonly    LTTSQL.Token.DataIsland             n_TypeScriptType;

            public                                                      TypeMapEntry(LTTSQL.Core.ParserReader reader)
            {
                n_Datatype = AddChild(new LTTSQL.Node.Node_Datatype(reader));
                ParseToken(reader, LTTSQL.Core.TokenID.AS);
                n_TypeScriptType = (LTTSQL.Token.DataIsland)ParseToken(reader, LTTSQL.Core.TokenID.DataIsland);
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_Datatype.TranspileNode(context);
            }
        }

        public      readonly    string                          n_EmitBasePath;
        public      readonly    string                          n_WebConfigDatabase;
        public      readonly    string                          n_BaseUrl;
        public      readonly    string                          n_Index;
        public      readonly    DataModel.EntityName            n_IndexProcedure;
        public      readonly    TypeMap                         n_TypeMap;

        public                                                  WEBSERVICE(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext): base(reader)
        {
            if (ParseOptionalToken(reader, "EMITBASEPATH") != null) {
                ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                n_EmitBasePath = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
            }

            if (ParseOptionalToken(reader, "DATABASE") != null) {
                ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                n_WebConfigDatabase = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
            }

            if (ParseOptionalToken(reader, "BASEURL") != null) {
                ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                n_BaseUrl = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
            }

            if (ParseOptionalToken(reader, "INDEX") != null) {
                ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                n_Index          = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                n_IndexProcedure = new DataModel.EntityName(n_Name.n_EntitiyName.Schema, n_Name.n_EntitiyName.Name + "/" + n_Index + ":GET");
            }

            if (reader.CurrentToken.isToken("TYPEMAP"))
                n_TypeMap = new TypeMap(reader);
        }

        public      override    bool                            IsMember(LTTSQL.Node.DeclarationObjectCode entity)
        {
            return entity is WEBMETHOD || entity is WEBCOMPLEXTYPE;
        }
        public      override    object                          TranspilseNodeAS(LTTSQL.Node.Node_AS node)
        {
            return new Emit.FromExpression(node.n_AsType.ValueString);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_TypeMap?.TranspileNode(context);
        }
        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            if (n_IndexProcedure != null) {
                stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                    stringWriter.Write(Library.SqlStatic.QuoteString(n_IndexProcedure.Fullname));
                    stringWriter.WriteLine(") AND [type] in ('P'))");
                stringWriter.Write("    DROP PROCEDURE ");
                    stringWriter.WriteLine(n_IndexProcedure.Fullname);
            }
        }
        public      override    bool                            EmitCode(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_Index != null) {
                emitContext.Database.Print("# create webmethod-index                " + n_IndexProcedure.Fullname);

                StringBuilder sqlStatement = new StringBuilder();

                sqlStatement.Append("CREATE PROCEDURE ");
                    sqlStatement.Append(n_IndexProcedure.Fullname);
                    sqlStatement.Append("\r\n");

                sqlStatement.Append("AS\r\n");
                sqlStatement.Append("BEGIN\r\n");
                sqlStatement.Append("    SET NOCOUNT,ANSI_NULLS,ANSI_PADDING,ANSI_WARNINGS,ARITHABORT,CONCAT_NULL_YIELDS_NULL,XACT_ABORT ON;\r\n");
                sqlStatement.Append("    SET NUMERIC_ROUNDABORT OFF;\r\n");
                sqlStatement.Append("    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;\r\n");
                sqlStatement.Append("    SELECT [*]=[name]+':'+[method]\r\n");
                sqlStatement.Append("      FROM (\r\n");

                int i = 0;

                foreach (var entiry in emitContext.Transpiler.EntityDeclarations) {
                    if (entiry.Declaration is WEBMETHOD webMethod && webMethod.DeclarationService == this) {
                        foreach (var method in webMethod.n_Declaration.n_Methods) {
                            sqlStatement.Append(' ', 17);
                            sqlStatement.Append(i++ == 0 ? "         " : "UNION ALL");
                            sqlStatement.Append(" SELECT [procname]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(webMethod.n_Declaration.n_EntityName.Fullname));
                                sqlStatement.Append(", [name]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(webMethod.n_Declaration.n_ServiceMethodName.n_Name.ValueString));
                                sqlStatement.Append(", [method]=");
                                sqlStatement.Append(SqlStatic.QuoteNString(method));
                                sqlStatement.Append("\r\n");
                        }
                    }
                }

                sqlStatement.Append("           ) x\r\n");
                sqlStatement.Append("     WHERE (PERMISSIONS(OBJECT_ID([procname])) & 32) = 32\r\n");
                sqlStatement.Append("  ORDER BY [name]\r\n");
                sqlStatement.Append("   FOR XML PATH('value'),ROOT('root'),TYPE\r\n");
                sqlStatement.Append("END");

                if (emitContext.Database.ExecuteStatement(sqlStatement.ToString(), null, emitContext.AddEmitError) != 0)
                    return false;
            }

            return true;
        }
        public      override    void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_Index != null) {
                emitContext.Database.ExecuteStatement("GRANT EXECUTE ON OBJECT::" + n_IndexProcedure.Fullname + " TO [public];", null, emitContext.AddEmitError);
            }
        }
        public      override    void                            EmitServiceFiles(EmitContext emitContext, LTTSQL.Node.DeclarationServiceMethod[] methods, bool rebuild)
        {
            var baseEmitDirectory = Path.Combine(emitContext.EmitOptions.BaseDirectory, n_EmitBasePath);

            if (rebuild)
                _cleanwebtarget(emitContext, baseEmitDirectory, false);

            var webConfigEmitor = new Emit.WebConfigEmitor(baseEmitDirectory, n_WebConfigDatabase);
            var proxyEmitor     = new Emit.ProxyEmitor();

            foreach (Node.WEBMETHOD webMethod in methods) {
                webConfigEmitor.AddWebMethod(webMethod);

                if (webMethod.n_Declaration.Proxy != null) {
                    try {
                        proxyEmitor.AddMethod(this, baseEmitDirectory, webMethod);
                    }
                    catch(Exception err) {
                        EmitError emitError = null;

                        if (err is EmitError) {
                            emitError = (EmitError)err;
                        }
                        else
                        if (err is EmitException emitException) {
                            if (emitException.Declaration is LTTSQL.DataModel.DocumentSpan documentSpan) {
                                emitError = new EmitError(documentSpan.Filename,
                                                            documentSpan.Beginning.Lineno,
                                                            documentSpan.Beginning.Linepos,
                                                            "Emit proxy error: " + err.Message);
                            }
                            else
                            if (emitException.Declaration is LTTSQL.Core.IAstNode astNode) {
                                var token = astNode.GetFirstToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);
                                if (token != null) {
                                    emitError = new EmitError(emitContext.Transpiler.GetSourceFile(token).Filename,
                                                                token.Beginning.Lineno,
                                                                token.Beginning.Linepos,
                                                                "Emit proxy error: " + err.Message);
                                }
                            }
                        }

                        if (emitError == null) {
                            emitError = new EmitError(EntityName.Fullname + ": Emit proxy error: " + err.Message);
                        }

                        emitContext.AddEmitError(emitError);
                    }
                }
            }

            if (n_Index != null)
                webConfigEmitor.AddIndexMethod(n_Index, n_IndexProcedure.Fullname);

            webConfigEmitor.Emit(emitContext);
            proxyEmitor.Emit(emitContext);
        }

        public      override    string                          CollapsedName()
        {
            return "webservice " + n_Name.n_EntitiyName.Name;
        }

        private                 void                            _cleanwebtarget(EmitContext emitContext, string directory, bool child)
        {
            try {
                if (child || Directory.Exists(directory)) {
                    foreach (string path in Directory.GetDirectories(directory)) {
                        _cleanwebtarget(emitContext, path, true);
                        _delete(path, true);
                    }

                    if (File.Exists(directory + "\\jannesen.web.config"))
                        _delete(directory + "\\jannesen.web.config", false);

                    foreach(var filename in Directory.GetFiles(directory, "*.ts", SearchOption.TopDirectoryOnly))
                        _delete(filename, false);
                }
            }
            catch(Exception err) {
                emitContext.AddEmitError(new EmitError("failed to clean directory '" + directory + "': " + err.Message));
            }
        }
        private     static      void                            _delete(string name, bool directory)
        {
            for (int i = 0 ;  ; ++i) {
                try {
                    if (directory)
                        Directory.Delete(name);
                    else
                        File.Delete(name);
                    return;
                }
                catch(IOException err) {
                    if (err is FileNotFoundException || err is DirectoryNotFoundException)
                        return;

                    System.Diagnostics.Debug.WriteLine("_delete(" + name + "): " + err.Message);
                    if (i > 100 || !_errorretry(err.HResult))
                        throw;
                }

                System.Threading.Thread.Sleep(100);
            }
        }
        private     static      bool                            _errorretry(int hresult)
        {
            return hresult == unchecked((int)0x80070020);
        }
    }
}
