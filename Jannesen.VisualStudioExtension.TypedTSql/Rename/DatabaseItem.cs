using System;
using System.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using LTTS = Jannesen.Language.TypedTSql;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    class DatabaseItem: IRootItem, IPreviewItem
    {
        public enum RenameType
        {
            TABLE,
            COLUMN,
            INDEX,
            USERDATATYPE
        };

        public              Renamer                             Renamer             { get; private set; }
        public              __PREVIEWCHANGESITEMCHECKSTATE      CheckState          { get; set; }
        private             string[]                            _src;
        private             string                              _srcname;
        private             RenameType                          _renameType;
        private             IVsTextLines                        _buffer;

        public                                                  DatabaseItem(Renamer renamer, string[] src, RenameType renameType)
        {
            this.Renamer    = renamer;
            this.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
            _src        = src;
            _srcname    = LTTS.Library.SqlStatic.QuoteNameIfNeeded(src[0]);

            for (int i = 1; i < src.Length ; ++i)
                _srcname += "." + LTTS.Library.SqlStatic.QuoteName(src[i]);

            _renameType = renameType;
        }

        // IRootItem
        public              bool                                databaseRefresh                     { get { return true; } }
        public              void                                ApplyChanges(IVsOutputWindowPane pane)
        {
            try {
                string cmd = _cmd();
                pane.OutputString(cmd + "\n");
                Renamer.Project.ExecDatabase(_setProperty());
                Renamer.Project.ExecDatabase(cmd);
            }
            catch(Exception err) {
                throw new Exception("Rename " + _renameType + " " + _srcname + " failed.", err);
            }
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
            data.SelectedImage = (ushort)StandardGlyphGroup.GlyphLibrary;
        }
        public              string                              GetText(VSTREETEXTOPTIONS options)
        {
            return "RENAME " + _renameType + " " + _srcname + " TO " + LTTS.Library.SqlStatic.QuoteName(_getNewName());
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

            return _VSTREESTATECHANGEREFRESH.TSCR_CURRENT;
        }
        public              void                                DisplayPreview(IVsTextView view)
        {
            if (_buffer == null) {
                var componentModel = (IComponentModel)VSPackage.GetGlobalService(typeof(SComponentModel));
                var adapter        = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                var vsTextBuffer   = adapter.CreateVsTextBufferAdapter((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)VSPackage.GetDTE());

                var cmd = _cmd();
                vsTextBuffer. InitializeContent(cmd, cmd.Length);

                _buffer = vsTextBuffer as IVsTextLines;
            }

            view.SetBuffer(_buffer);
        }

        private             string                              _getNewName()
        {
            var newName = Renamer.NewName;

            if (newName[0] == '[')
                newName = newName.Substring(1, newName.Length-2).Replace("[]", "]");

            return newName;
        }
        private             string                              _cmd()
        {
            switch(_renameType) {
            case RenameType.TABLE:           return "EXEC sp_rename " + LTTS.Library.SqlStatic.QuoteString(_srcname) + ", " + LTTS.Library.SqlStatic.QuoteString(_getNewName()) + ", 'OBJECT'";
            case RenameType.COLUMN:          return "EXEC sp_rename " + LTTS.Library.SqlStatic.QuoteString(_srcname) + ", " + LTTS.Library.SqlStatic.QuoteString(_getNewName()) + ", 'COLUMN'";
            case RenameType.INDEX:           return "EXEC sp_rename " + LTTS.Library.SqlStatic.QuoteString(_srcname) + ", " + LTTS.Library.SqlStatic.QuoteString(_getNewName()) + ", 'INDEX'";
            case RenameType.USERDATATYPE:    return "EXEC sp_rename " + LTTS.Library.SqlStatic.QuoteString(_srcname) + ", " + LTTS.Library.SqlStatic.QuoteString(_getNewName()) + ", 'USERDATATYPE'";
            default:                         return "TODO: " + _renameType;
            }
        }
        private             string                              _setProperty()
        {

            switch(_renameType) {
            case RenameType.TABLE:              return _setProperty("refactor:orgname", _src[0] + "." + LTTS.Library.SqlStatic.QuoteName(_src[1]), "SCHEMA", _src[0], "TABLE", _src[1]);
            case RenameType.COLUMN:             return _setProperty("refactor:orgname", _src[2],                                                   "SCHEMA", _src[0], "TABLE", _src[1], "COLUMN", _src[2]);
            case RenameType.INDEX:              return _setProperty("refactor:orgname", _src[2],                                                   "SCHEMA", _src[0], "TABLE", _src[1], "INDEX", _src[2]);
            case RenameType.USERDATATYPE:       return _setProperty("refactor:orgname", _src[0] + "." + LTTS.Library.SqlStatic.QuoteName(_src[1]), "SCHEMA", _src[0], "TYPE",  _src[1]);
            default:                            return "";
            }
        }
        private             string                              _setProperty(string name, string value, string level0type, string level0name, string level1type, string level1name, string level2type = null, string level2name = null)
        {
            var statement = new StringBuilder(256);

            statement.Append("IF NOT EXISTS (SELECT * FROM sys.fn_listextendedproperty(");
                _appendString(statement, name);       statement.Append(", ");
                _appendString(statement, level0type); statement.Append(", "); _appendString(statement, level0name);   statement.Append(", ");
                _appendString(statement, level1type); statement.Append(", "); _appendString(statement, level1name);   statement.Append(", ");
                _appendString(statement, level2type); statement.Append(", "); _appendString(statement, level2name);   statement.Append("))\n");

            statement.Append("    EXEC sp_addextendedproperty ");
                _appendString(statement, name);       statement.Append(", "); _appendString(statement, value);        statement.Append(", ");
                _appendString(statement, level0type); statement.Append(", "); _appendString(statement, level0name);   statement.Append(", ");
                _appendString(statement, level1type); statement.Append(", "); _appendString(statement, level1name);   statement.Append(", ");
                _appendString(statement, level2type); statement.Append(", "); _appendString(statement, level2name);   statement.Append("\n");

            return statement.ToString();
        }

        private static      void                                _appendString(StringBuilder builder, string value)
        {
            if (value != null) {
                builder.Append('\'');
                builder.Append(value.Replace("\'", "''"));
                builder.Append('\'');
            }
            else
                builder.Append("NULL");
        }
    }
}
