using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class String: Core.TokenWithSymbol
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.String;
            }
        }
        public      override        string                  ValueString
        {
            get {
                return _valueString;
            }
        }

        private                     string                  _valueString;

        internal                                            String(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
            int off = (text[0] == 'N' || text[0] == 'n') ? 1 : 0;
            int len = text.Length - 1;

            if (off < len && text[len] == text[off])
                --len;

            _valueString = text.Substring(1+off, len - off).Replace("''", "'");
        }
    }
}
