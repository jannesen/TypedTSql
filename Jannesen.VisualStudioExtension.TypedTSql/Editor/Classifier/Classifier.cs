using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using LTTS_Core = Jannesen.Language.TypedTSql.Core;
using Jannesen.VisualStudioExtension.TypedTSql.Classification;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.Classifier
{
    internal class Classifier: ExtensionBase, IClassifier
    {
        public      event       EventHandler<ClassificationChangedEventArgs>    ClassificationChanged;

        private     readonly    ClassificationFactory                           _classificationFactory;

        public                                                                  Classifier(IServiceProvider serviceProvider, ITextBuffer textBuffer, IClassificationTypeRegistryService registry) : base(serviceProvider, textBuffer)
        {
            _classificationFactory = new ClassificationFactory(registry);
        }

        public                  void                                            OnTranspileDone(ITextSnapshot snapshot)
        {
            ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }
        public                  IList<ClassificationSpan>                       GetClassificationSpans(SnapshotSpan span)
        {
            var currentSnapshot = TextBuffer.CurrentSnapshot;
            var fileResult      = GetFileResult();
            var rtn             = new List<ClassificationSpan>();

            if (fileResult != null && fileResult.Tokens != null) {
                try {
                    var         selectSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive).GetSpan(fileResult.Snapshot);
                    int         firstSpanToken = 0;
                    int         bsL = 0;
                    int         bsR = fileResult.Tokens.Count -1;

                    while (bsL < bsR - 2) {
                        var bsM = bsL + (bsR-bsL)/2;
                        var t   = fileResult.Tokens[bsM];

                        if (t.Ending.Filepos < selectSpan.Start) {
                            bsL = bsM + 1;
                            continue;
                        }

                        if (t.Beginning.Filepos > selectSpan.Start) {
                            bsR = bsM - 1;
                            continue;
                        }

                        firstSpanToken = bsM-1;
                        if (firstSpanToken < 0)
                            firstSpanToken = 0;
                        break;
                    }

                    for(int i = firstSpanToken ; i < fileResult.Tokens.Count ; ++i) {
                        LTTS_Core.Token token        = fileResult.Tokens[i];
                        SnapshotSpan    snapshotSpan = CreateSpan(fileResult.Snapshot, token.Beginning.Filepos, token.Ending.Filepos);

                        if (span.IntersectsWith(snapshotSpan)) {
                            IClassificationType     ct = _classificationFactory.TokenClassificationType(token);

                            if (ct != null)
                                rtn.Add(new ClassificationSpan(new SnapshotSpan(fileResult.Snapshot, token.Beginning.Filepos, token.Ending.Filepos-token.Beginning.Filepos), ct));
                        }
                        else {
                            if (token.Beginning.Filepos > selectSpan.End)
                                break;
                        }
                    }
                }
                catch(Exception) {
                    // snapshot.CreateTrackingSpan some times failed with a ArgumentOutOfRangeException exception. Just ignore is the best solution.
                }
            }

            return rtn;
        }
    }
}
