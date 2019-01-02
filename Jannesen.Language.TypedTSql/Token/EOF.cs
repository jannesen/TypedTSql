using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class EOF: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.EOF;
            }
        }

        internal                                            EOF(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }
    }
}
