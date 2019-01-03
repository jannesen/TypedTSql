using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class KeywordWithSymbol: Core.TokenWithSymbol
    {
        private                     Core.TokenID            _id;

        public      override        Core.TokenID            ID
        {
            get {
                return _id;
            }
        }
        public      override        bool                    isKeyword
        {
            get {
                return true;
            }
        }
        public      override        bool                    isNameOrKeyword
        {
            get {
                return true;
            }
        }

        internal                                            KeywordWithSymbol(Core.TokenID id, Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
            _id        = id;
        }
    }
}
