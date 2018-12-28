using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql
{
    /// <summary>
    /// Exports the Markdown content type and file extension
    /// </summary>
    internal static class FileAndContentTypeDefinitions
    {
        public  const       string                                  TypedTSqlContentTypeName    = "TTSql";
        public  const       string                                  TypedTSqlExtenstion         = ".ttsql";

        [Export, Name(TypedTSqlContentTypeName),        BaseDefinition("code")]
        public static       ContentTypeDefinition                   TypedTSqlContentType        = null;

        [Export, ContentType(TypedTSqlContentTypeName), FileExtension(TypedTSqlExtenstion)]
        public static       FileExtensionToContentTypeDefinition    TypedTSqlFileExtension      = null;
    }
}
