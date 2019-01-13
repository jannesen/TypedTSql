using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using LTTS                 = Jannesen.Language.TypedTSql;
using LTTS_Core            = Jannesen.Language.TypedTSql.Core;
using LTTS_DataModel       = Jannesen.Language.TypedTSql.DataModel;

#pragma warning disable 0618 // https://github.com/Microsoft/vs-editor-api/wiki/Modern-Quick-Info-API

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal class QuickInfoSource: IQuickInfoSource
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

        public              void                                    AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;
            if (_textBuffer == null)
                throw new ObjectDisposedException("QuickInfoSource");

            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            try {
                var tblsp = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(_serviceProvider, _textBuffer);
                var quickInfo = tblsp.LanguageService.GetQuickInfoAt(tblsp.FilePath, triggerPoint.Value);
                if (quickInfo != null) {
                    applicableToSpan = quickInfo.Span;
                    quickInfoContent.Add(quickInfo.Info);
                }
            }
            catch(Exception err) {
                try {
                    applicableToSpan = triggerPoint.Value.Snapshot.CreateTrackingSpan(new Span(triggerPoint.Value.Position, 1), SpanTrackingMode.EdgeExclusive);
                    quickInfoContent.Add("ERROR: " + err.Message);
                }
                catch(Exception err2) {
                    System.Diagnostics.Debug.WriteLine("AugmentQuickInfoSession: " + err2.Message);
                }
            }
        }
    }

    [Export(typeof(IQuickInfoSourceProvider)), ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName), Name("ToolTip QuickInfo Source"), Order(Before = "Default Quick Info Presenter")]
    internal class QuickInfoSourceProvider: IQuickInfoSourceProvider
    {
        public              IQuickInfoSource                        TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(VSPackage.ServiceProvider, textBuffer);
        }
    }
}
