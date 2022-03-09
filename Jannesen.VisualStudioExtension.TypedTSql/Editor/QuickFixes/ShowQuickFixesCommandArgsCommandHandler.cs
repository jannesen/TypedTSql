using System;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.GotoDefinition.Editor
{
    [Export(typeof(ICommandHandler))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    [Name(nameof(ShowQuickFixesCommandArgsCommandHandler))]
    internal class ShowQuickFixesCommandArgsCommandHandler: ICommandHandler<ShowQuickFixesCommandArgs>
    {
        [Import]
        private             SVsServiceProvider  ServiceProvider  = null;

        public              string              DisplayName => "Show quick fixes";

        public              CommandState        GetCommandState(ShowQuickFixesCommandArgs args)
        {
            return CommandState.Available;
        }
        public              bool                ExecuteCommand(ShowQuickFixesCommandArgs args, CommandExecutionContext context)
        {
            _ = TryQuickFixes(ServiceProvider, args.TextView, context);
            return true;
        }

        public static async Task                TryQuickFixes(IServiceProvider ServiceProvider, ITextView textView, CommandExecutionContext context)
        {
            try {
                var startPosition = textView.Selection.Start.Position;
                var endPosition   = textView.Selection.End.Position;
                var tblsp         = LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(ServiceProvider, textView.TextBuffer);
                bool fixedApplied = false;

                await tblsp.LanguageService.WhenReadyAndLocked((p) => {
                        if (startPosition == textView.Selection.Start.Position && endPosition == textView.Selection.End.Position) {
                            var quickFix = p.GetMessageAt(tblsp.FilePath, startPosition, endPosition).QuickFix;
                            if (quickFix == null)
                                throw new Exception("No quickfix available.");

                            var textViewOpen = VSPackage.OpenDocumentView(ServiceProvider, tblsp.LanguageService.VSProject, quickFix.Location.Filename);
                            textViewOpen.SetCaretPos (quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1);
                            textViewOpen.SetSelection(quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1, quickFix.Location.Ending.Lineno-1 , quickFix.Location.Ending.Linepos-1);
                            textViewOpen.GetSelectedText(out string selectedText);

                            if (selectedText != quickFix.FindString)
                                throw new Exception("Quickfix not possible: '" + selectedText + "' != '" + quickFix.FindString + "'.");

                            if (textViewOpen.ReplaceTextOnLine(quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1, quickFix.FindString.Length, quickFix.ReplaceString, quickFix.ReplaceString.Length) != 0)
                                throw new Exception("Replace failed.");

                            fixedApplied = true;
                        }
                    }, context.OperationContext.UserCancellationToken);

                if (fixedApplied) {
                    await tblsp.LanguageService.WhenReadyAndLocked(null, CancellationToken.None);
                }
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
    }
}
