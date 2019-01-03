using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    class FileItem: IRootItem, IPreviewItem
    {
        public              Renamer                             Renamer             { get; private set; }
        public              string                              Filename            { get; private set; }
        public              __PREVIEWCHANGESITEMCHECKSTATE      CheckState          { get; set; }

        public              FileLocationItem[]                  LocationItem
        {
            get {
                if (_childrenArray == null) {
                    var array = _children.ToArray();
                    Array.Sort(array, (c1, c2) => c1.Token.Beginning.Filepos - c2.Token.Beginning.Filepos);
                    _children      = null;
                    _childrenArray = array;
                }

                return _childrenArray;
            }
        }

        private             List<FileLocationItem>              _children;
        private             FileLocationItem[]                  _childrenArray;
        private             PreviewList                         _childrenPreviewList;
        private             IVsTextLines                        _buffer;

        public                                                  FileItem(Renamer renamer, string filename)
        {
            this.Renamer    = renamer;
            this.Filename   = filename;
            this.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
            _children = new List<FileLocationItem>();
        }

        public              void                                AddChild(FileLocationItem item)
        {
            if (_children == null)
                throw new InvalidOperationException("AddChild not possible any more.");

            _children.Add(item);
        }
        public              bool                                UpdateCheckState()
        {
            bool fChecked   = false;
            bool fUnchecked = false;

            foreach(var item in LocationItem) {
                switch(item.CheckState) {
                case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked:       fChecked   = true;      break;
                case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked:     fUnchecked = true;      break;
                }
            }

            var checkState = (fChecked && fUnchecked) ? __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked
                                           : fChecked ? __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked
                                                      : __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
            if (CheckState != checkState) {
                CheckState = checkState;
                return true;
            }

            return false;
        }

        // IRootItem
        public              bool                                databaseRefresh                     { get { return false; } }
        public              void                                ApplyChanges(IVsOutputWindowPane pane)
        {
            try {
                var items    = LocationItem;
                var textView = VSPackage.OpenDocumentView(Renamer.Project.VSProject, Filename);

                for (int n = items.Length - 1 ; n >= 0 ; --n)
                    items[n].ApplyChange(pane, textView);
            }
            catch(Exception err) {
                throw new Exception("ApplyChanges to '" + Filename + "' failed.", err);
            }
        }


        // IPreviewItem
        public              bool                                IsExpandable
        {
            get {
                return true;
            }
        }
        public              PreviewList                         Children
        {
            get {
                if (_childrenPreviewList == null)
                    _childrenPreviewList = new PreviewList(LocationItem);

                return _childrenPreviewList;
            }
        }
        public              void                                GetDisplayData(ref VSTREEDISPLAYDATA data)
        {
            data.Mask          = 0;
            data.State         = (uint)CheckState << 12;
            data.Image         =
            data.SelectedImage = (ushort)StandardGlyphGroup.GlyphArrow;
        }
        public              string                              GetText(VSTREETEXTOPTIONS options)
        {
            return Path.GetFileName(Filename);
        }
        public              _VSTREESTATECHANGEREFRESH           ToggleState()
        {
            switch (CheckState) {
            case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked:
                CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
                break;
            case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked:
            case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked:
                CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
                break;
            }

            foreach (var item in LocationItem)
                item.CheckState = CheckState;

            return _VSTREESTATECHANGEREFRESH.TSCR_CURRENT | _VSTREESTATECHANGEREFRESH.TSCR_CHILDREN;
        }
        public              void                                DisplayPreview(IVsTextView view)
        {
            if (_buffer == null) {
                ErrorHandler.ThrowOnFailure(Renamer.ServiceProvider.GetService<IVsInvisibleEditorManager>(typeof(SVsInvisibleEditorManager))
                                                   .RegisterInvisibleEditor(Filename, Renamer.Project.VSProject, (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING, null, out var editor));
                var guid = typeof(IVsTextLines).GUID;
                ErrorHandler.ThrowOnFailure(editor.GetDocData(0, ref guid, out var buffer));
                try {
                    _buffer = Marshal.GetObjectForIUnknown(buffer) as IVsTextLines;
                }
                finally {
                    if (buffer != IntPtr.Zero) {
                        Marshal.Release(buffer);
                    }
                }
            }

            view.SetBuffer(_buffer);
        }
    }
}
