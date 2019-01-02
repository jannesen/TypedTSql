using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using LTTS = Jannesen.Language.TypedTSql;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    interface IRootItem: IPreviewItem
    {
        bool        databaseRefresh             { get; }
        void        ApplyChanges(IVsOutputWindowPane pane);
    }

    internal class Renamer: INotifyPropertyChanged, IVsPreviewChangesEngine
    {
        private static          Regex                               _regexQName        = new Regex(@"^([a-zA-Z_][a-zA-Z0-9_]*|\[[a-zA-Z0-9_@\-\+.\,\:\;\~\`\!\#\$\%\%\^\&\*\/\\\(\)\{\}\[\]]*\])$", RegexOptions.Singleline|RegexOptions.CultureInvariant);
        private static          Regex                               _regexName         = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$",                    RegexOptions.Singleline|RegexOptions.CultureInvariant);
        private static          Regex                               _regexVariableName = new Regex(@"^@[a-zA-Z_][a-zA-Z0-9_@]*$",                  RegexOptions.Singleline|RegexOptions.CultureInvariant);
        private static          Regex                               _regexTempTable    = new Regex(@"^(\#[a-zA-Z_][a-zA-Z0-9_]*|\[\#[a-zA-Z0-9_@\-\+.\,\:\;\~\`\!\#\$\%\%\^\&\*\/\\\(\)\{\}\[\]]*\])$", RegexOptions.Singleline|RegexOptions.CultureInvariant);

        private                 IServiceProvider                    _serviceProvider;
        private                 LanguageService.Project             _languageServiceProject;
        private                 string                              _srcfilename;
        private                 LTTS.Library.FilePosition           _srcposition;
        private                 LTTS.DataModel.SymbolType           _symbolType;
        private                 string                              _oldName;
        private                 string                              _newName;
        private                 bool                                _previewChanges;
        private                 bool                                _isValid;
        private                 Regex                               _validator;
        private                 IRootItem[]                         _rootItems;
        private                 PreviewList                         _previewList;

        public                  IServiceProvider                    ServiceProvider
        {
            get {
                return _serviceProvider;
            }
        }
        public                  LanguageService.Project             Project
        {
            get {
                return _languageServiceProject;
            }
        }
        public                  string                              OldName
        {
            get {
                return _oldName;
            }
        }
        public                  string                              NewName
        {
            get {
                return _newName;
            }
            set {
                if (_newName != value) {
                    _newName = value;
                    _onPropertyChanged(nameof(NewName));
                    IsValid = !String.IsNullOrWhiteSpace(_newName) &&
                              _oldName != _newName &&
                              _validator.IsMatch(_newName);
                }

            }
        }
        public                  bool                                PreviewChanges
        {
            get {
                return _previewChanges;
            }
            set {
                if (_previewChanges != value) {
                    _previewChanges = value;
                    _onPropertyChanged(nameof(PreviewChanges));
                }
            }
        }
        public                  bool                                IsValid
        {
            get {
                return _isValid;
            }
            set {
                if (_isValid != value) {
                    _isValid = value;
                    _onPropertyChanged(nameof(IsValid));
                }
            }
        }
        // INotifyPropertyChanged
        public  event           PropertyChangedEventHandler         PropertyChanged;

        public                                                      Renamer(IServiceProvider serviceProvider, LanguageService.Project languageServiceProject, LTTS.DataModel.ISymbol symbol)
        {
            _serviceProvider        = serviceProvider;
            _languageServiceProject = languageServiceProject;
            _symbolType             = symbol.Type;
            _oldName                = LTTS.Library.SqlStatic.QuoteName(symbol.Name);
            _newName                = _oldName;
            _previewChanges         = true;
            _isValid                = false;

            _validator = _getValidator();
            _rootItems = _constructFileItems(languageServiceProject.FindReferences(symbol));

            var databaseItem = _constructDatabaseItem(symbol);
            if (databaseItem != null) {
                var rootItems = new IRootItem[_rootItems.Length + 1];
                Array.Copy(_rootItems, 0, rootItems, 1, _rootItems.Length);
                rootItems[0] = databaseItem;
                _rootItems = rootItems;
            }
        }
        public                                                      Renamer(IServiceProvider serviceProvider, LanguageService.Project languageServiceProject, string srcfilename, int srcposition, LTTS.SymbolReferenceList referenceList)
        {
            var srcitem = referenceList.FindByFilenamePosition(srcfilename, srcposition);
            if (srcitem == null)
                throw new InvalidOperationException("Can't find source in referencelist.");

            _serviceProvider        = serviceProvider;
            _languageServiceProject = languageServiceProject;
            _srcfilename            = srcfilename;
            _srcposition            = srcitem.Token.Beginning;
            _symbolType             = referenceList.Symbol.Type;
            _oldName                = srcitem.Token.Text;
            _newName                = _oldName;
            _previewChanges         = true;
            _isValid                = false;

            _validator = _getValidator();
            _rootItems = _constructFileItems(referenceList);

            var databaseItem = _constructDatabaseItem(srcitem.Token.Symbol);
            if (databaseItem != null) {
                var rootItems = new IRootItem[_rootItems.Length + 1];
                Array.Copy(_rootItems, 0, rootItems, 1, _rootItems.Length);
                rootItems[0] = databaseItem;
                _rootItems = rootItems;
            }
        }

        public                  void                                Run()
        {
            if (_showDialog()) {
                if (_previewChanges) {
                    _serviceProvider.GetService<IVsPreviewChangesService>(typeof(SVsPreviewChangesService))
                                    .PreviewChanges(this);
                } else {
                    ApplyChanges();
                }
            }
        }

        // IVsPreviewChangesEngine
        public                  int                                 ApplyChanges()
        {
            IVsLinkedUndoTransactionManager     linkedUndo = null;
            LanguageService.Project             lsp        = null;
            bool                                databaseRefresh = false;

            try {
                var pane = _outputPane();

                (lsp = _languageServiceProject).GlobalChange(true);

                if ((linkedUndo = _serviceProvider.GetService<IVsLinkedUndoTransactionManager>(typeof(SVsLinkedUndoTransactionManager))).OpenLinkedUndo((uint)LinkedTransactionFlags.mdtStrict, "TypedTSql refactor.") != VSConstants.S_OK)
                    throw new InvalidOperationException("linkedUndo.OpenLinkedUndo failed.");

                pane.Clear();
                pane.Activate();
                pane.OutputString(_title() + "\n");

                foreach (var f in _rootItems) {
                    f.ApplyChanges(pane);
                    if (f.databaseRefresh)
                        databaseRefresh = true;
                }

                if (databaseRefresh)
                    Project.Refresh();

                if (linkedUndo.CloseLinkedUndo() != VSConstants.S_OK)
                    throw new InvalidOperationException("linkedUndo.CloseLinkedUndo failed.");
                linkedUndo = null;

                lsp.GlobalChange(false);
                lsp = null;

                if (_srcfilename != null)
                    VSPackage.NavigateTo(Project.VSProject, _srcfilename, _srcposition.Lineno, _srcposition.Linepos);
            }
            catch(Exception err) {
                if (linkedUndo != null) {
                    if (linkedUndo.AbortLinkedUndo() != VSConstants.S_OK)
                        VSPackage.DisplayError(new Exception("linkedUndo.AbortLinkedUndo failed.", err));
                }

                if (lsp != null) {
                    lsp.GlobalChange(false);
                }

                VSPackage.DisplayError(err);
            }

            return VSConstants.S_OK;
        }
        public                  int                                 GetConfirmation(out string pbstrConfirmation)
        {
            pbstrConfirmation = "Apply";
            return VSConstants.S_OK;
        }
        public                  int                                 GetDescription(out string pbstrDescription)
        {
            pbstrDescription = "&Rename " + _oldName + " to " + _newName;
            return VSConstants.S_OK;
        }
        public                  int                                 GetHelpContext(out string pbstrHelpContext)
        {
            pbstrHelpContext = null;
            return VSConstants.S_FALSE;
        }
        public                  int                                 GetRootChangesList(out object ppIUnknownPreviewChangesList)
        {
            if (_previewList == null)
                _previewList = new PreviewList(_rootItems);

            ppIUnknownPreviewChangesList = _previewList;
            return VSConstants.S_OK;
        }
        public                  int                                 GetTextViewDescription(out string pbstrTextViewDescription)
        {
            pbstrTextViewDescription = "Preview Changes:";
            return VSConstants.S_OK;
        }
        public                  int                                 GetTitle(out string pbstrTitle)
        {
            pbstrTitle = _title();
            return VSConstants.S_OK;
        }
        public                  int                                 GetWarning(out string pbstrWarning, out int ppcwlWarningLevel)
        {
            pbstrWarning = null;
            ppcwlWarningLevel = 0;
            return VSConstants.S_OK;
        }

        private                 bool                                _showDialog()
        {
            var result = (new RenameDialog(this)).ShowModal();
            return result.HasValue && result.Value && _isValid;
        }
        private                 Regex                               _getValidator()
        {
            switch(_symbolType) {
            case LTTS.DataModel.SymbolType.TypeUser:
            case LTTS.DataModel.SymbolType.TypeExternal:
            case LTTS.DataModel.SymbolType.TypeTable:
            case LTTS.DataModel.SymbolType.Default:
            case LTTS.DataModel.SymbolType.Rule:
            case LTTS.DataModel.SymbolType.TableInternal:
            case LTTS.DataModel.SymbolType.TableSystem:
            case LTTS.DataModel.SymbolType.TableUser:
            case LTTS.DataModel.SymbolType.View:
            case LTTS.DataModel.SymbolType.Function:
            case LTTS.DataModel.SymbolType.FunctionScalar:
            case LTTS.DataModel.SymbolType.FunctionScalar_clr:
            case LTTS.DataModel.SymbolType.FunctionInlineTable:
            case LTTS.DataModel.SymbolType.FunctionMultistatementTable:
            case LTTS.DataModel.SymbolType.FunctionMultistatementTable_clr:
            case LTTS.DataModel.SymbolType.FunctionAggregateFunction_clr:
            case LTTS.DataModel.SymbolType.StoredProcedure:
            case LTTS.DataModel.SymbolType.StoredProcedure_clr:
            case LTTS.DataModel.SymbolType.StoredProcedure_extended:
            case LTTS.DataModel.SymbolType.Trigger:
            case LTTS.DataModel.SymbolType.Trigger_clr:
            case LTTS.DataModel.SymbolType.Column:
            case LTTS.DataModel.SymbolType.Index:
            case LTTS.DataModel.SymbolType.DatabasePrincipal:
            case LTTS.DataModel.SymbolType.RowsetAlias:
            case LTTS.DataModel.SymbolType.UDTValue:
            case LTTS.DataModel.SymbolType.Service:
            case LTTS.DataModel.SymbolType.ServiceComplexType:
                return _regexQName;

            case LTTS.DataModel.SymbolType.Parameter:
            case LTTS.DataModel.SymbolType.LocalVariable:
                return _regexVariableName;

            case LTTS.DataModel.SymbolType.Label:
            case LTTS.DataModel.SymbolType.Cursor:
                return _regexName;

            case LTTS.DataModel.SymbolType.TempTable:
                return _regexTempTable;

            default:
                throw new Exception("Rename of " + _symbolType.ToString() + " not allowed.");
            }
        }
        private                 FileItem[]                          _constructFileItems(LTTS.SymbolReferenceList referenceList)
        {
            var fileItems = new SortedList<string, FileItem>();

            foreach(var item in referenceList) {
                var filename = item.SourceFile.Filename;

                if (!fileItems.TryGetValue(filename, out var fileItem))
                    fileItems.Add(filename, fileItem = new FileItem(this, filename));

                fileItem.AddChild(new FileLocationItem(fileItem, item.Token, item.Line));
            }

            return fileItems.Values.ToArray();
        }
        private                 DatabaseItem                        _constructDatabaseItem(LTTS.DataModel.ISymbol symbol)
        {
            switch(symbol.Type) {
            case LTTS.DataModel.SymbolType.TableUser:
                {
                    if (symbol is LTTS.DataModel.EntityObjectTable entityTable && entityTable.EntityName.Database == null) {
                        return new DatabaseItem(this, new string[] { entityTable.EntityName.Schema, entityTable.EntityName.Name }, DatabaseItem.RenameType.TABLE);
                    }
                }
                break;

            case LTTS.DataModel.SymbolType.Column:
                {
                    if (symbol is LTTS.DataModel.ColumnDS column &&
                        column.Parent is LTTS.DataModel.EntityObjectTable entityTable &&
                        entityTable.Type == LTTS.DataModel.SymbolType.TableUser &&
                        entityTable.EntityName.Database == null &&
                        entityTable.EntityName.Schema   != "sys")
                    {
                        return new DatabaseItem(this, new string[] { entityTable.EntityName.Schema, entityTable.EntityName.Name, column.Name }, DatabaseItem.RenameType.COLUMN);
                    }
                }
                break;

            case LTTS.DataModel.SymbolType.Index:
                {
                    if (symbol is LTTS.DataModel.Index index &&
                        index.Parent is LTTS.DataModel.EntityObjectTable entityTable &&
                        entityTable.Type == LTTS.DataModel.SymbolType.TableUser &&
                        entityTable.EntityName.Database == null &&
                        entityTable.EntityName.Schema   != "sys")
                    {
                        return new DatabaseItem(this, new string[] { entityTable.EntityName.Schema, entityTable.EntityName.Name, index.Name }, DatabaseItem.RenameType.INDEX);
                    }
                }
                break;

            case LTTS.DataModel.SymbolType.TypeUser:
                {
                    if (symbol is LTTS.DataModel.EntityTypeUser entityTypeUser &&
                        entityTypeUser.EntityName.Database == null &&
                        entityTypeUser.EntityName.Schema   != "sys")
                    {
                        return new DatabaseItem(this, new string[] { entityTypeUser.EntityName.Schema, entityTypeUser.EntityName.Name }, DatabaseItem.RenameType.USERDATATYPE);
                    }
                }
                break;
            }

            return null;
        }
        private                 void                                _onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private                 IVsOutputWindowPane                 _outputPane()
        {
            var outWin = _serviceProvider.GetService<IVsOutputWindow>(typeof(IVsOutputWindow));
            var guid   = new Guid("B7BB9216-8534-4B91-8804-366B0DDCA09C");

            if (outWin.GetPane(ref guid, out var pane) != VSConstants.S_OK) {
                if (outWin.CreatePane(ref guid, "TypedTSql refactor", 1, 1) != VSConstants.S_OK)
                    throw new InvalidOperationException("CreatePane failed.");

                if (outWin.GetPane(ref guid, out pane) != VSConstants.S_OK)
                    return null;
            }

            return pane;
        }
        private                 string                              _title()
        {
            return "Rename " + Helpers.SymbolTypeToString(_symbolType) + " from " + _oldName + " to " + _newName;
        }
    }
}
