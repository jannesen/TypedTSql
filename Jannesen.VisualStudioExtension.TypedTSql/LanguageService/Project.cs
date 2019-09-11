using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using VSThreadHelper       = Microsoft.VisualStudio.Shell.ThreadHelper;
using VSInterop            = Microsoft.VisualStudio.OLE.Interop;
using VSComponentModelHost = Microsoft.VisualStudio.ComponentModelHost;
using VSShell              = Microsoft.VisualStudio.Shell;
using LTTS                 = Jannesen.Language.TypedTSql;
using LTTS_Core            = Jannesen.Language.TypedTSql.Core;
using LTTS_DataModel       = Jannesen.Language.TypedTSql.DataModel;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal delegate     void          ReadyCallback(Project project);

    internal sealed class Project: IDisposable
    {
        public sealed class SourceFile: IDisposable
        {
            public              Project             Project;
            public  readonly    string              FullPath;
            public              int                 PrevVersionNumber;
            public              ITextBuffer         TextBuffer;
            public              ITextSnapshot       TextSnapshot;
            public              LTTS.SourceFile     TypedTSqlSourceFile;
            public  volatile    FileResult          Result;

            public                                  SourceFile(Project project, string fullPath, LTTS.SourceFile typedTSqlSourceFile)
            {
                this.Project             = project;
                this.FullPath            = fullPath;
                this.PrevVersionNumber   = -1;
                this.TypedTSqlSourceFile = typedTSqlSourceFile;
            }
            public              void                Dispose()
            {
                Project             = null;
                TextBuffer          = null;
                TextSnapshot        = null;
                TypedTSqlSourceFile = null;
                Result              = null;
            }

            public              bool                SetTextBuffer(ITextBuffer textBuffer)
            {
                if (!Object.ReferenceEquals(TextBuffer, textBuffer)) {
                    this.TextBuffer             = textBuffer;
                    this.PrevVersionNumber      = textBuffer.CurrentSnapshot.Version.VersionNumber;
                    this.TextBuffer.PostChanged += _TextBuffer_Changed;

                    return true;
                }

                return false;
            }
            public              void                SetTextBuffer()
            {
                if (this.TextBuffer != null) {
                    this.TextBuffer.Properties.RemoveProperty(typeof(TextBufferLanguageServiceProject));
                    this.TextBuffer.PostChanged -= _TextBuffer_Changed;
                    this.PrevVersionNumber      = -1;
                    this.TextBuffer             = null;
                    this.TextSnapshot           = null;
                    this.Result                 = null;
                }
            }

            public              void                TranspileDone()
            {
                if (TextSnapshot != null) {
                    Result = new FileResult(TextSnapshot, TypedTSqlSourceFile);

                    if (TextBuffer.Properties.TryGetProperty<Editor.Classifier>(typeof(Editor.Classifier), out var classifier))
                        classifier.OnTranspileDone(TextSnapshot);

                    if (TextBuffer.Properties.TryGetProperty<Editor.ErrorTagger>(typeof(Editor.ErrorTagger), out var errorTagger))
                        errorTagger.OnTranspileDone(TextSnapshot);

                    if (TextBuffer.Properties.TryGetProperty<Editor.OutliningTagger>(typeof(Editor.OutliningTagger), out var outliningTagger))
                        outliningTagger.OnTranspileDone(TextSnapshot);
                }
                else
                    Result = null;
            }

            private             void                _TextBuffer_Changed(object sender, EventArgs e)
            {
                Project?.TextBuffer_Changed();
            }
        }

        class HierarchyListener : IVsHierarchyEvents, IDisposable
        {
            private             Project             _project;
            private             IVsHierarchy        _hierarchy;
            private             uint                _cookie;

            public                                  HierarchyListener(Project project, IVsHierarchy hierarchy)
            {
                _project   = project;
                _hierarchy = hierarchy;
                ErrorHandler.ThrowOnFailure(_hierarchy.AdviseHierarchyEvents(this, out _cookie));
            }
            public              void                Dispose()
            {
                var hr = _hierarchy.UnadviseHierarchyEvents(_cookie);
                if (hr != VSConstants.S_OK) {
                    System.Diagnostics.Debug.WriteLine("UnadviseHierarchyEvents FAILED!");
                }
            }

            public              int                 OnItemAdded(uint itemidParent, uint itemidSiblingPrev, uint itemidAdded)
            {
                _project.SyncProject();
                return VSConstants.S_OK;
            }
            public              int                 OnItemsAppended(uint itemidParent)
            {
                _project.SyncProject();
                return VSConstants.S_OK;
            }
            public              int                 OnItemDeleted(uint itemid)
            {
                _project.SyncProject();
                return VSConstants.S_OK;
            }
            public              int                 OnPropertyChanged(uint itemid, int propid, uint flags)
            {
                return VSConstants.S_OK;
            }
            public              int                 OnInvalidateItems(uint itemidParent)
            {
                return VSConstants.S_OK;
            }
            public              int                 OnInvalidateIcon(IntPtr hicon)
            {
                return VSConstants.S_OK;
            }
        }

        [Flags]
        enum WorkFlags
        {
            None                  = 0,
            Active                = 0x0001,
            Stopped               = 0x0002,
            AllWork               = Delay | SyncProject | SyncOpenDocuments | GlobalCatalog | Parse | Transpile | TranspileDone,
            Delay                 = 0x0010,
            SyncProject           = 0x0020,
            SyncOpenDocuments     = 0x0040,
            Parse                 = 0x0080,
            GlobalCatalog         = 0x0100,
            Transpile             = 0x0200,
            TranspileDone         = 0x0400
        }

        public                  string                                          Name                { get; private set; }
        public                  IVsProject                                      VSProject           { get; private set; }
        public                  Service                                         Service             { get; private set; }
        private                 HierarchyListener                               _hierarchyListener;
        private                 WorkFlags                                       _workFlags;
        private                 CancellationTokenSource                         _cancelWait;
        private                 Task                                            _workTask;
        private                 string                                          _databaseName;
        private                 SortedList<string, SourceFile>                  _sourceFiles;
        private                 LTTS.GlobalCatalog                              _globalCatalog;
        private                 LTTS.Transpiler                                 _transpiler;
        private                 ErrorList                                       _errorList;
        private    volatile     int                                             _globalChangeCount;
        private                 object                                          _lockObject;

        public                  LTTS.GlobalCatalog                              GlobalCatalog
        {
            get {
                return _globalCatalog;
            }
        }

        public                                                      Project(Service services, IVsProject vsproject)
        {
            Name                 = VSPackage.GetProjectFileName(vsproject);
            VSProject            = vsproject;
            Service              = services;
            _workFlags           = WorkFlags.SyncProject;
            _cancelWait          = new CancellationTokenSource();
            _sourceFiles         = new SortedList<string, SourceFile>();
            _transpiler          = new LTTS.Transpiler();
            _errorList           = new ErrorList(Service, VSProject);
            _lockObject          = new object();

            _hierarchyListener = new HierarchyListener(this, (IVsHierarchy)vsproject);
            System.Diagnostics.Debug.WriteLine(Name + ": Create LanguageService");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_hierarchyListener")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_errorList")]
        public                  void                                Dispose()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": Dispose");

            var errorList = _errorList;

            lock(_lockObject) {
                _workFlags = WorkFlags.Stopped;
                _cancelWait.Dispose();

                foreach(var s in _sourceFiles) {
                    s.Value.Dispose();
                }

                _sourceFiles.Clear();
            }

            Task.Run(async () => {
                        await VSThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                        if (_hierarchyListener != null) {
                            _hierarchyListener.Dispose();
                            _hierarchyListener = null;
                        }

                        if (_errorList != null) {
                            _errorList.Dispose();
                            _errorList = null;
                        }
                    });
        }
        public                  void                                Start()
        {
            lock(_lockObject) {
                if (_setWork(WorkFlags.Active)) {
                    System.Diagnostics.Debug.WriteLine(Name + ": Start LanguageService");
                    _workTask = Task.Run(_workTaskAsync);
                }
            }
        }
        public                  void                                Stop()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": Stop");

            bool    dispose;

            lock(_lockObject) {
                _workFlags = (_workFlags & ~WorkFlags.AllWork) | WorkFlags.Stopped;
                _cancelWait.Cancel();
                dispose = (_workFlags & WorkFlags.Active) == 0;
            }

            if (dispose)
                Dispose();
        }

        public                  void                                GlobalChange(bool start)
        {
            if (start)
                ++_globalChangeCount;
            else
                --_globalChangeCount;
        }
        public  async           Task                                WhenReady(ReadyCallback callback)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(5000);

            for (;;) {
                Task task;

                lock(_lockObject) {
                    if ((_workFlags & WorkFlags.Active) == 0 || end < DateTime.UtcNow) {
                        if (_cancelWait.IsCancellationRequested) {
                            _cancelWait.Dispose();
                            _cancelWait = new CancellationTokenSource();
                        }

                        if ((_workFlags & WorkFlags.Stopped) != 0)
                            throw new TaskCanceledException("Language service stopped.");

                        if (!(_workFlags == WorkFlags.None && _transpiler != null && _globalCatalog != null))
                            throw new TimeoutException("Language service not available.");

                        callback?.Invoke(this);
                        return;
                    }

                    _cancelWait.Cancel();
                    task = _workTask;
                }


                if (task != null && !task.IsCompleted)
                    await task;
                else
                    await Task.Delay(50);
            }
        }
        public                  void                                Refresh()
        {
            if (_setWork(WorkFlags.SyncProject | WorkFlags.SyncOpenDocuments | WorkFlags.Parse | WorkFlags.GlobalCatalog | WorkFlags.Transpile))
                Start();
        }
        public                  void                                ExecDatabase(string cmd)
        {
            _globalCatalog.Database.ExecuteStatement(cmd);
        }

        public                  bool                                ContainsFile(string fullpath)
        {
            lock(_lockObject) {
                return _sourceFiles.ContainsKey(fullpath.ToUpperInvariant());
            }
        }
        public                  LTTS_DataModel.DocumentSpan         GetDeclarationAt(string fullpath, int startposition, int endposition)
        {
            lock(_lockObject) {
                _available();
                var symbol = _getSymbolAt(fullpath, startposition, endposition);

                if (symbol.Declaration == null)
                    throw new Exception("Symbol has no declaration.");

                return _transpiler.GetDocumentSpan(symbol.Declaration);
            }
        }
        public                  LTTS_DataModel.DocumentSpan         GetDocumentSpan(object declaration)
        {
            lock(_lockObject) {
                _available();
                return _transpiler.GetDocumentSpan(declaration);
            }
        }

        public                  LTTS.TypedTSqlMessage               GetMessageAt(string fullpath, int startPosition, int endPosition)
        {
            VSShell.ThreadHelper.ThrowIfNotOnUIThread();

            if (_errorList == null)
                throw new ObjectDisposedException("LanguageService.Project");

            return _errorList.GetMMessageAt(fullpath, startPosition, endPosition);
        }
        public                  LTTS.SymbolReferenceList            FindReferencesAt(string fullpath, int start, int end)
        {
            lock(_lockObject) {
                _available();
                return _transpiler.GetReferences(_getSymbolAt(fullpath, start, end));
            }
        }
        public                  LTTS.SymbolReferenceList            FindReferences(LTTS.DataModel.ISymbol symbol)
        {
            lock(_lockObject) {
                _available();
                return _transpiler.GetReferences(symbol);
            }
        }
        public                  QuickInfo                           GetQuickInfoAt(string fullpath, SnapshotPoint point)
        {
            lock(_lockObject) {
                _available();
                var sourceFile = _sourceFiles[fullpath.ToUpperInvariant()];

                var token  = _transpiler.GetTokenAt(fullpath, point.TranslateTo(sourceFile.TextSnapshot, PointTrackingMode.Negative).Position);
                var symbol = (token as LTTS_Core.TokenWithSymbol)?.Symbol;
                if (symbol == null || symbol.Type == LTTS_DataModel.SymbolType.NoSymbol)
                    return null;

                return new QuickInfo(sourceFile.TextSnapshot.CreateTrackingSpan(new Span(token.Beginning.Filepos, token.Ending.Filepos - token.Beginning.Filepos), SpanTrackingMode.EdgeExclusive), symbol);
            }
        }

        internal                void                                SyncProject()
        {
            if (_setWork(WorkFlags.SyncProject))
                Start();
        }
        internal                SourceFile                          TextBufferConnected(TextBufferLanguageServiceProject textBufferLanguageService)
        {
            var filePath = textBufferLanguageService.FilePath;
            System.Diagnostics.Debug.WriteLine(Name + ": TextBufferConnected: " + filePath);

            SourceFile sourceFile;

            lock(_lockObject) {
                if (_setWork(WorkFlags.SyncOpenDocuments))
                    Start();

                _sourceFiles.TryGetValue(filePath.ToUpperInvariant(), out sourceFile);
            }

            return sourceFile;
        }
        internal                FileResult                          GetFileResult(string fullname)
        {
            lock(_lockObject) {
                return _sourceFiles.TryGetValue(fullname.ToUpperInvariant(), out var sourceFile) ? sourceFile.Result : null;
            }
        }
        internal                void                                Build_Done()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": Build_Done");

            if (_setWork(WorkFlags.SyncProject | WorkFlags.SyncOpenDocuments | WorkFlags.Parse | WorkFlags.GlobalCatalog | WorkFlags.Transpile))
                Start();
        }
        internal                void                                Document_Closed(string filename)
        {
            if (!string.IsNullOrEmpty(filename)) {
                if (ContainsFile(filename)) {
                    System.Diagnostics.Debug.WriteLine(Name + ": Document_Closed: " + filename);

                    if (_setWork(WorkFlags.SyncOpenDocuments))
                        Start();
                }
            }
        }
        internal                void                                Project_Changed()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": Project_Changed");

            if (_setWork(WorkFlags.SyncProject))
                Start();
        }
        internal                void                                TextBuffer_Changed()
        {
            if (_setWork(WorkFlags.Parse))
                Start();
        }

        private     async       Task                                _workTaskAsync()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": ServiceTask started");
            IVsStatusbar        progessStatusBar = null;
            uint                progessCookie    = 0;

            try {
                WorkFlags   work;

                while ((work = _getWork()) != WorkFlags.None) {
                    if (_globalChangeCount > 0)
                        _setWork(WorkFlags.Delay);

                    switch(work) {
                    case WorkFlags.Delay:
                        System.Diagnostics.Debug.WriteLine(Name + ": Delay");
                        try {
                            await Task.Delay(500, _getCancellationToken());
                        }
                        catch(TaskCanceledException) {
                        }
                        break;

                    case WorkFlags.SyncProject:
                        progessStatusBar = ((IServiceProvider)Service.Package).GetService<IVsStatusbar>(typeof(SVsStatusbar));
                        progessStatusBar.Progress(ref progessCookie, 1, "Sync project: " + Name, 0, 1);
                        await _syncProjectAsync();
                        break;

                    case WorkFlags.SyncOpenDocuments:
                        await _syncOpenDocumentsAsync();
                        break;

                    case WorkFlags.Parse:
                        _parse();
                        break;

                    case WorkFlags.GlobalCatalog:
                        _loadGlobalCatalog();
                        break;

                    case WorkFlags.Transpile:
                        _transpile();
                        break;

                    case WorkFlags.TranspileDone:
                        _transpileDone();
                        break;
                    }
                }

                if (progessStatusBar != null)
                    progessStatusBar.Progress(ref progessCookie, 0, "", 0, 0);
            }
            catch(Exception err) {
                System.Diagnostics.Debug.WriteLine(Name + ": Failed: " + err.Message);

                await VSThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                (((IServiceProvider)Service.Package).GetService(typeof(SVsStatusbar)) as IVsStatusbar)?.SetText("TTSQL Language service failed: " + err.Message);

                lock (_lockObject) {
                    _workTask  = null;
                    _workFlags &= ~WorkFlags.Active;
                }
            }

            System.Diagnostics.Debug.WriteLine(Name + ": ServiceTask stopped");
        }
        private     async       Task                                _syncProjectAsync()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": SyncProject");
            WorkFlags   setWork = WorkFlags.None;

            try {
                string              extensions;
                string              databaseName;
                List<string>        projectFiles = new List<string>();

                // Get fullpath of typed t-sql project items
                {
                    if (!(VSProject is IVsBrowseObjectContext context))
                        throw new Exception("Can't get IVsBrowseObjectContext.");

                    var unconfiguredProject = context.UnconfiguredProject;
                    if (context == null)
                        throw new Exception("Can't get UnconfiguredProject.");

                    using (var access = await unconfiguredProject.ProjectService.Services.ProjectLockService.ReadLockAsync()) {
                        var project = await access.GetProjectAsync(await unconfiguredProject.GetSuggestedConfiguredProjectAsync());

                        extensions      = project.GetPropertyValue("TypedTSqlExtensions");
                        databaseName    = project.GetPropertyValue("SqlDatabaseName");

                        foreach (var item in project.GetItems("SqlFile")) {
                            string fullpath = item.GetMetadataValue("FullPath");
                            if (LTTS.SourceFile.isTypedTSqlFile(fullpath)) {
                                projectFiles.Add(fullpath);
                            }
                        }
                    }
                }


                lock(_lockObject) {
                    if (_databaseName != databaseName) {
                        _databaseName    = databaseName;
                        setWork |= WorkFlags.GlobalCatalog;
                    }

                    try {
                        _transpiler.LoadExtensions(extensions);
                    }
                    catch(Exception) {
                        System.Diagnostics.Debug.WriteLine("Failed to load: " + extensions);
                    }

                    var hashset = new HashSet<string>();

                    foreach(var fullpath in projectFiles) {
                        var fullpath_U = fullpath.ToUpperInvariant();

                        hashset.Add(fullpath_U);

                        if (!_sourceFiles.ContainsKey(fullpath_U)) {
                            System.Diagnostics.Debug.WriteLine(Name + ": Add file " + fullpath_U);
                            var tsf = _transpiler.AddFile(fullpath);
                            _sourceFiles.Add(fullpath_U, new SourceFile(this, fullpath, tsf));
                            setWork |= WorkFlags.SyncOpenDocuments | WorkFlags.Parse | WorkFlags.Transpile;
                        }
                    }

                    foreach(var fullpath_U in new List<string>(_sourceFiles.Keys)) {
                        if (!hashset.Contains(fullpath_U)) {
                            System.Diagnostics.Debug.WriteLine(Name + ": Remove file " + fullpath_U);
                            _transpiler.RemoveFile(fullpath_U);

                            if (_sourceFiles.TryGetValue(fullpath_U, out var sourceFile)) {
                                _sourceFiles.Remove(fullpath_U);
                                sourceFile.Dispose();
                            }
                            setWork |= WorkFlags.Transpile;
                        }
                    }
                }
            }
            catch(Exception err) {
                throw new Exception("SyncProject failed.", err);
            }

            if (setWork != WorkFlags.None)
                _setWork(setWork);
        }
        private     async       Task                                _syncOpenDocumentsAsync()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": SyncOpenDocuments");

            WorkFlags   setWork = WorkFlags.None;

            try {
                await VSThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var files             = new HashSet<string>();
                var gsServiceProvider = (VSInterop.IServiceProvider)VSPackage.GetGlobalService(typeof(VSInterop.IServiceProvider));
                var gsComponentModel  = (VSComponentModelHost.IComponentModel)VSPackage.GetGlobalService(typeof(VSComponentModelHost.SComponentModel));

                using (var vsserviceProvider = new VSShell.ServiceProvider(gsServiceProvider)) {
                    var runningDocumentTable = new VSShell.RunningDocumentTable(vsserviceProvider);
                    var adapter              = gsComponentModel.GetService<Microsoft.VisualStudio.Editor.IVsEditorAdaptersFactoryService>();

                    foreach(var d in runningDocumentTable) {
                        string      fullpath = d.Moniker.ToUpperInvariant();

                        if (_sourceFiles.TryGetValue(fullpath, out SourceFile sourceFile)) {
                            files.Add(fullpath);

                            lock(_lockObject) {
                                if (sourceFile.SetTextBuffer(adapter.GetDataBuffer((Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer)d.DocData)))
                                    setWork |= WorkFlags.Parse;
                            }
                        }
                    }
                }

                foreach(var s in _sourceFiles) {
                    if (!files.Contains(s.Key) && s.Value.TextBuffer != null) {
                        lock(_lockObject) {
                            s.Value.SetTextBuffer();
                            setWork |= WorkFlags.Parse;
                        }
                    }
                }

                if (files.Count == 0) {
                    if (!CatalogExplorer.Panel.IsCatalogExplorerActive(Service.Package)) {
                        bool stop = false;

                        lock(_lockObject) {
                            if ((_workFlags & WorkFlags.SyncOpenDocuments) == 0) {
                                _workFlags = (_workFlags & ~WorkFlags.AllWork) | WorkFlags.Stopped;
                                stop = true;
                            }
                        }

                        if (stop)
                            Service.DeRegisterLanguageService(this);
                    }
                }
            }
            catch(Exception err) {
                throw new Exception("SyncOpenDocuments failed.", err);
            }
            finally {
                await TaskScheduler.Default;
            }

            if (setWork != WorkFlags.None)
                _setWork(setWork);
        }
        private                 void                                _parse()
        {
            var     setWork    = WorkFlags.None;
            var     cancelWait = _getCancellationToken();

            foreach(var sourceFile in _sourceFiles.Values) {
                if (sourceFile.TextBuffer != null) {
                    ITextSnapshot   snapshot = sourceFile.TextBuffer.CurrentSnapshot;

                    if (!Object.ReferenceEquals(sourceFile.TextSnapshot, snapshot)) {
                        if (sourceFile.PrevVersionNumber == snapshot.Version.VersionNumber || cancelWait.IsCancellationRequested) {
                            System.Diagnostics.Debug.WriteLine(Name + ": ParseTextBuffer: " + sourceFile.FullPath + "#" + snapshot.Version.VersionNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));

                            sourceFile.TypedTSqlSourceFile.ParseContent(snapshot.GetText());
                            sourceFile.TextSnapshot = snapshot;
                            setWork |= WorkFlags.Transpile | WorkFlags.TranspileDone;
                        }
                        else {
                            sourceFile.PrevVersionNumber = snapshot.Version.VersionNumber;
                            setWork |= WorkFlags.Delay | WorkFlags.Parse;
                        }
                    }
                }
                else {
                    if (sourceFile.PrevVersionNumber != 0) {
                        sourceFile.PrevVersionNumber = 0;
                        System.Diagnostics.Debug.WriteLine(Name + ": ParseFile: " + sourceFile.FullPath);

                        sourceFile.TypedTSqlSourceFile.ParseFile();

                        setWork |= WorkFlags.Transpile;
                    }
                }
            }

            if (setWork != WorkFlags.None)
                _setWork(setWork);
        }
        private                 void                                _loadGlobalCatalog()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": LoadGlobalCatalog");

            if (_globalCatalog != null) {
                _globalCatalog.Database.Dispose();
                _globalCatalog = null;
            }

            if (String.IsNullOrEmpty(_databaseName))
                throw new Exception("No database configured.");

            _globalCatalog = new LTTS.GlobalCatalog(_databaseName);
            _setWork(WorkFlags.Transpile);
        }
        private                 void                                _transpile()
        {
            System.Diagnostics.Debug.WriteLine(Name + ": Transpile");

            if (_globalCatalog != null) {
                _transpiler.Transpile(_globalCatalog);
                _setWork(WorkFlags.TranspileDone);
            }
        }
        private                 void                                _transpileDone()
        {
            foreach(var sourceFile in _sourceFiles.Values)
                sourceFile.TranspileDone();

            var errors = _transpiler.Errors;

            Task.Run(async () =>
                     {
                            await VSThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            try {
                                CatalogExplorer.Panel.Refresh(Service.Package);
                            }
                            catch(Exception err) {
                                VSPackage.DisplayError(new Exception("Refresh catalog explorer failed.", err));
                            }

                            try {
                                if (_errorList != null)
                                    _errorList.Update(errors);
                            }
                            catch(Exception err) {
                                _errorList = null;
                                VSPackage.DisplayError(new Exception("Typed T-Sql update error task list failed.", err));
                            }
                    });
        }

        private                 bool                                _setWork(WorkFlags flags)
        {
            bool        rtn = false;

            lock(_lockObject) {
                WorkFlags   cur = _workFlags;

                if ((cur & WorkFlags.Stopped) == 0) {
                    _workFlags = cur | flags;
                    rtn = (cur & flags) != flags;
                }
            }

            return rtn;
        }
        private                 WorkFlags                           _getWork()
        {
            lock(_lockObject) {
                WorkFlags   flags = _workFlags;

                if ((flags & WorkFlags.Stopped) == 0) {
                    if ((flags & WorkFlags.Active) != 0) {
                        for (WorkFlags f = WorkFlags.Delay ; f <= flags ; f = (WorkFlags)((int)f << 1)) {
                            if ((flags & f) != 0) {
                                _workFlags &= ~f;
                                return f;
                            }
                        }
                    }
                }
                else
                    Dispose();

                _workTask = null;
                _workFlags &= ~WorkFlags.Active;
                return WorkFlags.None;
            }
        }

        private                 CancellationToken                   _getCancellationToken()
        {
            lock(_lockObject) {
                return _cancelWait.Token;
            }
        }
        private                 void                                _available()
        {
            if (_workFlags == WorkFlags.None && _transpiler != null && _globalCatalog != null)
                return;

            throw new Exception("Language service busy.");
        }
        private                 LTTS_DataModel.ISymbol              _getSymbolAt(string filename, int startposition, int endposition)
        {
            if (!(_transpiler.GetTokenAt(filename, startposition, endposition) is LTTS_Core.TokenWithSymbol token))
                throw new Exception("Invalid token selected.");

            if (!token.hasSymbol)
                throw new Exception("Token has no symbol.");

            return token.Symbol;
        }
    }
}
