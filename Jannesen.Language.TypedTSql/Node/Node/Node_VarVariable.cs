using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_VarVariable: AstParseNode, ISetVariable
    {
        public      readonly    Token.TokenLocalName                n_Name;

        public                  Token.TokenLocalName                TokenName           { get { return n_Name; } }
        public                  bool                                isVarDeclare        { get { return true;   } }

        public      static      bool                                CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("VAR") && reader.NextPeek().isToken(TokenID.LocalName);
        }
        public                                                      Node_VarVariable(Core.ParserReader reader)
        {
            ParseToken(reader, "VAR");
            n_Name = (Token.TokenLocalName)ParseToken(reader, TokenID.LocalName);
        }

        public      override    void                                Emit(EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (c is Token.Name name && name.isToken("VAR")) {
                    continue;
                }

                c.Emit(emitWriter);
            }
        }
    }
}
