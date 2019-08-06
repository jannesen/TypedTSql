using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class TextViewCreationListener : IVsTextViewCreationListener
    {
#pragma warning disable 0649
        [Import]
        private             IVsEditorAdaptersFactoryService         adaptersFactory;

        [Import]
        private             SVsServiceProvider                      ServiceProvider;
#pragma warning restore 0649

        public              void                                    VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = adaptersFactory.GetWpfTextView(textViewAdapter);

            new ContextMenu(ServiceProvider, textView).AddCommandFilter(textViewAdapter);

            textView.Options.SetOptionValue<int>(DefaultOptions.IndentSizeOptionId, 4);
            textView.Options.SetOptionValue<int>(DefaultOptions.TabSizeOptionId, 4);
            textView.Options.SetOptionValue<bool>(DefaultOptions.ConvertTabsToSpacesOptionId, true);
        }
    }
}
