using System;
using System.Collections.Generic;
using System.IO;

namespace Jannesen.Language.TypedTSql.Core
{
    public struct TokenNameID
    {
        public      string      Name;
        public      int         Id;

        public                  TokenNameID(string name, int id)
        {
            this.Name = name;
            this.Id   = id;
        }
    }

    public abstract class AstParseNode: IAstNode
    {
        public delegate void CustomEmitor(EmitWriter emitWriter);

        private static          Node.Node_CustomNode                Semicolon = new Node.Node_CustomNode(";");

        public                  AstNodeList                         Children
        {
            get {
                return _children;
            }
        }
        public                  IAstNode                            Parent
        {
            get {
                return _parent;
            }
        }
        public                  bool                                isWhitespaceOrComment
        {
            get {
                return false;
            }
        }

        private                 AstNodeList                         _children;
        private                 IAstNode                            _parent;

        public                                                      AstParseNode()
        {
        }

        public      virtual     Token                               GetFirstToken(GetTokenMode mode)
        {
            if (_children != null) {
                for (int i = 0 ; i < _children.Count ; ++i) {
                    var n = _children[i].GetFirstToken(mode);
                    if (n != null)
                        return n;
                }
            }

            return null;
        }
        public      virtual     Token                               GetLastToken(GetTokenMode mode)
        {
            if (_children != null) {
                for (int i = _children.Count - 1 ; i >= 0 ; --i) {
                    var n = _children[i].GetLastToken(mode);
                    if (n != null)
                        return n;
                }
            }

            return null;
        }
        public      virtual     void                                TranspileNode(Transpile.Context context)
        {
            throw new InvalidOperationException("Transpile not implemented for " + this.GetType().FullName + ".");
        }
        public      virtual     void                                Emit(EmitWriter emitWriter)
        {
            if (_children != null) {
                _children.Emit(emitWriter);
            }
        }
        public                  void                                EmitCustom(EmitWriter emitWriter, CustomEmitor customEmitor, bool emitCodeAsComment = true)
        {
            EmitWriterArray emitArray = new EmitWriterArray(emitWriter.EmitContext);

            if (_children != null)
                _children.Emit(emitArray);

            int tokenindex_before_start;
            int tokenindex_code_start;
            int tokenindex_after_start;
            int tokenindex_after_end;

            tokenindex_before_start = tokenindex_code_start = 0;

            while (tokenindex_code_start < emitArray.Nodes.Count && emitArray.Nodes[tokenindex_code_start].isWhitespaceOrComment)
                ++tokenindex_code_start;

            tokenindex_after_end = tokenindex_after_start = emitArray.Nodes.Count;

            while (tokenindex_after_start > tokenindex_code_start && emitArray.Nodes[tokenindex_after_start - 1].isWhitespaceOrComment)
                --tokenindex_after_start;

            for (int i = tokenindex_before_start; i < tokenindex_code_start; ++i)
                emitArray.Nodes[i].Emit(emitWriter);

            if ((!emitWriter.EmitOptions.DontEmitCustomComment) && emitCodeAsComment && tokenindex_code_start < tokenindex_after_start) {
                EmitWriterString emitCommentWriter = new EmitWriterString(emitWriter.EmitContext);

                for (int i = tokenindex_code_start; i < tokenindex_after_start; ++i)
                    emitArray.Nodes[i].Emit(emitCommentWriter);

                emitWriter.WriteText("/*" + emitCommentWriter.String.Replace("/*", "**").Replace("*/", "**") + "*/");
            }

            customEmitor(emitWriter);

            for (int i = tokenindex_after_start; i < tokenindex_after_end; ++i)
                emitArray.Nodes[i].Emit(emitWriter);
        }
        public                  void                                EmitCommentNewine(EmitWriter emitWriter)
        {
            if (_children != null) {
                int i = 0;
                int e = _children.Count;

                while (i < e && _emitCommentNewineSkip(_children[i]))
                    ++i;

                while (e > 0 && _emitCommentNewineSkip(_children[e-1]))
                    --e;

                while (i < e) {
                    if (_children[i].isWhitespaceOrComment)
                        _children[i].Emit(emitWriter);

                    ++i;
                }
            }
        }

        public                  Token                               ParseToken(ParserReader reader, TokenID id)
        {
            reader.CurrentToken.validateToken(id);
            return ParseToken(reader);
        }
        public                  Token                               ParseToken(ParserReader reader, params TokenID[] ids)
        {
            reader.CurrentToken.validateToken(ids);
            return ParseToken(reader);
        }
        public                  Token                               ParseToken(ParserReader reader, string name)
        {
            reader.CurrentToken.validateToken(name);
            return TokenWithSymbol.SetKeyword(ParseToken(reader));
        }
        public                  Token                               ParseToken(ParserReader reader, params string[] names)
        {
            reader.CurrentToken.validateToken(names);
            return TokenWithSymbol.SetKeyword(ParseToken(reader));
        }
        public                  int                                 ParseToken(ParserReader reader, TokenNameID[] namedIDs)
        {
            var n = reader.CurrentToken.validateToken(namedIDs);
            TokenWithSymbol.SetKeyword(ParseToken(reader));
            return namedIDs[n].Id;
        }
        public                  Token                               ParseOptionalToken(ParserReader reader, TokenID id)
        {
            return reader.CurrentToken.isToken(id) ? ParseToken(reader) : null;
        }
        public                  Token                               ParseOptionalToken(ParserReader reader, params TokenID[] ids)
        {
            return reader.CurrentToken.isToken(ids) ? ParseToken(reader) : null;
        }
        public                  Token                               ParseOptionalToken(ParserReader reader, string name)
        {
            if (reader.CurrentToken.isToken(name))
                return TokenWithSymbol.SetKeyword(ParseToken(reader));

            return null;
        }
        public                  Token                               ParseOptionalToken(ParserReader reader, params string[] names)
        {
            if (reader.CurrentToken.isToken(names))
                return TokenWithSymbol.SetKeyword(ParseToken(reader));

            return null;
        }
        public                  int                                 ParseOptionalToken(ParserReader reader, TokenNameID[] nameIDs)
        {
            var n = reader.CurrentToken.isToken(nameIDs);

            if (n < 0)
                return 0;

            TokenWithSymbol.SetKeyword(ParseToken(reader));

            return nameIDs[n].Id;
        }
        public                  T                                   ParseEnum<T>(ParserReader reader, Core.ParseEnum<T> parseEnum)
        {
            return parseEnum.Parse(this, reader);
        }
        public                  TokenWithSymbol                     ParseName(Core.ParserReader reader)
        {
            return (TokenWithSymbol)ParseToken(reader, TokenID.Name, TokenID.QuotedName);
        }

        public                  Token                               ParseInteger(ParserReader reader)
        {
            var token = ParseToken(reader, TokenID.Number);

            token.validateInteger();

            return token;
        }
        public                  Node.IExprNode                      ParseExpression(Core.ParserReader reader)
        {
            return AddChild(Node.Expr.Parse(reader, Node.ParseExprContext.Normal));
        }
        public                  Node.IExprNode                      ParseExpression(Core.ParserReader reader, Node.ParseExprContext context)
        {
            return AddChild(Node.Expr.Parse(reader, context));
        }
        public                  Node.IExprNode                      ParseSimpleExpression(Core.ParserReader reader, bool constValue=false)
        {
            var expr = Node.Expr.Parse(reader, Node.ParseExprContext.Normal);

            var type = expr.ExpressionType;

            if (!(type == Node.ExprType.Const || type == Node.ExprType.Variable))
                expr = new Node.Expr_SimpleWrapper(expr, constValue);

            return AddChild(expr);
        }
        public                  void                                ParseStatementEnd(Core.ParserReader reader, bool autoaddsemi = true)
        {
            if (reader.CurrentToken.isToken(TokenID.Semicolon))
                ParseToken(reader);
            else
                AddBeforeWhitespace(autoaddsemi ? Semicolon : null);
        }

        public                  Token                               ParseToken(Core.ParserReader reader)
        {
            return reader.ReadToken(this);
        }

        public                  T                                   AddChild<T>(T child) where T : IAstNode
        {
            if (_children == null)
                _children = new AstNodeList();

            _children.Add(child);
            child.SetParent(this);

            return child;
        }
        public                  void                                AddLeading(Core.ParserReader reader)
        {
            reader.ReadLeading(this);
        }
        public                  void                                AddBeforeWhitespace(IAstNode child)
        {
            int i = _children.Count;

            while (i > 0 && _children[i - 1].isWhitespaceOrComment)
                --i;

            int toIndex = i;

            if (child != null)
                _children.Insert(toIndex++, child);

            if (i > 0 && _children[i - 1] is AstParseNode)
                _moveWhitespaceOrComment((AstParseNode)_children[i - 1], _children, ref toIndex);
        }
        public                  void                                InsertBefore(IAstNode curNode, Node.Node_CustomNode node)
        {
            if (_children == null)
                _children = new AstNodeList();

            _children.Insert(_children.IndexOf(curNode), node);
            node.SetParent(this);
        }
        public                  void                                InsertRangeChild(int index, List<Node.Node_CustomNode> nodes)
        {
            if (_children == null)
                _children = new AstNodeList();

            _children.InsertRange(index, nodes);

            foreach(var n in nodes)
                n.SetParent(this);
        }

        public                  void                                SetParent(IAstNode parent)
        {
            _parent = parent;
        }

        private                 void                                _moveWhitespaceOrComment(AstParseNode node, AstNodeList toList, ref int toIndex)
        {
            if (node._children != null) {
                int i = node._children.Count;

                while (i > 0 && node._children[i - 1].isWhitespaceOrComment)
                    --i;

                if (i > 0 && node._children[i - 1] is AstParseNode)
                    _moveWhitespaceOrComment((AstParseNode)node._children[i - 1], toList, ref toIndex);

                while (i < node._children.Count) {
                    toList.Insert(toIndex++, node._children[i]);
                    node._children.RemoveAt(i);
                }
            }
        }
        private     static      bool                                _emitCommentNewineSkip(IAstNode node)
        {
            if (node.isWhitespaceOrComment) {
                if (node is Jannesen.Language.TypedTSql.Token.LineComment  ||
                    node is Jannesen.Language.TypedTSql.Token.BlockComment ||
                    (node is Jannesen.Language.TypedTSql.Token.WhiteSpace whitespace && whitespace.hasNewLine))
                    return false;
            }

            return true;
        }
    }
}
