using System;
using System.Collections.Generic;
using System.IO;

namespace Jannesen.Language.TypedTSql.Core
{
    public enum GetTokenMode
    {
        AllToken                = 0,
        RemoveWhiteSpace,
        RemoveWhiteSpaceAndComment
    }

    public interface IAstNode
    {
        AstNodeList     Children                { get; }
        IAstNode        ParentNode              { get; }
        bool            isWhitespaceOrComment   { get; }

        Token               GetFirstToken(GetTokenMode mode);
        Token               GetLastToken(GetTokenMode mode);
        void                Emit(EmitWriter emitWriter);

        void                SetParentNode(IAstNode parent);
    }

    public class AstNodeList: List<IAstNode>
    {
        public                      void                Emit(EmitWriter emitWriter)
        {
            foreach(var node in this)
                node.Emit(emitWriter);
        }

        public                      Token               FirstNoWhithspaceToken
        {
            get {
                for (int i = 0 ; i < this.Count ; ++i) {
                    if ((this[i] is Token) && !((Token)this[i]).isWhitespaceOrComment)
                        return (Token)this[i];

                    if (this[i] is AstParseNode) {
                        AstNodeList c = ((AstParseNode)this[i]).Children;
                        if (c != null) {
                            Token       t = c.FirstNoWhithspaceToken;
                            if (t != null)
                                return t;
                        }
                    }
                }

                return null;
            }
        }
    }
}
