using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using OLE = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor
{
    internal sealed class ContextMenu: OLE.IOleCommandTarget
    {
        private         IServiceProvider        _serviceProvider;
        private         IWpfTextView            _textView;
        private         OLE.IOleCommandTarget   _next;

        public                                  ContextMenu(IServiceProvider serviceProvider, IVsTextView textViewAdapter, IWpfTextView textView)
        {
            _serviceProvider = serviceProvider;
            _textView        = textView;
            textViewAdapter.AddCommandFilter(this, out var next);
            _next = next;
        }

        public          int                     QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLE.OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (Query(ref pguidCmdGroup, prgCmds[0].cmdID)) {
                prgCmds[0].cmdf = (uint)(OLE.OLECMDF.OLECMDF_ENABLED | OLE.OLECMDF.OLECMDF_SUPPORTED);
                return VSConstants.S_OK;
            }

            return _next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
        public          int                     Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Task t;

            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch (nCmdID) {
                case (uint)VSConstants.VSStd97CmdID.GotoDefn:                       t = Exec_GotoDefn();            return VSConstants.S_OK;
                case (uint)VSConstants.VSStd97CmdID.FindReferences:                 t = Exec_FindReferences();      return VSConstants.S_OK;
                }
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch (nCmdID) {
                case (uint)VSConstants.VSStd2KCmdID.RENAME:                         t = Exec_Rename();              return VSConstants.S_OK;
                }
            }
            else if (pguidCmdGroup == VSConstants.VsStd14)
            {
                switch (nCmdID) {
                case (uint)VSConstants.VSStd14CmdID.ShowQuickFixes:
                case (uint)VSConstants.VSStd14CmdID.ShowQuickFixesForPosition:      t = Exec_ShowQuickFixes();      return VSConstants.S_OK;
                }
             }

            return _next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        private         bool                    Query(ref Guid pguidCmdGroup, uint cmdid)
        {
            if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                switch (cmdid) {
                case (uint)VSConstants.VSStd97CmdID.GotoDefn:                       return true;
                case (uint)VSConstants.VSStd97CmdID.FindReferences:                 return true;
                }
            }
            else if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                switch (cmdid) {
                case (uint)VSConstants.VSStd2KCmdID.RENAME:                         return true;
                }
            }
            else if (pguidCmdGroup == VSConstants.VsStd14)
            {
                switch (cmdid) {
                case (uint)VSConstants.VSStd14CmdID.ShowQuickFixes:                 return true;
                case (uint)VSConstants.VSStd14CmdID.ShowQuickFixesForPosition:      return true;
                }
            }

            return false;
        }
        private async   Task                    Exec_GotoDefn()
        {
            try {
                var position = _textView.Caret.Position.BufferPosition.Position;
                var tblsp    = _getTexBufferLanguageServiceProject();

                await tblsp.LanguageService.WhenReady((p) => {
                        VSPackage.NavigateTo(_serviceProvider, tblsp.LanguageService.VSProject, p.GetDeclarationAt(tblsp.FilePath, position));
                    });

            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
        private async   Task                    Exec_FindReferences()
        {
            try {
                var position = _textView.Caret.Position.BufferPosition.Position;
                var tblsp    = _getTexBufferLanguageServiceProject();

                await tblsp.LanguageService.WhenReady((p) => {
                        tblsp.LanguageService.Service.Library.SearchReferences(_serviceProvider, tblsp.LanguageService.VSProject, p.FindReferencesAt(tblsp.FilePath, position));
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
        private async   Task                    Exec_ShowQuickFixes()
        {
            try {
                var position = _textView.Caret.Position.BufferPosition.Position;
                var tblsp    = _getTexBufferLanguageServiceProject();

                await tblsp.LanguageService.WhenReady((p) => {
                        var quickFix = p.GetMessageAt(tblsp.FilePath, position).QuickFix;
                        if (quickFix == null)
                            throw new Exception("No quickfix available.");

                        var textView = VSPackage.OpenDocumentView(_serviceProvider, tblsp.LanguageService.VSProject, quickFix.Location.Filename);
                        textView.SetCaretPos (quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1);
                        textView.SetSelection(quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1, quickFix.Location.Ending.Lineno-1 , quickFix.Location.Ending.Linepos-1);
                        textView.GetSelectedText(out string selectedText);

                        if (selectedText != quickFix.FindString)
                            throw new Exception("Quickfix not possible: '" + selectedText + "' != '" + quickFix.FindString + "'.");

                        if (textView.ReplaceTextOnLine(quickFix.Location.Beginning.Lineno-1, quickFix.Location.Beginning.Linepos-1, quickFix.FindString.Length, quickFix.ReplaceString, quickFix.ReplaceString.Length) != 0)
                            throw new Exception("Replace failed.");
                    });
                await tblsp.LanguageService.WhenReady(null);
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }
        private async   Task                    Exec_Rename()
        {
            try {
                var position   = _textView.Caret.Position.BufferPosition.Position;
                var tblsp      = _getTexBufferLanguageServiceProject();
                await tblsp.LanguageService.WhenReady((project) => {
                                                            var filePath = tblsp.FilePath;
                                                            (new Rename.Renamer(_serviceProvider,
                                                                                project,
                                                                                filePath,
                                                                                position,
                                                                                project.FindReferencesAt(filePath, position))).Run();
                                                      });
            }
            catch(Exception err) {
                VSPackage.DisplayError(err);
            }
        }

        private         LanguageService.TextBufferLanguageServiceProject   _getTexBufferLanguageServiceProject()
        {
            return LanguageService.TextBufferLanguageServiceProject.GetLanguageServiceProject(_serviceProvider, _textView.TextBuffer);
        }
    }
}
