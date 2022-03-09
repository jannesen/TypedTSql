using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;


namespace Jannesen.VisualStudioExtension.TypedTSql.GotoDefinition.Editor
{
    [Export(typeof(ICommandHandler))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Name(nameof(ShowQuickFixesForPositionCommandArgsCommandHandler))]
    internal class ShowQuickFixesForPositionCommandArgsCommandHandler: ICommandHandler<ShowQuickFixesForPositionCommandArgs>
    {
        [Import]
        private             SVsServiceProvider  ServiceProvider  = null;

        public              string              DisplayName => "Show quick fixes for position";

        public              CommandState        GetCommandState(ShowQuickFixesForPositionCommandArgs args)
        {
            return CommandState.Available;
        }
        public              bool                ExecuteCommand(ShowQuickFixesForPositionCommandArgs args, CommandExecutionContext context)
        {
            _ = ShowQuickFixesCommandArgsCommandHandler.TryQuickFixes(ServiceProvider, args.TextView, context);
            return true;
        }
    }
}
