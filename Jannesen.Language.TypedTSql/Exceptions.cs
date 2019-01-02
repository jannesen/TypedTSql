using System;

namespace Jannesen.Language.TypedTSql
{
    public class ParseFileException: Exception
    {
        public                              ParseFileException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class ParseException: Exception
    {
        public      Core.Token              Token       { get ; private set; }

        public                              ParseException(Core.Token token, string message): base(message)
        {
            Token = token;
        }
    }

    public class ErrorException: Exception
    {
        public                              ErrorException(string message): base(message)
        {
        }
        public                              ErrorException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class TranspileException: Exception
    {
        public      Core.IAstNode           Node        { get ; private set; }
        public      QuickFix                QuickFix    { get ; private set; }

        public                              TranspileException(Core.IAstNode node, string message): base(message)
        {
            Node = node;
        }
        public                              TranspileException(Core.IAstNode node, string message, QuickFix quickFix): base(message)
        {
            Node     = node;
            QuickFix = quickFix;
        }
        public                              TranspileException(Core.IAstNode node, string message, Exception innerException): base(message, innerException)
        {
            Node = node;
        }
    }

    public class GlobalCatalogException: Exception
    {
        public                              GlobalCatalogException(string message): base(message)
        {
        }
        public                              GlobalCatalogException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class CatalogCacheException: Exception
    {
        public                              CatalogCacheException(string message): base(message)
        {
        }
        public                              CatalogCacheException(string message, Exception innerException): base(message, innerException)
        {
        }
    }

    public class CatalogInvalidCacheFileException: Exception
    {
        public                              CatalogInvalidCacheFileException(string message): base(message)
        {
        }
    }

    public class NeedsTranspileException: Exception
    {
        public                              NeedsTranspileException(): base("Can't transpile because reference needs transpiled.")
        {
        }
    }

    public class QuickFix
    {
        public      DataModel.DocumentSpan  Location            { get; private set; }
        public      string                  FindString          { get; private set; }
        public      string                  ReplaceString       { get; private set; }

        public                              QuickFix(DataModel.DocumentSpan location, string findString, string replaceString)
        {
            Location      = location;
            FindString    = findString;
            ReplaceString = replaceString;
        }
    }
}
