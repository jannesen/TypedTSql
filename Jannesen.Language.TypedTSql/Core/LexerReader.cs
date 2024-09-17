using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jannesen.Language.TypedTSql.Core
{
    public class LexerReader
    {
        class TextID
        {
            public  string          Text;
            public  Core.TokenID    ID;

            public                  TextID(string text, Core.TokenID id)
            {
                this.Text = text;
                this.ID   = id;
            }
        }
        private     static  Dictionary<string, TextID>          _nameDictionary = _initKeywords();

        private             SourceFile                          _sourceFile;
        private             string                              _text;
        private             Library.FilePosition                _beginpos;
        private             int                                 _curpos;

        public                                                  LexerReader(SourceFile sourceFile, string text, Library.FilePosition startPos)
        {
            _sourceFile = sourceFile;
            _text       = text;
            _beginpos   = startPos;
        }

        public              Core.Token                          ReadToken()
        {
            _curpos = _beginpos.Filepos;

            int c = _charAt(_curpos++);

            if (_isWhiteSpace(c))                                                   return _readWhiteSpace();

            if (_isNameStart(c)) {
                if ((c == 'N' || c == 'n') && _charAt(_curpos) == '\'') {
                    ++_curpos;
                    return _readString(Core.TokenID.String, '\'');
                }

                return _readName();
            }

            if (c == '@')                                                           return _readLocalName();
            if (c == '[')                                                           return _readQuoted();
            if (c == '"')                                                           return _readString(Core.TokenID.QuotedName, '"');
            if (c == '\'')                                                          return _readString(Core.TokenID.String,     '\'');
            if (c == '0' && (_charAt(_curpos) == 'X' || _charAt(_curpos) == 'x'))   return _readBinary();
            if (c == '`')                                                           return _readDataIsland();
            if (_isDigit(c))                                                        return _readNumber();
            if (c == '\n')                                                          return _newToken(Core.TokenID.WhiteSpace);
            if (c == '-' && _charAt(_curpos) == '-')                                return _readLineComment();
            if (c == '/' && _charAt(_curpos) == '*')                                return _readBlockComment();

            if (_charAt(_curpos) == '=') {
                switch(c) {
                case '>':   ++_curpos;  return _newToken(Core.TokenID.GreaterEqual);
                case '<':   ++_curpos;  return _newToken(Core.TokenID.LessEqual);
                case '+':   ++_curpos;  return _newToken(Core.TokenID.PlusAssign);
                case '-':   ++_curpos;  return _newToken(Core.TokenID.MinusAssign);
                case '*':   ++_curpos;  return _newToken(Core.TokenID.MultAssign);
                case '/':   ++_curpos;  return _newToken(Core.TokenID.DivAssign);
                case '%':   ++_curpos;  return _newToken(Core.TokenID.ModAssign);
                case '&':   ++_curpos;  return _newToken(Core.TokenID.AndAssign);
                case '^':   ++_curpos;  return _newToken(Core.TokenID.XorAssign);
                case '|':   ++_curpos;  return _newToken(Core.TokenID.OrAssign);

                case '=':
                    if (_charAt(_curpos+1) == '=') {
                        _curpos += 2;
                        return _newToken(Core.TokenID.DistinctEqual);
                    }
                    break;

                case '!':
                    if (_charAt(_curpos+1) == '=') {
                        _curpos += 2;
                        return _newToken(Core.TokenID.DistinctNotEqual);
                    }
                    else {
                        _curpos += 1;
                        return _newToken(Core.TokenID.NotEqual);
                    }                    
                }
            }

            if (c == '<' && _charAt(_curpos) == '>') {
                ++_curpos;
                return _newToken(Core.TokenID.NotEqual);
            }

            switch(c) {
            case '=':       return _newToken(Core.TokenID.Equal);
            case '>':       return _newToken(Core.TokenID.Greater);
            case '<':       return _newToken(Core.TokenID.Less);
            case '!':       return _newToken(Core.TokenID.Exclamation);
            case '.':       return _newToken(Core.TokenID.Dot);
            case '(':       return _newToken(Core.TokenID.LrBracket);
            case ')':       return _newToken(Core.TokenID.RrBracket);
            case ',':       return _newToken(Core.TokenID.Comma);
            case ';':       return _newToken(Core.TokenID.Semicolon);
            case '*':       return _newToken(Core.TokenID.Star);
            case '/':       return _newToken(Core.TokenID.Divide);
            case '%':       return _newToken(Core.TokenID.Module);
            case '+':       return _newToken(Core.TokenID.Plus);
            case '-':       return _newToken(Core.TokenID.Minus);
            case '~':       return _newToken(Core.TokenID.BitNot);
            case '|':       return _newToken(Core.TokenID.BitOr);
            case '&':       return _newToken(Core.TokenID.BitAnd);
            case '^':       return _newToken(Core.TokenID.BitXor);
            case ':':
                if (_charAt(_curpos) == ':') {
                    ++_curpos;
                    return _newToken(Core.TokenID.DoubleColon);
                }

                return _newToken(Core.TokenID.Colon);
            case -1:        return null;
            }

            return _newToken(Core.TokenID.InvalidCharacter);
        }

        private             Core.Token                          _readWhiteSpace()
        {
            while (_isWhiteSpace(_charAt(_curpos)))
                ++_curpos;

            if (_charAt(_curpos) == '\n')
                ++_curpos;

            return _newToken(Core.TokenID.WhiteSpace);
        }
        private             Core.Token                          _readName()
        {
            while (_isName(_charAt(_curpos)))
                ++_curpos;

            string                  text      = _text.Substring(_beginpos.Filepos, _curpos - _beginpos.Filepos);
            Library.FilePosition    beginning = _beginpos;
            _beginpos.Filepos =  _curpos;
            _beginpos.Linepos += text.Length;

            TextID  textID;

            lock(_nameDictionary) {
                if (!_nameDictionary.TryGetValue(text, out textID))
                    _nameDictionary.Add(text, textID = new TextID(text, _nameDictionary.TryGetValue(text.ToUpperInvariant(), out var upperTextID) ? upperTextID.ID : TokenID.Name));
            }

            return Core.Token.Create(textID.ID, beginning, _beginpos, textID.Text);
        }
        private             Core.Token                          _readLocalName()
        {
            while (_isName(_charAt(_curpos)))
                ++_curpos;

            if (_curpos - _beginpos.Filepos < 2)
                return _newToken(Core.TokenID.InvalidCharacter);

            return _newToken(Core.TokenID.LocalName);
        }
        private             Core.Token                          _readQuoted()
        {
            int             c;

            while ((c = _charAt(_curpos++)) != -1 && c != ']') {
                if (c < 32) {
                    _curpos--;
                    break;
                }

                if (c == '[')
                    _charAt(_curpos);
            }

            var token = _newToken(Core.TokenID.QuotedName);

            if (c != ']')
                _sourceFile.AddParseMessage(new TypedTSqlParseError(_sourceFile, token, new ParseException(token, "Invalid character in quoted-name.")));

            return token;
        }
        private             Core.Token                          _readString(Core.TokenID id, char quotechar)
        {
            int     c;

            while ((c = _charAt(_curpos++)) != -1) {
                if (c < 32) {
                    _curpos--;
                    break;
                }

                if (c == quotechar) {
                    if (_charAt(_curpos) == quotechar) {
                        ++_curpos;
                    }
                    else
                        break;
                }
            }

            var token = _newToken(id);

            if (c != quotechar)
                _sourceFile.AddParseMessage(new TypedTSqlParseError(_sourceFile, token, new ParseException(token, "Invalid character in string.")));

            return token;
        }
        private             Core.Token                          _readLineComment()
        {
            int     c;

            while ((c = _charAt(_curpos++)) != -1 && c != '\n')
                ;

            return _newToken(Core.TokenID.LineComment);
        }
        private             Core.Token                          _readBlockComment()
        {
            int     c;

            ++_curpos;

            while ((c = _charAt(_curpos++)) >= 0) {
                if (c == '*' && _charAt(_curpos) == '/') {
                    ++_curpos;
                    break;
                }
            }

            return _newToken(Core.TokenID.BlockComment);
        }
        private             Core.Token                          _readBinary()
        {
            ++_curpos;

            while (_isHexDigit(_charAt(_curpos)))
                ++_curpos;

            return _newToken(Core.TokenID.BinaryValue);
        }
        private             Core.Token                          _readDataIsland()
        {
            int             c;
            char[]          post;

            {
                List<char>      prefix = new List<char>();

                while ((c = _charAt(_curpos++)) != -1 && c != '[')
                    prefix.Add((char)c);
                prefix.Add(']');

                post = prefix.ToArray();
                Array.Reverse(post);
            }

            while ((c = _charAt(_curpos++)) != -1) {
                if (c == '`' && _tokentextEndsWith(post))
                    break;
            }

            return _newToken(Core.TokenID.DataIsland);
        }
        private             Core.Token                          _readNumber()
        {
            while (_isDigit(_charAt(_curpos)))
                ++_curpos;

            if (_charAt(_curpos) == '.') {
                ++_curpos;

                while (_isDigit(_charAt(_curpos)))
                    ++_curpos;
            }

            if (_charAt(_curpos) == 'E' || _charAt(_curpos) == 'e') {
                ++_curpos;

                if (_charAt(_curpos) == '+' || _charAt(_curpos) == '-')
                    ++_curpos;

                while (_isDigit(_charAt(_curpos)))
                    ++_curpos;
            }

            return _newToken(Core.TokenID.Number);
        }

        private             Core.Token                          _newToken(Core.TokenID id)
        {
            if (_curpos > _text.Length)
                _curpos = _text.Length;

            string                  text      = _text.Substring(_beginpos.Filepos, _curpos - _beginpos.Filepos);

            Library.FilePosition    beginning = _beginpos;

            _beginpos.Filepos =  _curpos;

            for (int i = 0 ; i < text.Length ; ++i) {
                if (text[i] == '\n') {
                    _beginpos.Lineno++;
                    _beginpos.Linepos = 1;
                }
                else
                    _beginpos.Linepos++;
            }

            return Core.Token.Create(id, beginning, _beginpos, text);
        }

        private             int                                 _charAt(int pos)
        {
            return (pos < _text.Length) ? _text[pos] : -1;
        }
        private             bool                                _tokentextEndsWith(char[] endswith)
        {
            int p = (_curpos-1) - endswith.Length;
            if (p < 0)
                return false;

            for(int i = 0 ; i < endswith.Length ; ++i) {
                if (_text[p] != endswith[i])
                    return false;

                ++p;
            }

            return true;
        }

        private     static  bool                                _isWhiteSpace(int c)
        {
            return c == ' '  ||
                   c == '\r' ||
                   c == '\t';
        }
        private     static  bool                                _isDigit(int c)
        {
            return ('0' <= c && c <= '9');
        }
        private     static  bool                                _isHexDigit(int c)
        {
            return ('0' <= c && c <= '9') ||
                   ('A' <= c && c <= 'F') ||
                   ('a' <= c && c <= 'f');
        }
        private     static  bool                                _isNameStart(int c)
        {
            return ('a' <= c && c <= 'z') ||
                   ('A' <= c && c <= 'Z') ||
                   c == '_'               ||
                   c == '#';
        }
        private     static  bool                                _isName(int c)
        {
            return ('a' <= c && c <= 'z') ||
                   ('A' <= c && c <= 'Z') ||
                   ('0' <= c && c <= '9') ||
                   c == '_'               ||
                   c == '$'               ||
                   c == '#'               ||
                   c == '@';
        }

        private     static  Dictionary<string, TextID>          _initKeywords()
        {
            var     rtn = new Dictionary<string, TextID>(65111);

            foreach (Core.TokenID tokenId in Enum.GetValues(typeof(Core.TokenID))) {
                if ((Core.TokenID._beginkeywords           < tokenId && tokenId < Core.TokenID._endkeywords          ) ||
                    (Core.TokenID._beginkeywordswithsymbol < tokenId && tokenId < Core.TokenID._endkeywordswithsymbol))
                {
                    var x = tokenId.ToString();
                    rtn.Add(x, new TextID(x, tokenId));
                }
            }

            return rtn;
        }
    }
}
