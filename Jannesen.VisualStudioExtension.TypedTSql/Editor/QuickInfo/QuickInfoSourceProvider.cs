using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("TypedTSql toolTip QuickInfo Source")]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Order(Before = "default")]
    internal class MyQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        private             SVsServiceProvider                      ServiceProvider = null;

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(ServiceProvider, textBuffer);
        }
    }
}
