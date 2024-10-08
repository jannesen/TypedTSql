﻿using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms189484.aspx
    // Statement_SET_assign ::=
    //      SET @local_variable {+= | -= | *= | /= | %= | &= | ^= | |= } expression
    [StatementParser(Core.TokenID.Name, prio:1)]
    public class Statement_VAR: Statement
    {
        public      readonly    VarDeclareScope                     n_Scope;
        public      readonly    Token.TokenLocalName                n_VariableName;
        public      readonly    IExprNode                           n_Expression;
        public                  DataModel.VariableLocal             Variable            { get; private set; }

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("VAR", "LET");
        }
        public                                                      Statement_VAR(Core.ParserReader reader, IParseContext parseContext)
        {
            n_Scope = ParseToken(reader, "VAR", "LET").isToken("VAR") ? VarDeclareScope.CodeScope : VarDeclareScope.BlockScope;
            n_VariableName = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
            ParseToken(reader, Core.TokenID.Equal);
            n_Expression = ParseExpression(reader);

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            Variable = null;
            n_Expression.TranspileNode(context);
            Variable = context.VarVariableSet(n_VariableName, n_Scope, n_Expression.SqlType);
            n_VariableName.SetSymbolUsage(Variable.Symbol, DataModel.SymbolUsageFlags.Declaration | DataModel.SymbolUsageFlags.Write);
        }

        public      override    void                                Emit(Core.EmitWriter emitWriter)
        {
            bool    f = true;

            foreach(var node in this.Children) {
                if (f && node is Core.Token token && token.isToken(n_Scope == VarDeclareScope.CodeScope ? "VAR" : "LET")) {
                    emitWriter.WriteText("SET");
                    f = false;
                    continue;
                }

                node.Emit(emitWriter);
            }
        }
    }
}
