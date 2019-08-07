using System;
using Microsoft.VisualStudio.Text;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal class TextBufferLanguageServiceProject
    {
        private     readonly        IServiceProvider                    _serviceProvider;
        private     readonly        ITextBuffer                         _textBuffer;
        private                     Project                             _languageService;
        private                     Project.SourceFile                  _sourceFile;

        public                      ITextBuffer                         TextBuffer
        {
            get {
                return _textBuffer;
            }
        }
        public                      string                              FilePath
        {
            get {
                return _textBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;
            }
        }
        public                      Project                             LanguageService
        {
            get {
                lock(this) {
                    _updateLink();

                    if (_languageService == null) {
                        throw new InvalidOperationException("File is not part of TypedTSql project.");
                    }

                    return _languageService;
                }
            }
        }

        private                                                         TextBufferLanguageServiceProject(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            _serviceProvider = serviceProvider;
            _textBuffer      = textBuffer;
        }

        public      static          TextBufferLanguageServiceProject    GetLanguageServiceProject(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty<TextBufferLanguageServiceProject>(typeof(TextBufferLanguageServiceProject), () => new TextBufferLanguageServiceProject(serviceProvider, textBuffer));
        }

        internal                    FileResult                          GetFileResult()
        {
            lock(this) {
                _updateLink();
                return _sourceFile != null && _sourceFile.TextBuffer == _textBuffer ? _sourceFile.Result : null;
            }
        }

        private                     void                                _updateLink()
        {
            if (_languageService == null || _sourceFile == null || _sourceFile.Project != _languageService) {
                var filePath = FilePath;
                var project = VSPackage.GetContainingProject(CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, filePath);
                if (project != null) {
                    if (_serviceProvider.GetService(typeof(Service)) is Service service) { 
                        _languageService = service.GetLanguageService(project);
                        _sourceFile      = _languageService.TextBufferConnected(this);
                    }
                    else {
                        System.Diagnostics.Debug.WriteLine("WARNING: LanguageService.Service not registrated.");
                    }
                }
                else {
                    System.Diagnostics.Debug.WriteLine("ERROR: " + filePath + " not part of a project");
                }
            }
        }
    }
}
