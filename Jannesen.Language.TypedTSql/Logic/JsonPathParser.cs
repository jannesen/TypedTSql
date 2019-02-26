using System;
using System.Text;

namespace Jannesen.Language.TypedTSql.Logic
{
    public class JsonPathParser
    {
        enum TokenId
        {
            EOP             = -1,
            Name            = 1,
            QuotedName,
            Integer,
            DollarSign,
            Dot,
            OpeningBracket,
            CloseBracket,
        }

        struct Token
        {
            public  TokenId             id;
            public  string              value;
        }

        private     string                  _path;
        private     int                     _pos;
        private     DataModel.JsonSchema    _schema;

        public                              JsonPathParser()
        {
        }

        public      DataModel.JsonSchema    Parse(DataModel.JsonSchema schema, string path)
        {
            _path   = path;
            _pos    = 0;
            _schema = schema;

            if (_readToken().id != TokenId.DollarSign)
                throw new FormatException("json-path must start with a $");

            Token currentToken;

            for (;;) {
                switch((currentToken = _readToken()).id) {
                case TokenId.EOP:
                    return _schema;

                case TokenId.Dot:
                    currentToken = _readToken();

                    if (currentToken.id == TokenId.Name || currentToken.id == TokenId.QuotedName)
                        _selectObject(currentToken.value);
                    else
                        throw new FormatException("Unexpexted token " + currentToken.id + " after '.'.");
                    break;

                case TokenId.OpeningBracket:
                    currentToken = _readToken();
                    if (_readToken().id != TokenId.CloseBracket)
                        throw new FormatException("Expect ']'.");

                    switch(currentToken.id) {
                    case TokenId.Integer:
                        _selectArray();
                        break;

                    case TokenId.QuotedName:
                        _selectObject(currentToken.value);
                        break;

                    default:
                        throw new FormatException("Unexpected token " + currentToken.id);
                    }
                    break;

                default:
                    throw new FormatException("Unexpected token " + currentToken.id);
                }
            }
        }

        private             Token           _readToken()
        {
            if (_pos >= _path.Length)
                return new Token() { id = TokenId.EOP };

            char c = _path[_pos++];

            switch(c) {
            case '$':       return new Token() { id = TokenId.DollarSign      };
            case '.':       return new Token() { id = TokenId.Dot             };
            case '[':       return new Token() { id = TokenId.OpeningBracket  };
            case ']':       return new Token() { id = TokenId.CloseBracket    };
            case '\'':
            case '\"': {
                    var   s = new StringBuilder();
                    char  e = c;

                    for (;;) {
                        if (_pos >= _path.Length)
                            throw new FormatException("End of path while reading quotedstring.");

                        c = _path[_pos++];

                        if (c == e)
                            return new Token() { id = TokenId.QuotedName, value = s.ToString() };

                        if (c == '\\') {
                            if (_pos >= _path.Length)
                                throw new FormatException("End of path while reading quotedstring.");
                            c = _path[_pos++];
                        }

                        s.Append(c);
                    }
                }

            default:
                if (_validNameChar(c)) {
                    var   s = new StringBuilder();

                    s.Append(c);

                    if (_validIntegerChar(c)) {
                        while (_pos < _path.Length && _validIntegerChar(_path[_pos]))
                            s.Append(_path[_pos++]);

                        return new Token() { id = TokenId.Integer, value = s.ToString() };
                    }
                    else {
                        while (_pos < _path.Length && _validNameChar(_path[_pos]))
                            s.Append(_path[_pos++]);

                        return new Token() { id = TokenId.Name, value = s.ToString() };
                    }
                }

                throw new FormatException("Invalid character in path.");
            }
        }
        private             void            _selectObject(string name)
        {
            if (_schema is DataModel.JsonSchemaObject obj) {
                if (obj.Properties.TryGetValue(name, out var item))
                    _schema = item.JsonSchema;
                else
                    throw new FormatException("Unknown property '" + name + "'.");
            }
            else
                throw new FormatException("except object.");
        }
        private             void            _selectArray()
        {
            if (_schema is DataModel.JsonSchemaArray arr)
                _schema = arr.JsonSchema;
            else
                throw new FormatException("except array.");
        }

        private     static  bool            _validNameChar(char c)
        {
            return (c >= 'A' && c <='Z') ||
                   (c >= 'a' && c <='z') ||
                   (c >= '0' && c <='9') ||
                   c == '_';
        }
        private     static  bool            _validIntegerChar(char c)
        {
            return (c >= '0' && c <='9');
        }
    }
}
