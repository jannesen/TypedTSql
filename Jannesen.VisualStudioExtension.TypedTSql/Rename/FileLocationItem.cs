using System;
using Jannesen.VisualStudioExtension.TypedTSql.Library;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using LTTS_Core = Jannesen.Language.TypedTSql.Core;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    class FileLocationItem: IPreviewItem
    {
        public              FileItem                            FileItem            { get; private set; }
        public              LTTS_Core.Token                     Token               { get; private set; }
        public              string                              Line                { get; private set; }

        public              __PREVIEWCHANGESITEMCHECKSTATE      CheckState          { get; set; }

        public                                                  FileLocationItem(FileItem fileItem, LTTS_Core.Token token, string line)
        {
            this.FileItem   = fileItem;
            this.Token      = token;
            this.Line       = line;
            this.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
        }

        public              void                                ApplyChange(IVsOutputWindowPane pane, IVsTextView textView)
        {
            textView.SetSelection(Token.Beginning.Lineno-1, Token.Beginning.Linepos-1, Token.Ending.Lineno-1, Token.Ending.Linepos-1);
            textView.GetSelectedText(out string selectedText);

            if (selectedText != Token.Text)
                throw new Exception("Rename not possible: Expect '" + Token.Text + "' got '" + selectedText + "'.");

            var newText = FileItem.Renamer.NewName;

            pane.OutputString(FileItem.Filename + "(" + Token.Beginning.Lineno + "," + Token.Beginning.Linepos + "): replace '" + selectedText + "' with '" + newText + "'.\n");

            if (textView.ReplaceTextOnLine(Token.Beginning.Lineno-1, Token.Beginning.Linepos-1, selectedText.Length, newText, newText.Length) != 0)
                throw new Exception("Replace failed.");
        }

        // IPreviewItem
        public              bool                                IsExpandable
        {
            get {
                return false;
            }
        }
        public              PreviewList                         Children
        {
            get {
                return null;
            }
        }
        public              void                                GetDisplayData(ref VSTREEDISPLAYDATA data)
        {
            data.Mask          = 0;
            data.State         = (uint)CheckState << 12;
            data.Image         =
            data.SelectedImage = (ushort)StandardGlyphGroup.GlyphReference;
        }
        public              string                              GetText(VSTREETEXTOPTIONS options)
        {
            return Line;
        }
        public              _VSTREESTATECHANGEREFRESH           ToggleState()
        {
            CheckState = (CheckState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked) ? __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked : __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;

            return _VSTREESTATECHANGEREFRESH.TSCR_CURRENT |  (FileItem.UpdateCheckState() ? _VSTREESTATECHANGEREFRESH.TSCR_PARENTS : 0);
        }
        public              void                                DisplayPreview(IVsTextView view)
        {
            FileItem.DisplayPreview(view);

            view.EnsureSpanVisible(new TextSpan()
                                    {
                                        iStartLine  = Token.Beginning.Lineno  - 1,
                                        iStartIndex = Token.Beginning.Linepos - 1,
                                        iEndLine    = Token.Ending.Lineno  - 1,
                                        iEndIndex   = Token.Ending.Linepos - 1
                                    });
            view.SetSelection(Token.Beginning.Lineno  - 1,
                              Token.Beginning.Linepos - 1,
                              Token.Ending.Lineno  - 1,
                              Token.Ending.Linepos - 1);
        }
    }
}
