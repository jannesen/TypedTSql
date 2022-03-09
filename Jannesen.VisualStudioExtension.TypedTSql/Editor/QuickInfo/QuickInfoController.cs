using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

#pragma warning disable 0618 // https://github.com/Microsoft/vs-editor-api/wiki/Modern-Quick-Info-API

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.QuickInfo
{
    internal class QuickInfoController : IIntellisenseController
    {
        private             ITextView                       _textView;
        private             IList<ITextBuffer>              _subjectBuffers;
        private             QuickInfoControllerProvider     _componentContext;
        private             IQuickInfoSession               _session;

        internal                                            QuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, QuickInfoControllerProvider componentContext)
        {
            _textView         = textView;
            _subjectBuffers   = subjectBuffers;
            _componentContext = componentContext;

            _textView.MouseHover += _onTextViewMouseHover;
        }

        public              void                            ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
        public              void                            DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
        public              void                            Detach(ITextView textView)
        {
            if (_textView == textView) {
                _textView.MouseHover -= _onTextViewMouseHover;
                _textView = null;
            }
        }

        private             void                            _onTextViewMouseHover(object sender, MouseHoverEventArgs e)
        {
            SnapshotPoint? point = _textView.BufferGraph.MapDownToFirstMatch(new SnapshotPoint(_textView.TextSnapshot, e.Position),
                                                                             PointTrackingMode.Positive,
                                                                             snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                                                                             PositionAffinity.Predecessor);

            if (point != null) {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position, PointTrackingMode.Positive);

                if (!_componentContext.QuickInfoBroker.IsQuickInfoActive(_textView)) {
                    _session = _componentContext.QuickInfoBroker.CreateQuickInfoSession(_textView, triggerPoint, true);
                    _session.Start();
                }
            }
        }
    }
}
