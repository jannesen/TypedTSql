using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.ErrorTagger
{
    [Export(typeof(IViewTaggerProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), TagType(typeof(ErrorTag))]
    internal class ErrorTaggerProvider: IViewTaggerProvider
    {
        [Import]
        private                     SVsServiceProvider                          ServiceProvider = null;

        public                      ITagger<T>                                  CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(typeof(ErrorTagger), () => new ErrorTagger(ServiceProvider, buffer) as ITagger<T>);
        }
    }
}
