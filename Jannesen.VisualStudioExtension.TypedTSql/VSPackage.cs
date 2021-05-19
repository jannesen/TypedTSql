using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using STask=System.Threading.Tasks;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using LTTS           = Jannesen.Language.TypedTSql;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", VSPackage.Version, IconResourceID = 400)]
    [Guid(VSPackage.PackageGuid)]
    [Description("Typed Transact Sql visual studio extensions")]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CatalogExplorer.Panel))]
    [ProvideService(typeof(LanguageService.Service), ServiceName = "TypedTSql Language Services")]
    public sealed class VSPackage: AsyncPackage, IDisposable
    {
        public enum ColorTheme
        {
            Unknown = 0,
            Light,
            Blue,
            Dark,
        }

        public      const       string                                  PackageGuid     = "FCFDB553-8F52-420F-9195-E183E9E501DE";
        public      const       string                                  Version         = "1.09.07.002";        //@VERSIONINFO
        private static readonly Dictionary<Guid, ColorTheme>            _colorThemes    = new Dictionary<Guid, ColorTheme>()
                                                                                            {
                                                                                                { new Guid("de3dbbcd-f642-433c-8353-8f1df4370aba"), ColorTheme.Light },
                                                                                                { new Guid("a4d6a176-b948-4b29-8c66-53c97a1ed7d0"), ColorTheme.Blue },
                                                                                                { new Guid("1ded0138-47ce-435e-84ef-9ec1f439b749"), ColorTheme.Dark },
                                                                                            };

        private                 Commands.CustomMenuCommand              _customMenuCommand;

        public                                                          VSPackage()
        {
        }
                                                                        ~VSPackage()
        {
            Dispose(false);
        }
        public                  void                                    Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override      STask.Task                              InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            this.AddService(typeof(LanguageService.Service), _createLanguageServiceAsync, true);

            _customMenuCommand = new Commands.CustomMenuCommand(this);

            return STask.Task.FromResult<object>(null);
        }
        protected   override    void                                    Dispose(bool disposing)
        {
            if (disposing) {
                ((IServiceContainer)this).RemoveService(typeof(LanguageService.Service));

                if (_customMenuCommand != null) {
                    _customMenuCommand.Dispose();
                    _customMenuCommand = null;
                }
            }
        }

        public      static      EnvDTE.DTE                              GetDTE()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(Package.GetGlobalService(typeof(EnvDTE.DTE)) is EnvDTE.DTE dte)) {
                throw new InvalidOperationException("Failed to get the EnvDTE.DTE.");
            }

            return dte;
        }
        public      static      IVsSolution                             GetIVsSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(Package.GetGlobalService(typeof(SVsSolution)) is IVsSolution solution)) {
                throw new InvalidOperationException("Failed to get the solution.");
            }

            return solution;
        }
        public      static      string                                  GetProjectFileName(IVsProject project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IVsHierarchy)project).GetCanonicalName(VSConstants.VSITEMID_ROOT, out string cname);

            return cname;
        }
        public      static      IVsProject                              GetIVsProject(string projecttypeguid, string projectpathname)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach(IVsProject project in GetLoadedProjects(projecttypeguid)) {
                if (string.Compare(GetProjectFileName(project), projectpathname, StringComparison.OrdinalIgnoreCase) == 0)
                    return project;
            }

            throw new Exception("Can't find project '" + projectpathname + "'.");
        }
        public      static      IVsProject                              GetContainingProject(string projecttypeguid, string fileName)
        {
            if (!String.IsNullOrEmpty(fileName)) {
                ThreadHelper.ThrowIfNotOnUIThread();
                foreach(IVsProject project in GetLoadedProjects(projecttypeguid)) {
                    VSDOCUMENTPRIORITY[]    prio = new VSDOCUMENTPRIORITY[1];

                    if (project.IsDocumentInProject(fileName, out int fFound, prio, out uint itemid) == VSConstants.S_OK && fFound != 0)
                        return project;
                }
            }

            return null;
        }
        public      static      IEnumerable<IVsProject>                 GetLoadedProjects(string projecttypeguid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolution         solution    = GetIVsSolution();
            Guid                guid        = (projecttypeguid != null) ? Guid.Parse(projecttypeguid) : Guid.Empty;

            solution.GetProjectEnum((uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | (projecttypeguid != null ? __VSENUMPROJFLAGS.EPF_MATCHTYPE : 0)), ref guid, out IEnumHierarchies enumerator);

            IVsHierarchy[]      hierarchy   = new IVsHierarchy[1] { null };
            uint                fetched     = 0;

            for (enumerator.Reset(); enumerator.Next(1, hierarchy, out fetched) == VSConstants.S_OK && fetched == 1; /*nothing*/) {
                yield return (IVsProject)hierarchy[0];
            }
        }
        public      static      ColorTheme                              GetCurrentTheme()
        {
            try {
                dynamic     themeService = VSPackage.GetGlobalService(typeof(SVsColorThemeService));
                dynamic     currentTheme = themeService.CurrentTheme;
                Guid        themeGuid    = currentTheme.ThemeId;

                if (_colorThemes.TryGetValue(themeGuid, out ColorTheme colorTheme))
                    return colorTheme;

                throw new Exception("Unknown theme: '" + themeGuid.ToString() + "'.");
            }
            catch(Exception err) {
                System.Diagnostics.Debug.WriteLine("GetCurrentTheme failed:" + err.Message);
                return ColorTheme.Unknown;
            }
        }
        public      static      bool                                    NavigateTo(IServiceProvider serviceProvider, IVsProject project, string fullPath, int line, int column)
        {
            try {
                OpenDocumentView(serviceProvider, project, fullPath).SetCaretPos(line-1, column-1);
                return true;
            }
            catch(Exception err) {
                DisplayError(new Exception("Can't open and navigate.", err));
                return false;
            }
        }
        public      static      bool                                    NavigateTo(IServiceProvider serviceProvider, IVsProject project, string fullPath, int line, int column, int endLine, int endColumn)
        {
            try {
                var textView = OpenDocumentView(serviceProvider, project, fullPath);
                textView.SetCaretPos(line-1, column-1);
                textView.SetSelection(line-1, column-1, endLine-1 , endColumn-1);

                return true;
            }
            catch(Exception err) {
                DisplayError(new Exception("Can't open and navigate.", err));
                return false;
            }
        }
        public      static      bool                                    NavigateTo(IServiceProvider serviceProvider, IVsProject project, LTTS_DataModel.DocumentSpan documentSpan)
        {
            return NavigateTo(serviceProvider, project,
                              documentSpan.Filename,
                              documentSpan.Beginning.Lineno, documentSpan.Beginning.Linepos);
        }

        public      static      bool                                    InsertTextInActiveDocument(string text, bool activeDocument=false)
        {
            try {
                EnvDTE.Document             document = GetDTE().ActiveDocument;

                if (document==null)
                    throw new Exception("No active document");

                EnvDTE.TextDocument     textDocument = document.Object("TextDocument") as EnvDTE.TextDocument;
                if (document==null)
                    throw new Exception("Active document is not a text document.");

                EnvDTE.TextSelection    selection    = textDocument?.Selection as EnvDTE.TextSelection;
                if (document==null)
                    throw new Exception("Can't access textselector.");

                if (text.IndexOf('\n')>=0) {
                    int     line           = selection.ActivePoint.Line;
                    int     lineCharOffset = selection.ActivePoint.LineCharOffset;

                    selection.GotoLine(line, true);
                    StringBuilder prefix = new StringBuilder(selection.Text.Substring(0, lineCharOffset-1));
                    selection.MoveToLineAndOffset(line, lineCharOffset);

                    for (int i = 0 ; i < prefix.Length ; ++i) {
                        if (prefix[i] != '\t')
                            prefix[i] = ' ';
                    }

                    text = text.Replace("\n", "\r\n" + prefix.ToString());
                }

                selection.Insert(text, (int)EnvDTE.vsInsertFlags.vsInsertFlagsInsertAtStart);
                selection.CharRight(false, 1);

                if (activeDocument) {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input,
                                                new Action (() => {
                                                    try {
                                                        document.Activate();
                                                    }
                                                    catch(Exception err) {
                                                        DisplayError(new Exception("Can't active document", err));
                                                    }
                                                }));
                }
                return true;
            }
            catch(Exception err) {
                DisplayError(new Exception("Can't insert text into active document", err));
                return false;
            }
        }
        public      static      void                                    DisplayError(Exception err)
        {
            string msg = err.Message;

            for (var e = err.InnerException ; e != null ; e = e.InnerException)
                msg += "\r\n" + e.Message;

#if DEBUG
            while (err.InnerException != null)
                    err = err.InnerException;

            System.Diagnostics.Debug.WriteLine("ERROR:\r\n" + msg + "\r\nSTACKTRACE\r\n" + err.StackTrace);
#endif

            System.Windows.Forms.MessageBox.Show(msg, "ERROR", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        public                  void                                    ShowCatalogExplorer()
        {
            JoinableTaskFactory.RunAsync(async () => {
                    try {
                        await ShowToolWindowAsync(typeof(CatalogExplorer.Panel), 0, true, DisposalToken);
                    }
                    catch(Exception err) {
                        DisplayError(new Exception("Failt to Show CatalogExplorer.", err));
                    }
                });
        }

        public      static      IVsTextView                             OpenDocumentView(IServiceProvider serviceProvider, IVsProject project, string fullPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsWindowFrame  windowFrame;

            if (project != null && project.IsDocumentInProject(fullPath, out int fFound, new VSDOCUMENTPRIORITY[1], out uint itemid) == VSConstants.S_OK && fFound != 0) {
                var result = project.OpenItem(itemid, Guid.Empty, (IntPtr)(int)-1, out windowFrame);
                if (result != VSConstants.S_OK)
                    throw new Exception("Failed to open file in project: " + result.ToString("X", System.Globalization.CultureInfo.InvariantCulture));
            }
            else
                windowFrame = VsShellUtilities.OpenDocumentWithSpecificEditor(serviceProvider, fullPath, Guid.Empty, Guid.Empty);


            var textView = VsShellUtilities.GetTextView(windowFrame);
            if (textView == null)
                throw new Exception("Failed to get textview for document.");

            windowFrame.Show();

            return textView;
        }

        public      override    IVsAsyncToolWindowFactory               GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse(CatalogExplorer.Panel.GUID)) ? this : null;
        }
        protected   override    STask.Task<object>                      InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            return STask.Task.FromResult<object>(this);
        }

        private                 STask.Task<object>                      _createLanguageServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
        {
            return STask.Task.FromResult<object>(new LanguageService.Service(this));
        }
    }
}
