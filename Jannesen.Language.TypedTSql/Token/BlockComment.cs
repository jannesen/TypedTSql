using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class BlockComment: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.BlockComment;
            }
        }
        public      override        bool                    hasNewLine
        {
            get {
                return true;
            }
        }
        public      override        bool                    isWhitespaceOrComment
        {
            get {
                return true;
            }
        }

        internal                                            BlockComment(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
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
                // Dont emit
            }
            else {
                emitWriter.WriteToken(this);
            }
        }
    }
}
