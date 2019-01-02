using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class QuotedName: Core.TokenWithSymbol
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.QuotedName;
            }
        }
        public      override        string                  ValueString
        {
            get {
                return _valueString;
            }
        }
        public      override        bool                    isNameOrQuotedName
        {
            get {
                return true;
            }
        }

        private                     string                  _valueString;

        internal                                            QuotedName(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
            int len = text.Length - 1;

            if (text[len] == ']')
                --len;

            _valueString = text.Substring(1, len).Replace("[]", "]");
        }
    }
}
