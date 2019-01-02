using System;
using Microsoft.VisualStudio.Text;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal class TextBufferLanguageServiceProject
    {
        public      readonly        string                              FilePath;
        public      readonly        Project                             LanguageService;

        private                                                         TextBufferLanguageServiceProject(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            this.FilePath     = textBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;

            var project = VSPackage.GetContainingProject(CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, FilePath);
            if (project != null) {
                LanguageService = serviceProvider.GetService<Service>(typeof(Service))
                                                 .GetLanguageService(project);
                LanguageService.TextBufferConnected(this);
            }
        }

        public      static          TextBufferLanguageServiceProject    GetLanguageServiceProject(IServiceProvider serviceProvider, ITextBuffer textBuffer)
        {
            var tbls = textBuffer.Properties.GetOrCreateSingletonProperty<TextBufferLanguageServiceProject>(typeof(TextBufferLanguageServiceProject), () => new TextBufferLanguageServiceProject(serviceProvider, textBuffer));
            return tbls.LanguageService != null ? tbls : null;
        }
    }
}
