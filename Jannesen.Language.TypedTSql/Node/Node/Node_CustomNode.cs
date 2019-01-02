using System;
using System.IO;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_CustomNode: Core.IAstNode
    {
        public                  string              NewCode { get; private set; }

        public                  Core.AstNodeList    Children
        {
            get {
                return null;
            }
        }
        public                  Core.IAstNode       Parent
        {
            get {
                return _parent;
            }
        }
        public                  bool                isWhitespaceOrComment
        {
            get {
                return false;
            }
        }

        private                 Core.IAstNode       _parent;

        public                                      Node_CustomNode(string newCode)
        {
            NewCode = newCode;
        }

        public                  Core.Token          GetFirstToken(Core.GetTokenMode mode)
        {
            return null;
        }
        public                  Core.Token          GetLastToken(Core.GetTokenMode mode)
        {
            return null;
        }
        public                  void                Emit(Core.EmitWriter streamWriter)
        {
            streamWriter.WriteText(NewCode);
        }
        public                  void                SetParent(Core.IAstNode parent)
        {
            _parent = parent;
        }
    }
}
