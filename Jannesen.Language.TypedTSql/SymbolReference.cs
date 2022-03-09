using System;
using System.Text;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql
{
    public class SymbolReference
    {
        public struct Usage
        {
            public string                           Containing;
            public DataModel.SymbolUsageFlags       UsageFlags;
        }
        public              SymbolReferenceList         SymbolReferenceList { get; private set; }
        public              SourceFile                  SourceFile          { get; private set; }
        public              Core.TokenWithSymbol        Token               { get; private set; }

        public              DataModel.DocumentSpan      DocumentSpan
        {
            get {
                return new DataModel.DocumentSpan(SourceFile.Filename, Token);
            }
        }

        public                                          SymbolReference(SymbolReferenceList symbolReferenceListSourceFile, SourceFile sourceFile, Core.TokenWithSymbol token)
        {
            this.SymbolReferenceList = symbolReferenceListSourceFile;
            this.SourceFile          = sourceFile;
            this.Token               = token;
        }

        public              List<Core.Token>            GetLineTokens()
        {
            var lineno = Token.Beginning.Lineno;
            var tokens = SourceFile.Tokens;

            int     tokennr = 0;
            int     bsL = 0;
            int     bsR = tokens.Count -1;

            for (;;) {
                var bsM = bsL + (bsR-bsL)/2;

                if (bsL < bsR) {
                    var t   = tokens[bsM];

                    if (t.Beginning.Lineno < lineno) {
                        bsL = bsM + 1;
                        continue;
                    }

                    if (t.Beginning.Lineno > lineno) {
                        bsR = bsM - 1;
                        continue;
                    }
                }

                tokennr =  bsM;
                break;
            }

            while (tokennr > 0 && tokens[tokennr-1].Beginning.Lineno >= lineno)
                --tokennr;

            while (tokennr < tokens.Count && tokens[tokennr].isWhitespaceOrComment)
                ++tokennr;

            var rtn = new List<Core.Token>();

            while (tokennr < tokens.Count && tokens[tokennr].Ending.Lineno == lineno)
                rtn.Add(tokens[tokennr++]);

            return rtn;
        }
        public              Usage                       GetUsage()
        {
            string              containing = null;

            var symbolUsage = Token.SymbolData?.GetSymbolUsage(SymbolReferenceList.Symbol);

            for (Core.IAstNode node = Token ; node != null ; node = node.ParentNode) {
                if (node is Node.DeclarationEntity declarationEntity) {
                    containing = declarationEntity.EntityName.Fullname;
                    break;
                }
            }

            return new Usage() {
                       Containing = containing,
                       UsageFlags = (symbolUsage != null) ? symbolUsage.Usage : DataModel.SymbolUsageFlags.Unknown
                   };
        }
        public  static      string                      GetLine(IEnumerable<Core.Token> tokens)
        {
            var rtn = new StringBuilder();

            foreach(var t in tokens) {
                rtn.Append(t.Text);
            }

            return rtn.ToString();
        }
    }

    public class SymbolReferenceList: List<SymbolReference>
    {
        public              DataModel.ISymbol           Symbol              { get; private set; }

        public                                          SymbolReferenceList(DataModel.ISymbol symbol)
        {
            Symbol = symbol;
        }

        public              SymbolReference             FindByFilenamePosition(string filename, int position)
        {
            foreach(var r in this) {
                if (r.Token.Beginning.Filepos <= position && position < r.Token.Ending.Filepos &&
                    String.Compare(r.SourceFile.Filename, filename, StringComparison.OrdinalIgnoreCase) == 0)
                    return r;
            }

            return null;
        }
    }
}
