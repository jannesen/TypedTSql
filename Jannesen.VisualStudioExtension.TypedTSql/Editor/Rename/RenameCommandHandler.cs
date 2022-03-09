using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using LTTS = Jannesen.VisualStudioExtension.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.Rename
{
    [Export(typeof(ICommandHandler))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Name(nameof(RenameCommandHandler))]
    internal class RenameCommandHandler : ICommandHandler<RenameCommandArgs>
    {
        [Import]
        private             SVsServiceProvider  ServiceProvider = null;

        public              string              DisplayName => "Rename";

        public              CommandState        GetCommandState(RenameCommandArgs args)
        {
            return CommandState.Available;
        }
        public              bool                ExecuteCommand(RenameCommandArgs args, CommandExecutionContext context)
        {
            _ = _executeCommandAsync(args, context);
            return true;
        }

        private    async    Task                _executeCommandAsync(RenameCommandArgs args, CommandExecutionContext context)
        {
            try {
                var startPosition = args.TextView.Selection.Start.Position;
                var endPosition   = args.TextView.Selection.End.Position;
                var tblsp         = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(ServiceProvider, args.TextView.TextBuffer);

                await tblsp.LanguageService.WhenReadyAndLocked((project) => {
                          if (startPosition == args.TextView.Selection.Start.Position && endPosition == args.TextView.Selection.End.Position) {
                              var filePath = tblsp.FilePath;
                              (new LTTS.Rename.Renamer(ServiceProvider,
                                                       project,
                                                       filePath,
                                                       startPosition,
                                                       project.FindReferencesAt(filePath, startPosition, endPosition))).Run();
                          }
                      }, context.OperationContext.UserCancellationToken);
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
    }
}
