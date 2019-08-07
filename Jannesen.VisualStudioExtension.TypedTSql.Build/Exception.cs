using System;
using System.Runtime.Serialization;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build
{
    public class BuildException: Exception
    {
        public          BuildException(string message): base(message)
        {
        }
        public          BuildException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

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
