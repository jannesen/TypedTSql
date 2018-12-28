using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Adornments;
using System.ComponentModel.Composition;
using LTTS = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal class ErrorTagger: ExtensionBase, ITagger<ErrorTag>
    {
        public      event   EventHandler<SnapshotSpanEventArgs>             TagsChanged;

        public                                                              ErrorTagger(IServiceProvider serviceProvider, ITextBuffer textBuffer): base(serviceProvider, textBuffer)
        {
        }

        public              void                                            OnTranspileDone(ITextSnapshot snapshot)
        {
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, new Span(0, snapshot.Length))));
        }
        public              IEnumerable<ITagSpan<ErrorTag>>                 GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var fileResult = GetFileResult();

            if (fileResult == null || fileResult.Messages == null)
                yield break;

            foreach (var error in fileResult.Messages) {
                SnapshotSpan    snapshotSpan = CreateSpan(fileResult.Snapshot, error.Beginning.Filepos, error.Ending.Filepos);

                if (spans.IntersectsWith(snapshotSpan)) {
                    yield return new TagSpan<ErrorTag>(snapshotSpan, new ErrorTag(_mapClassification(error.Classification), error.Message));
                }
            }

            yield break;
        }

        private     static  string                                          _mapClassification(LTTS.TypedTSqlMessageClassification classification)
        {
            switch(classification)
            {
            case LTTS.TypedTSqlMessageClassification.ParseError:        return PredefinedErrorTypeNames.SyntaxError;
            case LTTS.TypedTSqlMessageClassification.TranspileError:    return PredefinedErrorTypeNames.CompilerError;
            case LTTS.TypedTSqlMessageClassification.TranspileWarning:  return PredefinedErrorTypeNames.Warning;
            default:                                                    return PredefinedErrorTypeNames.OtherError;
            }
        }
    }

    [Export(typeof(IViewTaggerProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), TagType(typeof(ErrorTag))]
    internal class ErrorTaggerProvider: IViewTaggerProvider
    {
        public                      ITagger<T>                                  CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(typeof(ErrorTagger), () => new ErrorTagger(VSPackage.ServiceProvider, buffer) as ITagger<T>);
        }
    }
}
