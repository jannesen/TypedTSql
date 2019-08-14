using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class LineComment: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.LineComment;
            }
        }
        public      override        bool                    isWhitespaceOrComment
        {
            get {
                return true;
            }
        }


        internal                                            LineComment(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public      override        Core.Token              GetFirstToken(Core.GetTokenMode mode)
        {
            if (mode == Core.GetTokenMode.RemoveWhiteSpaceAndComment)
                return null;

            return this;
        }
        public      override        Core.Token              GetLastToken(Core.GetTokenMode mode)
        {
            if (mode == Core.GetTokenMode.RemoveWhiteSpaceAndComment)
                return null;

            return this;
        }
        public      override        void                    Emit(Core.EmitWriter emitWriter)
        {
            if (emitWriter.EmitOptions.DontEmitComment) {
                if (Beginning.Linepos > 1)
                    emitWriter.WriteText(Text.EndsWith("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n");
            }
            else {
                emitWriter.WriteToken(this);
            }
        }
    }
}
