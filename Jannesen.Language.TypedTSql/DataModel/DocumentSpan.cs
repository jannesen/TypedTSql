using System;
using System.Text;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class DocumentSpan
    {
        public                  string                  Filename                { get; protected set; }
        public                  Library.FilePosition    Beginning               { get; protected set; }
        public                  Library.FilePosition    Ending                  { get; protected set; }

        public                                          DocumentSpan(string Filename, Core.IAstNode node)
        {
            this.Filename  = Filename;

            var firstToken = node.GetFirstToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);
            if (firstToken != null)
                this.Beginning = firstToken.Beginning;

            var lastToken = node.GetLastToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);
            if (firstToken != null)
                this.Ending = lastToken.Ending;
        }
        public                                          DocumentSpan(string Filename, Core.Token token)
        {
            this.Filename  = Filename;
            this.Beginning = token.Beginning;
            this.Ending    = token.Ending;
        }

        public  override    string                      ToString()
        {
            var rtn = new StringBuilder();

            rtn.Append(Filename);

            if (Beginning.hasValue) {
                rtn.Append("(");
                rtn.Append(Beginning.Lineno);
                rtn.Append(",");
                rtn.Append(Beginning.Linepos);
                rtn.Append(")");
            }

            return rtn.ToString();
        }
    }
}
