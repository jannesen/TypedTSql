using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.Language.TypedTSql.Core
{
    public class ParserReader
    {
        public struct ParsePosition: IEquatable<ParsePosition>
        {
            public              int         Processed;
            public              int         CurrentParse;

            public  static      bool        operator == (ParsePosition p1, ParsePosition p2)
            {
                return p1.Processed    == p2.Processed &&
                       p1.CurrentParse == p2.CurrentParse;
            }
            public  static      bool        operator != (ParsePosition p1, ParsePosition p2)
            {
                return !(p1 == p2);
            }
            public  override    bool        Equals(object obj)
            {
                if (obj is ParsePosition)
                    return this == (ParsePosition)obj;

                return false;
            }
            public              bool        Equals(ParsePosition o)
            {
                return this == o;
            }
            public  override    int         GetHashCode()
            {
                return Processed.GetHashCode() ^ CurrentParse.GetHashCode();
            }
        }

        public              Transpiler                  Transpiler          { get; private  set; }
        public              SourceFile                  SourceFile          { get; private  set; }
        public              Token[]                     Tokens              { get; private  set; }
        public              Node.Node_ParseOptions      Options             { get; private  set; }
        private             ParsePosition               _position;
        private             int                         _tokenCount;
        private             Token                       _eoftoken;

        public              ParsePosition               Position
        {
            get {
                return _position;
            }
        }
        public              Token                       CurrentToken
        {
            get {
                return (_position.CurrentParse >= 0 && _position.CurrentParse < _tokenCount) ? Tokens[_position.CurrentParse] : _getEofToken();
            }
        }

        public                                          ParserReader(Transpiler transpiler, SourceFile sourceFile, Token[] tokens)
        {
            Transpiler  = transpiler;
            SourceFile  = sourceFile;
            Tokens      = tokens;
            _tokenCount = tokens.Length;

            _position.Processed    = 0;
            _position.CurrentParse = _skipWhiteSpaceAndBlockComment(0);
        }

        public              Node.Node_ParseOptions      ParseOptions()
        {
            return Options = new Node.Node_ParseOptions(this);
        }

        public              Token                       NextPeek()
        {
            int idx = _skipWhiteSpaceAndBlockComment(_position.CurrentParse + 1);
            return (idx >= 0 && idx < _tokenCount) ?  Tokens[idx] : _getEofToken();
        }
        public              Token[]                     Peek(int count)
        {
            Token[]     rtn = new Token[count];

            int idx = _position.CurrentParse;

            for (int n = 0 ; n < count ; ++n) {
                rtn[n] = (idx >= 0 && idx < _tokenCount) ?  Tokens[idx] : _getEofToken();
                idx = _skipWhiteSpaceAndBlockComment(idx + 1);
            }

            return rtn;
        }

        public              void                        ReadBlanklines(AstParseNode node)
        {
            while (_position.Processed < _tokenCount && Tokens[_position.Processed].ID == TokenID.WhiteSpace && Tokens[_position.Processed].hasNewLine)
                _addTokens(node, _position.Processed+1);
        }
        public              void                        ReadLeading(AstParseNode node)
        {
            _addTokens(node, _position.CurrentParse);
        }
        public              Token                       ReadToken(AstParseNode node, bool dontReadNewLine=false)
        {
            if (_position.CurrentParse < _tokenCount) {
                Token       rtn = Tokens[_position.CurrentParse];

                _position.CurrentParse += 1;

                if (_position.CurrentParse < _tokenCount && Tokens[_position.CurrentParse].isWhitespaceOrComment && !dontReadNewLine) {
                    if (Tokens[_position.CurrentParse].hasNewLine) {
                        _position.CurrentParse += 1;
                    }
                    else
                    if (_position.CurrentParse + 1 < _tokenCount && Tokens[_position.CurrentParse].ID == TokenID.WhiteSpace && Tokens[_position.CurrentParse + 1].hasNewLine) {
                        _position.CurrentParse += 2;
                    }
                }

                _addTokens(node, _position.CurrentParse);

                _position.CurrentParse = _skipWhiteSpaceAndBlockComment(_position.CurrentParse);

                return rtn;
            }
            else
                return _getEofToken();
        }
        public              void                        AddError(Exception err)
        {
            SourceFile.AddParseMessage(new TypedTSqlParseError(SourceFile,
                                                               (err is ParseException ? ((ParseException)err).Token : CurrentToken),
                                                               err));
        }

        public              void                        _addTokens(AstParseNode node, int pos)
        {
            if (pos > _tokenCount)
                pos = _tokenCount;

            while (_position.Processed < pos)
                node.AddChild(Tokens[_position.Processed++]);
        }
        private             int                         _skipWhiteSpaceAndBlockComment(int idx)
        {
            while (idx < _tokenCount &&
                   (Tokens[idx].ID == TokenID.WhiteSpace || Tokens[idx].ID == TokenID.LineComment || Tokens[idx].ID == TokenID.BlockComment))
                ++idx;

            return idx;
        }
        private             Token                       _getEofToken()
        {
            if (_eoftoken == null) {
                var p = (_tokenCount>0) ? Tokens[_tokenCount -1].Ending : new Library.FilePosition(0, 0, 0);
                _eoftoken = Token.Create(TokenID.EOF, p, p, "");
            }

            return _eoftoken;
        }
    }
}
