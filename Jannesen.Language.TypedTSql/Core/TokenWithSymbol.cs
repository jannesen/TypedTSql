using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;

namespace Jannesen.Language.TypedTSql.Core
{
    public abstract class TokenWithSymbol: Token
    {
        public class NoSymbolClass: DataModel.ISymbol
        {
            public          DataModel.SymbolType        Type                    { get { return DataModel.SymbolType.NoSymbol;   } }
            public          string                      Name                    { get { return "";                              } }
            public          string                      FullName                { get { return Name;                            } }
            public          object                      Declaration             { get { return null;                            } }
            public          DataModel.ISymbol           ParentSymbol            { get { return null;                            } }
            public          DataModel.ISymbol           SymbolNameReference     { get { return null;                            } }
        }

        private  readonly static    DataModel.SymbolUsage   _keywordSymbol = new DataModel.SymbolUsage(new NoSymbolClass(), DataModel.SymbolUsageFlags.Reference);
        private  readonly static    DataModel.SymbolUsage   _noSymbol      = new DataModel.SymbolUsage(new NoSymbolClass(), DataModel.SymbolUsageFlags.Reference);

        public      override        bool                    isKeyword
        {
            get {
                return _symbolData == _keywordSymbol;
            }
        }
        public                      bool                    hasSymbol
        {
            get {
                return _symbolData != null && _symbolData != _noSymbol && _symbolData != _keywordSymbol;
            }
        }

        public                      DataModel.SymbolData    SymbolData          { get {  return _symbolData; } }

        private                     DataModel.SymbolData    _symbolData;      

        internal                                            TokenWithSymbol(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public                      bool                    HasSymbol(DataModel.ISymbol symbol)
        {
            return _symbolData != null ? _symbolData.HasSymbol(symbol) : false;
        }

        public      static          Token                   SetKeyword(Token token)
        {
            if (token is TokenWithSymbol tokenWith) {
                tokenWith._symbolData = _keywordSymbol;
            }

            return token;
        }
        public      static          Token                   SetNoSymbol(Token token)
        {
            if (token is TokenWithSymbol tokenWith) {
                tokenWith._symbolData = _noSymbol;
            }

            return token;
        }
        public      static          TokenWithSymbol         SetNoSymbol(TokenWithSymbol token)
        {
            token._symbolData = _noSymbol;
            return token;
        }
        public                      void                    SetSymbolData(DataModel.SymbolData symbolData)
        {
            _symbolData = symbolData;
        }
        public                      void                    SetSymbolUsage(DataModel.ISymbol symbol, DataModel.SymbolUsageFlags usage)
        {
            _symbolData = new DataModel.SymbolUsage(symbol, usage);
        }
        public                      void                    SetSymbolUsage(DataModel.Column column, DataModel.SymbolUsageFlags usage)
        {
            SetSymbolUsage(column.Symbol, usage);
        }
        public                      void                    SetSymbolUsage(DataModel.Variable variable, DataModel.SymbolUsageFlags usage)
        {
            SetSymbolUsage(variable.Symbol, usage);
        }
        internal                    void                    ClearSymbol()
        {
            if (_symbolData != null) {
                if (object.ReferenceEquals(_symbolData, _keywordSymbol) || object.ReferenceEquals(_symbolData, _noSymbol))
                    return;

                if (_symbolData is DataModel.SymbolUsage symbolUsage) {
                    if (symbolUsage.Symbol is Internal.BuildinFunctionDeclaration ||
                        symbolUsage.Symbol is Core.AstParseNode) {
                        return ;
                    }
                }

                _symbolData = null;
            }
        }
    }
}
