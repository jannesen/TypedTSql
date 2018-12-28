using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class Name: Core.TokenWithSymbol
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.Name;
            }
        }
        public      override        string                  ValueString
        {
            get {
                return Text;
            }
        }
        public      override        bool                    isNameOrKeyword
        {
            get {
                return true;
            }
        }
        public      override        bool                    isNameOrQuotedName
        {
            get {
                return true;
            }
        }

        internal                                            Name(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }
    }
}
