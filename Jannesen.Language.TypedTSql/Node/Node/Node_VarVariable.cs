using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_VarVariable: AstParseNode, ISetVariable
    {
        public      readonly    VarDeclareScope                     n_Scope;
        public      readonly    Token.TokenLocalName                n_Name;

        public                  Token.TokenLocalName                TokenName           { get { return n_Name; } }
        public                  VarDeclareScope                     isVarDeclare        { get { return n_Scope;   } }

        public      static      bool                                CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("VAR", "LET") && reader.NextPeek().isToken(TokenID.LocalName);
        }
        public                                                      Node_VarVariable(Core.ParserReader reader)
        {
            n_Scope = ParseToken(reader, "VAR", "LET").isToken("VAR") ? VarDeclareScope.CodeScope : VarDeclareScope.BlockScope;
            n_Name = (Token.TokenLocalName)ParseToken(reader, TokenID.LocalName);
        }

        public      override    void                                Emit(EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (c is Token.Name name && name.isToken(n_Scope == VarDeclareScope.CodeScope ? "VAR" : "LET")) {
                    continue;
                }

                c.Emit(emitWriter);
            }
        }

        public      static      void                                EmitVarVariable(EmitWriter emitWriter, DataModel.VariableList variableList, int indent)
        {
            if (variableList != null) { 
                foreach(var v in variableList) {
                    if (v.isVarDeclare) {
                        emitWriter.WriteIndent(indent);
                        emitWriter.WriteText("DECLARE ");
                        emitWriter.WriteText(v.SqlName ?? v.Name);
                        emitWriter.WriteText(" ");

                        var sqlType = v.SqlType;

                        if (sqlType is DataModel.SqlTypeTable typeTable) {
                            int pos = emitWriter.Linepos;
                            emitWriter.WriteText("TABLE");
                            emitWriter.WriteNewLine(pos);
                            emitWriter.WriteText("(");

                            var fnext = false;
                            foreach (var c in typeTable.Columns) {
                                if (fnext) {
                                    emitWriter.WriteText(",");
                                }
                                emitWriter.WriteNewLine(pos + 4);
                                emitWriter.WriteText(Library.SqlStatic.QuoteName(c.Name));
                                emitWriter.WriteText(" ");
                                emitWriter.WriteText(c.SqlType.ToSql());

                                emitWriter.WriteText(c.isNullable ? " NULL" : " NOT NULL");
                                fnext = true;
                            }

                            emitWriter.WriteNewLine(pos);
                            emitWriter.WriteText(")");
                        }
                        else {
                            emitWriter.WriteText(v.SqlType.ToSql());
                        }

                        emitWriter.WriteText(";");
                        emitWriter.WriteNewLine(-1);
                    }
                }
            }
        }
    }
}
