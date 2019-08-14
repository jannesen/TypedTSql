using System;
using System.Text;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql
{
    public class SymbolReference
    {
        public              SourceFile                  SourceFile      { get; private set; }
        public              Core.TokenWithSymbol        Token           { get; private set; }
        public              string                      Line            { get; private set; }

        public              DataModel.DocumentSpan      DocumentSpan
        {
            get {
                return new DataModel.DocumentSpan(SourceFile.Filename, Token);
            }
        }

        public                                          SymbolReference(SourceFile sourceFile, Core.TokenWithSymbol token)
        {
            this.SourceFile = sourceFile;
            this.Token      = token;
            this.Line       = _getLine();
        }

        public  override    string                      ToString()
        {
            var rtn = new StringBuilder();

            rtn.Append(SourceFile.Filename);
            rtn.Append(" - (");
            rtn.Append(Token.Beginning.Lineno);
            rtn.Append(",");
            rtn.Append(Token.Beginning.Linepos);
            rtn.Append("): ");
            rtn.Append(Line);

            return rtn.ToString();
        }

        private             string                      _getLine()
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

            StringBuilder   rtn = new StringBuilder();

            while (tokennr < tokens.Count && tokens[tokennr].Ending.Lineno == lineno)
                rtn.Append(tokens[tokennr++].Text);

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
