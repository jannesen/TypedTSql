using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{

    partial class ContentControl: UserControl, IDisposable
    {
        private                 EnvDTE.SolutionEvents       _solutionEvents;

        public                  ItemList                    Projects                { get; private set; }

        public                  ItemType                    Filter
        {
            get {
                ItemType        rtn = ItemType.None;

                if (filter_datatype.IsChecked.Value)            rtn |= ItemType.DataType;
                if (filter_table.IsChecked.Value)               rtn |= ItemType.Table;
                if (filter_view.IsChecked.Value)                rtn |= ItemType.View;
                if (filter_function_scalar.IsChecked.Value)     rtn |= ItemType.FunctionScalar;
                if (filter_function_tabular.IsChecked.Value)    rtn |= ItemType.FunctionTable;
                if (filter_storedprocedure.IsChecked.Value)     rtn |= ItemType.StoreProcedure;
                if (filter_trigger.IsChecked.Value)             rtn |= ItemType.Trigger;

                return rtn;
            }
        }

        public                                              ContentControl()
        {
            this.InitializeComponent();
            this.Projects = new ItemList();
        }
                                                            ~ContentControl()
        {
            Dispose(false);
        }

        public                  void                        Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected   virtual     void                        Dispose(bool disposing)
        {
            if (disposing) {
                if (_solutionEvents != null) {
                    _solutionEvents.AfterClosing    -= _onSolucationOpenClose;
                    _solutionEvents.Opened          -= _onSolucationOpenClose;
                    _solutionEvents.ProjectAdded    -= _onSolucationProjectAdded;
                    _solutionEvents.ProjectRemoved  -= _onSolucationProjectRemoved;
                    _solutionEvents = null;
                }

                while (this.Projects.Count > 0)
                    _removeProject((ItemProject)this.Projects[0]);
            }
        }

        public                  void                        Refresh()
        {
            foreach(ItemProject p in Projects) {
                var t = p.Refresh();
            }
        }

        public                  Style                       GetStyle(string name)
        {
            return (Style)this.Resources[name];
        }

        private                 void                        _userctl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_solutionEvents == null) {
                _solutionEvents = VSPackage.GetDTE().Events.SolutionEvents;

                _solutionEvents.AfterClosing    += _onSolucationOpenClose;
                _solutionEvents.Opened          += _onSolucationOpenClose;
                _solutionEvents.ProjectAdded    += _onSolucationProjectAdded;
                _solutionEvents.ProjectRemoved  += _onSolucationProjectRemoved;

                _refreshDatabases();
            }
        }
        private                 void                        _userctl_Unloaded(object sender, RoutedEventArgs e)
        {

        }
        private                 void                        _filter_Click(object sender, RoutedEventArgs e)
        {
            foreach(ItemProject itemDatabase in Projects)
                itemDatabase.SelectItems(Filter);
        }
        private                 void                        _onSolucationOpenClose()
        {
            _refreshDatabases();
        }
        private                 void                        _onSolucationProjectAdded(EnvDTE.Project project)
        {
            try {
                if (string.Compare(project.Kind, CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, true) == 0)
                    _refreshDatabases();
            }
            catch(Exception) {
            }
        }
        private                 void                        _onSolucationProjectRemoved(EnvDTE.Project project)
        {
            try {
                if (string.Compare(project.Kind, CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, true) == 0)
                    _refreshDatabases();
            }
            catch(Exception) {
            }
        }

        private                 void                        _refreshDatabases()
        {
            try {
                ThreadHelper.ThrowIfNotOnUIThread();

                HashSet<object> projects = new HashSet<object>();

                foreach(IVsProject project in VSPackage.GetLoadedProjects(CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid)) {
                    if (project.GetMkDocument(VSConstants.VSITEMID_ROOT, out var projectFilename) == 0) {
                        if (!string.IsNullOrWhiteSpace(projectFilename)) {
                            _addProject(project, projectFilename);
                            projects.Add(project);
                        }
                    }
                }

                foreach(ItemProject itemProject in this.Projects.ToArray()) {
                    if (!projects.Contains(itemProject.VSProject))
                        _removeProject(itemProject);
                }
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("Failed to refresh databaselist in database explorer contentcontrol.", err));
            }
        }

        private                 void                        _addProject(IVsProject project, string projectFilename)
        {
            try {
                foreach(ItemProject p in Projects) {
                    if (p.VSProject == project)
                        return;
                }

                ItemProject itemProject = new ItemProject(this, project, projectFilename);

                this.Projects.Add(itemProject);
                this.treeView.Items.Add(itemProject);

                var t = itemProject.Refresh();
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("Failed to add database to database explorer contentcontrol.", err));
            }
        }
        private                 void                        _removeProject(ItemProject itemDatabase)
        {
            try {
                this.treeView.Items.Remove(itemDatabase);
                this.Projects.Remove(itemDatabase);
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("Failed to remove database from database explorer contentcontrol.", err));
            }
        }
    }
}
