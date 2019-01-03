using System;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build
{
    [Serializable]
    public class BuildException: Exception
    {
        public          BuildException(string message): base(message)
        {
        }
        public          BuildException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    [Serializable]
    public class StatusFileException: Exception
    {
        public          StatusFileException(string message): base(message)
        {
        }
        public          StatusFileException(string message, Exception innerException): base(message, innerException)
        {
        }
    }
}
