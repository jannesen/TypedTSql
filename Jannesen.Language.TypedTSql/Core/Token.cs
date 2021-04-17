using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Jannesen.Language.TypedTSql.Core
{
    public abstract class Token: IAstNode
    {
        public      abstract        TokenID                 ID              { get; }
        public                      Library.FilePosition    Beginning       { get; private set; }
        public                      Library.FilePosition    Ending          { get; private set; }
        public                      string                  Text            { get; private set; }

        public                      AstNodeList             Children
        {
            get {
                return null;
            }
        }
        public                      IAstNode                Parent
        {
            get {
                return _parent;
            }
        }

        public      virtual         string                  ValueString
        {
            get {
                throw new InvalidOperationException("Can't get ValueString on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         Int32                   ValueInt
        {
            get {
                throw new InvalidOperationException("Can't get ValueInt on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         Int64                   ValueBigInt
        {
            get {
                throw new InvalidOperationException("Can't get ValueBigInt on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         decimal                 ValueDecimal
        {
            get {
                throw new InvalidOperationException("Can't get ValueInteger on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         double                  ValueFloat
        {
            get {
                throw new InvalidOperationException("Can't get ValueFloat on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         byte[]                  ValueBinary
        {
            get {
                throw new InvalidOperationException("Can't get ValueBinary on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         XmlElement              ValueXmlFragment
        {
            get {
                throw new InvalidOperationException("Can't get ValueXmlFragment  on a " + ID.ToString() + " token.");
            }
        }
        public      virtual         bool                    hasNewLine
        {
            get {
                return false;
            }
        }
        public      virtual         bool                    isWhitespaceOrComment
        {
            get {
                return false;
            }
        }
        public      virtual         bool                    isKeyword
        {
            get {
                return false;
            }
        }
        public      virtual         bool                    isNameOrKeyword
        {
            get {
                return false;
            }
        }
        public      virtual         bool                    isNameOrQuotedName
        {
            get {
                return false;
            }
        }
        public      virtual         bool                    isInteger()
        {
            if (ID != Core.TokenID.Number)
                return false;

            for (int i = 0 ; i < Text.Length ; ++i) {
                if (Text[i] < '0' || Text[i] > '9')
                    return false;
            }

            return true;
        }

        private                     IAstNode                _parent;

        protected                                           Token(Library.FilePosition beginning, Library.FilePosition ending, string text)
        {
            Beginning = beginning;
            Ending    = ending;
            Text      = text;
        }
        internal    static          Token                   Create(TokenID id, Library.FilePosition beginning, Library.FilePosition ending, string text)
        {
            switch(id) {
            case TokenID.InvalidCharacter:  return new TypedTSql.Token.InvalidCharacter(beginning, ending, text);
            case TokenID.WhiteSpace:        return new TypedTSql.Token.WhiteSpace(beginning, ending, text);
            case TokenID.LineComment:       return new TypedTSql.Token.LineComment(beginning, ending, text);
            case TokenID.BlockComment:      return new TypedTSql.Token.BlockComment(beginning, ending, text);
            case TokenID.Name:              return new TypedTSql.Token.Name(beginning, ending, text);
            case TokenID.QuotedName:        return new TypedTSql.Token.QuotedName(beginning, ending, text);
            case TokenID.LocalName:         return new TypedTSql.Token.TokenLocalName(beginning, ending, text);
            case TokenID.String:            return new TypedTSql.Token.String(beginning, ending, text);
            case TokenID.Number:            return new TypedTSql.Token.Number(beginning, ending, text);
            case TokenID.BinaryValue:       return new TypedTSql.Token.Binary(beginning, ending, text);
            case TokenID.DataIsland:        return new TypedTSql.Token.DataIsland(beginning, ending, text);
            case TokenID.EOF:               return new TypedTSql.Token.EOF(beginning, ending, text);
            default:
                if (TokenID._operators < id  && id < TokenID._beginkeywords)
                    return new TypedTSql.Token.Operator(id, beginning, ending, text);

                if (TokenID._beginkeywords < id  && id < TokenID._endkeywords)
                    return new TypedTSql.Token.Keyword(id, beginning, ending, text);

                if (TokenID._beginkeywordswithsymbol < id  && id < TokenID._endkeywordswithsymbol)
                    return new TypedTSql.Token.KeywordWithSymbol(id, beginning, ending, text);

                throw new NotImplementedException("TypedTSqlToken " + id.ToString());
            }
        }

        public                      bool                    isToken(TokenID id)
        {
            return ID == id;
        }
        public                      bool                    isToken(params TokenID[] ids)
        {
            for(int i = 0 ; i < ids.Length ; ++i) {
                if (ids[i] == ID)
                    return true;
            }

            return false;
        }
        public                      int                     isToken(params TokenNameID[] nameIDs)
        {
            string text = Text.ToUpperInvariant();

            for(int i = 0 ; i < nameIDs.Length ; ++i) {
                if (nameIDs[i].Name == text)
                    return i;
            }

            return -1;
        }

        public                      bool                    isToken(string name)
        {
            return isNameOrKeyword && Text.ToUpperInvariant() == name;
        }
        public                      bool                    isToken(params string[] names)
        {
            if (isNameOrKeyword) {
                string text = Text.ToUpperInvariant();

                for(int i = 0 ; i < names.Length ; ++i) {
                    if (names[i] == text)
                        return true;
                }
            }

            return false;
        }
        public                      bool                    isToken(object id_name)
        {
            return (id_name is string) ? (isNameOrKeyword && (string)id_name == Text.ToUpperInvariant())
                                       : ((TokenID)id_name) == ID;
        }
        public                      void                    validateToken(TokenID id)
        {
            if (ID == id)
                return;

            throw new ParseException(this, "Expect " + id.ToString() + " got " + ID.ToString() + ".");
        }
        public                      TokenID                 validateToken(params TokenID[] ids)
        {
            for(int i = 0 ; i < ids.Length ; ++i) {
                if (ids[i] == ID)
                    return ID;
            }

            throw new ParseException(this, "Except " + _setToString(ids) + " got " + ID.ToString() + ".");
        }
        public                      int                     validateToken(params TokenNameID[] nameIDs)
        {
            var v = isToken(nameIDs);

            if (v < 0)
                throw new ParseException(this, "Except " + _setToString(nameIDs) + " got " + ID.ToString() + ".");

            return v;
        }
        public                      void                    validateToken(string name)
        {
            if (!isNameOrKeyword)
                throw new ParseException(this, "Expect Name got " + ID.ToString() + ".");

            string text = Text.ToUpperInvariant();

            if (name == text)
                return ;

            throw new ParseException(this, "Expect " + name + " got '" + text + "'.");
        }
        public                      string                  validateToken(params string[] names)
        {
            if (!isNameOrKeyword)
                throw new ParseException(this, "Expect Name got " + ID.ToString() + ".");

            string text = Text.ToUpperInvariant();

            for(int i = 0 ; i < names.Length ; ++i) {
                if (names[i] == text)
                    return text;
            }

            throw new ParseException(this, "Expect " + _setToString(names) + " got '" + text + "'.");
        }
        public                      TokenID                 validateToken(params object[] namesandid)
        {
            string text = Text.ToUpperInvariant();

            for(int i = 0 ; i < namesandid.Length ; ++i) {
                object t = namesandid[i];

                if ((t is TokenID && ((TokenID)t == ID  )) ||
                    (t is string  && ((string     )t == text)))
                    return ID;
            }

            throw new ParseException(this, "Expect " + _setToString(namesandid) + " got '" + text + "'.");
        }
        public                      void                    validateInteger()
        {
            if (!isInteger())
                throw new ParseException(this, "Expect Integer.");
        }

        public      virtual         Token                   GetFirstToken(GetTokenMode mode)
        {
            return this;
        }
        public      virtual         Token                   GetLastToken(GetTokenMode mode)
        {
            return this;
        }
        public      virtual         void                    Emit(EmitWriter emitWriter)
        {
            emitWriter.WriteToken(this);
        }
        public                      void                    SetParent(IAstNode parent)
        {
            _parent = parent;
        }

        public      static          bool                    operator == (Token t1, Token t2)
        {
            if (object.ReferenceEquals(t1, t2)) return true;
            if (t1 is null || t2 is null) return false;

            return (t1.ID == t2.ID && t1.Beginning == t2.Beginning && t1.Ending == t2.Ending && t1.Text == t2.Text);
        }
        public      static          bool                    operator != (Token t1, Token t2)
        {
            return !(t1 == t2);
        }
        public      override        bool                    Equals(object o2)
        {
            if (o2 is Token)
                return this == (Token)o2;

            return false;
        }
        public      override        int                     GetHashCode()
        {
            return ((int)ID) ^ Beginning.GetHashCode() ^ Ending.GetHashCode() ^ Text.GetHashCode();
        }
        public      override        string                  ToString()
        {
            return ID.ToString();
        }

        internal    static          string                  _setToString(TokenID[] ids)
        {
            StringBuilder       rtn = new StringBuilder();

            for(int i = 0 ; i < ids.Length ; ++i) {
                if (i > 0)
                    rtn.Append(',');

                rtn.Append(ids[i].ToString());
            }

            return rtn.ToString();
        }
        internal    static          string                  _setToString(TokenNameID[] nameIDs)
        {
            StringBuilder       rtn = new StringBuilder();

            for(int i = 0 ; i < nameIDs.Length ; ++i) {
                if (i > 0)
                    rtn.Append(',');

                rtn.Append(nameIDs[i].Name);
            }

            return rtn.ToString();
        }
        internal    static          string                  _setToString(string[] names)
        {
            StringBuilder       rtn = new StringBuilder();

            for(int i = 0 ; i < names.Length ; ++i) {
                if (i > 0)
                    rtn.Append(',');

                rtn.Append(names[i]);
            }

            return rtn.ToString();
        }
        internal    static          string                  _setToString(object[] namesandids)
        {
            StringBuilder       rtn = new StringBuilder();

            for(int i = 0 ; i < namesandids.Length ; ++i) {
                object t = namesandids[i];

                if (i > 0)
                    rtn.Append(',');

                if (t is string)        rtn.Append((string)t);
                if (t is TokenID)   rtn.Append(((TokenID)t).ToString());
            }

            return rtn.ToString();
        }
    }

    public abstract class TokenWithSymbol: Token
    {
        public class NoSymbolClass: DataModel.ISymbol
        {
            public          DataModel.SymbolType        Type                    { get { return DataModel.SymbolType.NoSymbol;   } }
            public          string                      Name                    { get { return "";                              } }
            public          object                      Declaration             { get { return null;                            } }
            public          DataModel.ISymbol           Parent                  { get { return null;                            } }
            public          DataModel.ISymbol           SymbolNameReference     { get { return null;                            } }
        }

        private  readonly static    NoSymbolClass           _keywordSymbol = new NoSymbolClass();
        private  readonly static    NoSymbolClass           _noSymbol      = new NoSymbolClass();

        public      override        bool                    isKeyword
        {
            get {
                return Symbol == _keywordSymbol;
            }
        }
        public                      bool                    hasSymbol
        {
            get {
                return Symbol != null && Symbol != _noSymbol && Symbol != _keywordSymbol;
            }
        }

        public                      DataModel.ISymbol       Symbol      { get; private set; }

        internal                                            TokenWithSymbol(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public      static          Token                   SetKeyword(Token token)
        {
            (token as TokenWithSymbol)?.SetSymbol(_keywordSymbol);
            return token;
        }
        public      static          Token                   SetNoSymbol(Token token)
        {
            (token as TokenWithSymbol)?.SetSymbol(_noSymbol);
            return token;
        }
        public      static          TokenWithSymbol         SetNoSymbol(TokenWithSymbol token)
        {
            token.SetSymbol(_noSymbol);
            return token;
        }
        public                      void                    SetSymbol(DataModel.ISymbol symbol)
        {
            Symbol = symbol;
        }
        public                      void                    ClearSymbol()
        {
            if (this.Symbol != null && !(this.Symbol == _keywordSymbol || this.Symbol == _noSymbol ||this.Symbol is Internal.BuildinFunctionDeclaration  || this.Symbol is Core.AstParseNode))
                this.Symbol = null;
        }
    }
}
