using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;


namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.GotoDefinition
{
    [Export(typeof(ICommandHandler))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Name(nameof(GotoDefinitionCommandHandler))]
    internal class GotoDefinitionCommandHandler: ICommandHandler<GoToDefinitionCommandArgs >
    {
        [Import]
        private                 SVsServiceProvider  ServiceProvider  = null;

        public                  string              DisplayName => "Goto definition";

        public                  CommandState        GetCommandState(GoToDefinitionCommandArgs args)
        {
            return CommandState.Available;
        }
        public                  bool                ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext context)
        {
            _ = _executeCommandAsync(args, context);
            return true;
        }

        private    async        Task                _executeCommandAsync(GoToDefinitionCommandArgs args, CommandExecutionContext context)
        {
            try {
                var startPosition = args.TextView.Selection.Start.Position;
                var endPosition   = args.TextView.Selection.End.Position;
                var tblsp         = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(ServiceProvider, args.TextView.TextBuffer);

                await tblsp.LanguageService.WhenReadyAndLocked((p) => {
                        if (startPosition == args.TextView.Selection.Start.Position && endPosition == args.TextView.Selection.End.Position) {
                            VSPackage.NavigateTo(ServiceProvider, tblsp.LanguageService.VSProject, p.GetDeclarationAt(tblsp.FilePath, startPosition, endPosition));
                        }
                    }, context.OperationContext.UserCancellationToken);

            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
    }
}
