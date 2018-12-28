using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class WhiteSpace: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.WhiteSpace;
            }
        }
        public      override        bool                    hasNewLine
        {
            get {
                return Text.IndexOf('\n') >= 0;
            }
        }
        public      override        bool                    isWhitespaceOrComment
        {
            get {
                return true;
            }
        }

        internal                                            WhiteSpace(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public      override        Core.Token              GetFirstToken(Core.GetTokenMode mode)
        {
            if (mode >= Core.GetTokenMode.RemoveWhiteSpace)
                return null;

            return this;
        }
        public      override        Core.Token              GetLastToken(Core.GetTokenMode mode)
        {
            if (mode >= Core.GetTokenMode.RemoveWhiteSpace)
                return null;

            return this;
        }
    }
}
