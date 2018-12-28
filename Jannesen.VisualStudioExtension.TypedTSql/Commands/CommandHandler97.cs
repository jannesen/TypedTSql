using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Commands
{
    [ExportCommandGroup(VSConstants.CMDSETID.StandardCommandSet97_string)]
    [AppliesTo(CPS.TypedTSqlUnconfiguredProject.UniqueCapability)]
    internal class CommandHandler97: ICommandGroupHandler
    {
        public                  CommandStatusResult     GetCommandStatus(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus)
        {
            switch ((VSConstants.VSStd97CmdID)commandId)
            {
            case VSConstants.VSStd97CmdID.Refresh:
                if (_languageServiceProject(nodes) != null)
                    return new CommandStatusResult(true, commandText,  progressiveStatus | CommandStatus.Enabled | CommandStatus.Supported);
                break;
            }

            return CommandStatusResult.Unhandled;
        }
        public                  bool                    TryHandleCommand(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut)
        {
            switch ((VSConstants.VSStd97CmdID)commandId)
            {
            case VSConstants.VSStd97CmdID.Refresh:
                try {
                    var lsp = _languageServiceProject(nodes);
                    if (lsp != null) {
                        lsp.Refresh();
                        return true;
                    }
                }
                catch(Exception err) {
                    VSPackage.DisplayError(new Exception("CommandHandler._handler failed.", err));
                }
                break;
            }

            return false;
        }

        private                 LanguageService.Project      _languageServiceProject(IImmutableSet<IProjectTree> nodes)
        {
            try {
                IProjectTree    root;

                using (IEnumerator<IProjectTree> i = nodes.GetEnumerator())
                {
                    i.MoveNext();
                    root = i.Current.Root;
                }

                return VSPackage.ServiceProvider.GetService<LanguageService.Service>(typeof(LanguageService.Service))
                                                .FindLangaugeServiceByName(((IVsBrowseObjectContext)root).ProjectPropertiesContext.File);
            }
            catch(Exception err) {
                System.Diagnostics.Debug.WriteLine("CommandHandler97._languageServiceProject failed: " + err.Message);
                return null;
            }
        }
    }
}
