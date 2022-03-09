using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.OutliningTagger
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
}
