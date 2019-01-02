using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class Keyword: Core.Token
    {
        private                     Core.TokenID            _id;

        public      override        Core.TokenID            ID
        {
            get {
                return _id;
            }
        }
        public      override        bool                    isNameOrKeyword
        {
            get {
                return true;
            }
        }
        public      override        bool                    isKeyword
        {
            get {
                return true;
            }
        }

        internal                                            Keyword(Core.TokenID id, Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
            _id        = id;
        }
    }
}
