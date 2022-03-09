using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#pragma warning disable 0618 // https://github.com/Microsoft/vs-editor-api/wiki/Modern-Quick-Info-API

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.QuickInfo
{
    [Export(typeof(IIntellisenseControllerProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), Name("ToolTip QuickInfo Controller")]
    internal class QuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        public              IQuickInfoBroker                QuickInfoBroker         { get; private set; }

        public              IIntellisenseController         TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            return new QuickInfoController(textView, subjectBuffers, this);
        }
    }
}
