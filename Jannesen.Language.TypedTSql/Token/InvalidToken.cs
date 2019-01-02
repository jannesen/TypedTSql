using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class InvalidCharacter: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.InvalidCharacter;
            }
        }

        internal                                            InvalidCharacter(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public      override        string                  ToString()
        {
            return "Invalid character '" + Text+ "'";
        }
    }
}
