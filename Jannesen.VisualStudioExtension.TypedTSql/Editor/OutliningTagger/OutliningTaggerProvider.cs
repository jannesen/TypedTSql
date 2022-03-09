using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.OutliningTagger
{
    [Export(typeof(ITaggerProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), TagType(typeof(IOutliningRegionTag))]
    internal class OutliningTaggerProvider : ITaggerProvider
    {
        [Import]
        private                     SVsServiceProvider                          ServiceProvider = null;

        public                      ITagger<T>                                  CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(typeof(OutliningTagger), () => new OutliningTagger(ServiceProvider, buffer) as ITagger<T>);
        }
    }
}
