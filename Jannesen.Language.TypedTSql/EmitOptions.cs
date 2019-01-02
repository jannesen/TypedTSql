using System;

namespace Jannesen.Language.TypedTSql
{
    public class EmitOptions
    {
        public              bool                    DontEmitComment;
        public              bool                    DontEmitCustomComment;
        public              string                  BaseDirectory;
        public              Action<EmitError>       OnEmitError;
        public              Action<string>          OnEmitMessage;
    }
}
