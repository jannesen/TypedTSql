using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using LTTS = Jannesen.VisualStudioExtension.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.FindReferences
{
    [Export(typeof(ICommandHandler))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Name(nameof(FindReferencesCommandHandler))]
    internal class FindReferencesCommandHandler: ICommandHandler<FindReferencesCommandArgs>
    {
        [Import]
        private             SVsServiceProvider          ServiceProvider = null;

        public              string                      DisplayName => "Find references";

        public              CommandState                GetCommandState(FindReferencesCommandArgs args)
        {
            return CommandState.Available;
        }
        public              bool                        ExecuteCommand(FindReferencesCommandArgs args, CommandExecutionContext context)
        {
            _ = _executeCommandAsync(args, context);
            return true;
        }

        private    async    Task                        _executeCommandAsync(FindReferencesCommandArgs args, CommandExecutionContext context)
        {
            try {
                var startPosition = args.TextView.Selection.Start.Position;
                var endPosition   = args.TextView.Selection.End.Position;
                var tblsp         = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(ServiceProvider, args.TextView.TextBuffer);
                await tblsp.LanguageService.WhenReadyAndLocked((project) => {
                                                            var filePath = tblsp.FilePath;
                                                            (new LTTS.Rename.Renamer(ServiceProvider,
                                                                                     project,
                                                                                     filePath,
                                                                                     startPosition,
                                                                                     project.FindReferencesAt(filePath, startPosition, endPosition))).Run();
                                                      }, context.OperationContext.UserCancellationToken);
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
    }
}
