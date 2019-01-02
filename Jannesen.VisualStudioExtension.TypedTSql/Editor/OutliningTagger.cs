using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal class OutliningTagger: ExtensionBase, ITagger<IOutliningRegionTag>
    {
        public      event       EventHandler<SnapshotSpanEventArgs>             TagsChanged;

        public                                                                  OutliningTagger(IServiceProvider serviceProvider, ITextBuffer textBuffer): base(serviceProvider, textBuffer)
        {
        }

        public                  void                                            OnTranspileDone(ITextSnapshot snapshot)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }
        public                  IEnumerable<ITagSpan<IOutliningRegionTag>>      GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var currentSnapshot = TextBuffer.CurrentSnapshot;
            var fileResult      = GetFileResult();

            if (fileResult == null || fileResult.OutliningRegions == null)
                yield break;

            foreach (var region in fileResult.OutliningRegions) {
                SnapshotSpan    snapshotSpan = CreateSpan(fileResult.Snapshot, region.Beginning, region.Ending);

                if (spans.IntersectsWith(snapshotSpan))
                    yield return new TagSpan<LanguageService.OutliningRegion>(snapshotSpan, region);
            }

            yield break;
        }
    }

    [Export(typeof(ITaggerProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), TagType(typeof(IOutliningRegionTag))]
    internal class OutliningTaggerProvider : ITaggerProvider
    {
        public                      ITagger<T>                                  CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(typeof(OutliningTagger), () => new OutliningTagger(VSPackage.ServiceProvider, buffer) as ITagger<T>);
        }
    }
}
