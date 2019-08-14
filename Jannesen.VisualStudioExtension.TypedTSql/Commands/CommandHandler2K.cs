using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;

namespace Jannesen.VisualStudioExtension.TypedTSql.Commands
{
    [ExportCommandGroup(VSConstants.CMDSETID.StandardCommandSet2K_string)]
    [AppliesTo(CPS.TypedTSqlUnconfiguredProject.UniqueCapability)]
    internal class CommandHandler2K: ICommandGroupHandler
    {

        public                  CommandStatusResult     GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus)
        {
            if (commandId == (long)VSConstants.VSStd2KCmdID.OUTLN_COLLAPSE_TO_DEF) {
                foreach (var node in nodes) {
                    if (!node.IsFolder && node.Visible && node.FilePath.EndsWith(FileAndContentTypeDefinitions.TypedTSqlExtenstion, StringComparison.OrdinalIgnoreCase)) {
                        progressiveStatus |= CommandStatus.Enabled | CommandStatus.Supported;
                        return new CommandStatusResult(true, commandText, progressiveStatus);
                    }
                }
            }

            return CommandStatusResult.Unhandled;
        }
        public                  bool                    TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut)
        {
            return false;
        }
    }
}
