using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;

#pragma warning disable 0618 // https://github.com/Microsoft/vs-editor-api/wiki/Modern-Quick-Info-API

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.QuickInfo
{
    [Export(typeof(IQuickInfoSourceProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), Name("ToolTip QuickInfo Source"), Order(Before = "Default Quick Info Presenter")]
    internal class QuickInfoSourceProvider: IQuickInfoSourceProvider
    {
        [Import]
        private             SVsServiceProvider                      ServiceProvider = null;

        public              IQuickInfoSource                        TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(ServiceProvider, textBuffer);
        }
    }
}
