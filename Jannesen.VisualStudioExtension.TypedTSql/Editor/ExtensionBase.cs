using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal class ExtensionBase
    {
        public  readonly            IServiceProvider                                ServiceProvider;
        public  readonly            ITextBuffer                                     TextBuffer;

        public                                                                      ExtensionBase(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            this.ServiceProvider = serviceProvider;
            this.TextBuffer      = textBuffer;

            this.TextBuffer.ContentTypeChanged += _onContentTypeChanged;
        }

        protected                   SnapshotSpan                                    CreateSpan(ITextSnapshot snapshot, int beginning, int ending)
        {
            try {
                return (Object.ReferenceEquals(TextBuffer.CurrentSnapshot, snapshot))
                                ? new SnapshotSpan(snapshot, beginning, ending-beginning)
                                : snapshot.CreateTrackingSpan(beginning, ending-beginning, SpanTrackingMode.EdgeNegative).GetSpan(TextBuffer.CurrentSnapshot);
            }
            catch(Exception) {
                return new SnapshotSpan(TextBuffer.CurrentSnapshot, 0, 0);
            }
        }
        protected                   LanguageService.FileResult                      GetFileResult()
        {
            var tblsp = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(ServiceProvider, TextBuffer);
            return tblsp?.LanguageService.GetFileResult(tblsp.FilePath);
        }

        private                     void                                            _onContentTypeChanged(object sender, ContentTypeChangedEventArgs e)
        {
            TextBuffer.ContentTypeChanged -= _onContentTypeChanged;
            TextBuffer.Properties.RemoveProperty(this.GetType());
        }
    }
}
