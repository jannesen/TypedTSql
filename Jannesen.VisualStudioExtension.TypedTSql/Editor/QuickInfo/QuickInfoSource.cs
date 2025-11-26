using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Jannesen.VisualStudioExtension.TypedTSql.Classification;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.QuickInfo
{
    internal class QuickInfoSource : IAsyncQuickInfoSource
    {
        private             IServiceProvider                        _serviceProvider;
        private             ITextBuffer                             _textBuffer;

        public                                                      QuickInfoSource(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            _serviceProvider = serviceProvider;
            _textBuffer      = textBuffer;
        }
        public              void                                    Dispose()
        {
            _textBuffer = null;
        }

        public              Task<QuickInfoItem>                     GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            if (_textBuffer == null)
                throw new ObjectDisposedException("QuickInfoSource");

            var snapshot = _textBuffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);
            if (triggerPoint.HasValue) {
                try {
                    var tblsp     = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(_serviceProvider, _textBuffer);
                    var quickInfo = tblsp.LanguageService.GetQuickInfoAt(tblsp.FilePath, triggerPoint.Value);

                    if (quickInfo != null) {
                        return Task.FromResult(
                                    new QuickInfoItem(snapshot.CreateTrackingSpan(new Span(quickInfo.Begin, quickInfo.End-quickInfo.Begin), SpanTrackingMode.EdgeExclusive),
                                                      quickInfo.Info)
                               );
                    }
                }
                catch(Exception err) {
                    try {
                        return Task.FromResult(
                                    new QuickInfoItem(snapshot.CreateTrackingSpan(new Span(triggerPoint.Value.Position, 1), SpanTrackingMode.EdgeExclusive),
                                                      new ClassifiedTextElement(
                                                          new ClassifiedTextRun(ClassificationTypes.Error, "ERROR: " + err.Message)
                                                      ))
                               );
                    }
                    catch(Exception err2) {
                        System.Diagnostics.Debug.WriteLine("AugmentQuickInfoSession: " + err2.Message);
                    }
                }
            }

            return Task.FromResult((QuickInfoItem)null);
        }
    }
}
