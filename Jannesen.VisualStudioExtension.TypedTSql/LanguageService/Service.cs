using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    [Guid(Service.GUID)]
    internal sealed class Service: IDisposable
    {
        public  const           string                              GUID        = "65623183-4333-4555-9B2A-AC2A78208E9B";
        public                  VSPackage                           Package                         { get ; private set; }
        public                  ErrorListProvider                   ErrorListProvider               { get ; private set; }
        public                  Library.SimpleLibrary               Library                         { get ; private set; }

        private                 List<Project>                       _projects;
        private                 EnvDTE.Events                       _events;
        private                 EnvDTE.BuildEvents                  _buildEvents;
        private                 EnvDTE.DocumentEvents               _documentEvents;
        private                 EnvDTE.ProjectItemsEvents           _projectItemsEvents;
        private                 EnvDTE.SolutionEvents               _solutionEvents;
        private                 uint                                _objectLibraryCookie;
        private                 object                              _lockObject;

        public                                                      Service(VSPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Package = package;

            _projects   = new List<Project>();
            _lockObject = new object();

            _registerErrorListProvider(package);
            _registerLibrary();
            _registerEvents();
        }
        public                  void                                Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _unregisterEvents();
            _stopProjects();
            _unregisterLibrary();
            _unregisterErrorListProvider();
        }

        public                  Project                             GetLanguageService(IVsProject vsproject)
        {
            Project project = null;
            bool    start = false;

            lock(_lockObject)
            {
                for (int i = 0 ; i < _projects.Count ; ++i) {
                    if (object.Equals(_projects[i].VSProject, vsproject)) {
                        project = _projects[i];
                        break;
                    }
                }

                if (project == null) {
                    project = new Project(this, vsproject);
                    _projects.Add(project);
                    start = true;
                }
            }

            if (start)
                project.Start();

            return project;
        }
        public                  Project                             FindLangaugeServiceByName(string projectName)
        {
            lock(_lockObject)
            {
                for (int i = 0 ; i < _projects.Count ; ++i) {
                    if (String.Compare(_projects[i].Name, projectName, true) == 0)
                        return _projects[i];
                }
            }

            return null;
        }
        public                  void                                DeRegisterLanguageService(Project languageService)
        {
            lock(_lockObject)
            {
                _projects.Remove(languageService);
            }
        }

        private                 void                                _event_Build_Done(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            foreach(var languageService in _toArray())
                languageService.Build_Done();
        }
        private                 void                                _event_Document_Closed(EnvDTE.Document Document)
        {
            foreach(var languageService in _toArray())
                languageService.Document_Closed(Document?.FullName);
        }
        private                 void                                _event_ProjectItem_AddedRemoved(EnvDTE.ProjectItem ProjectItem)
        {
            var languageService = _findLanguageService(VSPackage.GetContainingProject(CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, ProjectItem.Document?.FullName));

            if (languageService != null)
                languageService.Project_Changed();
        }
        private                 void                                _event_ProjectItem_Rename(EnvDTE.ProjectItem ProjectItem, string oldFileName)
        {
            var languageService = _findLanguageService(VSPackage.GetContainingProject(CPS.TypedTSqlUnconfiguredProject.ProjectTypeGuid, ProjectItem.Document?.FullName));

            if (languageService != null)
                languageService.Project_Changed();
        }
        private                 void                                _eventSolution_BeforeClosing()
        {
            _stopProjects();
        }
        private                 void                                _eventSolution_ProjectRemoved(EnvDTE.Project Project)
        {
            if (!string.IsNullOrEmpty(Project.FullName)) {
                var         vsproject = VSPackage.GetIVsProject(null, Project.FullName);
                Project     project   = null;

                lock(_lockObject)
                {
                    for (int i = 0 ; i < _projects.Count ; ++i) {
                        if (object.Equals(_projects[i].VSProject, vsproject)) {
                            project = _projects[i];
                            _projects.RemoveAt(i);
                            break;
                        }
                    }
                }

                project.Stop();
            }
        }

        private                 Project[]                           _toArray()
        {
            lock(_lockObject)
            {
                return _projects.ToArray();
            }
        }
        private                 Project                             _findLanguageService(IVsProject vsproject)
        {
            lock(_lockObject)
            {
                for (int i = 0 ; i < _projects.Count ; ++i) {
                    if (object.Equals(_projects[i].VSProject, vsproject))
                        return _projects[i];
                }
            }

            return null;
        }
        private                 void                                _stopProjects()
        {
            Project[]   languageServices;

            lock(_lockObject)
            {
                languageServices = _projects.ToArray();
                _projects.Clear();
            }

            foreach(var project in languageServices)
                project.Stop();
        }

        private                 void                                _registerErrorListProvider(VSPackage package)
        {
            try {
                ErrorListProvider = new ErrorListProvider(package) { ProviderName = "Types T-Sql errors", ProviderGuid = new Guid("7b26e18f-f4c6-4fe1-9629-78eae9ebe322") };
            }
            catch(Exception err) {
                _unregisterErrorListProvider();
                VSPackage.DisplayError(new Exception("Failed to register TypedTSql.ErrorListProvider.", err));
            }
        }
        private                 void                                _unregisterErrorListProvider()
        {
            if (ErrorListProvider != null) {
                ErrorListProvider.Dispose();
                ErrorListProvider = null;
            }
        }
        private                 void                                _registerLibrary()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try {
                Library = new Library.SimpleLibrary();
                ErrorHandler.ThrowOnFailure(((IServiceProvider)Package).GetService<IVsObjectManager2>(typeof(SVsObjectManager)).RegisterSimpleLibrary(Library, out _objectLibraryCookie));
            }
            catch(Exception err) {
                Library = null;
                VSPackage.DisplayError(new Exception("Failed to register TypedTSql.Library.", err));
            }
        }
        private                 void                                _unregisterLibrary()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Library != null) {
                ((IServiceProvider)Package).GetService<IVsObjectManager2>(typeof(SVsObjectManager)).UnregisterLibrary(_objectLibraryCookie);
            }
        }
        private                 void                                _registerEvents()
        {
            _events = VSPackage.GetDTE().Events;
            _buildEvents = _events.BuildEvents;
            _buildEvents.OnBuildDone        += new EnvDTE._dispBuildEvents_OnBuildDoneEventHandler(_event_Build_Done);
            _documentEvents = _events.DocumentEvents;
            _documentEvents.DocumentClosing += new EnvDTE._dispDocumentEvents_DocumentClosingEventHandler(_event_Document_Closed);
            _projectItemsEvents = _events.SolutionItemsEvents;
            _projectItemsEvents.ItemAdded   += _event_ProjectItem_AddedRemoved;
            _projectItemsEvents.ItemRemoved += _event_ProjectItem_AddedRemoved;
            _projectItemsEvents.ItemRenamed += new EnvDTE._dispProjectItemsEvents_ItemRenamedEventHandler(_event_ProjectItem_Rename);
            _solutionEvents = _events.SolutionEvents;
            _solutionEvents.BeforeClosing   += new EnvDTE._dispSolutionEvents_BeforeClosingEventHandler(_eventSolution_BeforeClosing);
            _solutionEvents.ProjectRemoved  += new EnvDTE._dispSolutionEvents_ProjectRemovedEventHandler(_eventSolution_ProjectRemoved);
        }
        private                 void                                _unregisterEvents()
        {
            _buildEvents.OnBuildDone        -= new EnvDTE._dispBuildEvents_OnBuildDoneEventHandler(_event_Build_Done);
            _documentEvents.DocumentClosing -= new EnvDTE._dispDocumentEvents_DocumentClosingEventHandler(_event_Document_Closed);
            _projectItemsEvents.ItemAdded   -= new EnvDTE._dispProjectItemsEvents_ItemAddedEventHandler(_event_ProjectItem_AddedRemoved);
            _projectItemsEvents.ItemRemoved -= new EnvDTE._dispProjectItemsEvents_ItemRemovedEventHandler(_event_ProjectItem_AddedRemoved);
            _projectItemsEvents.ItemRenamed -= new EnvDTE._dispProjectItemsEvents_ItemRenamedEventHandler(_event_ProjectItem_Rename);
            _solutionEvents.BeforeClosing   -= new EnvDTE._dispSolutionEvents_BeforeClosingEventHandler(_eventSolution_BeforeClosing);
            _solutionEvents.ProjectRemoved  -= new EnvDTE._dispSolutionEvents_ProjectRemovedEventHandler(_eventSolution_ProjectRemoved);
            _solutionEvents     = null;
            _projectItemsEvents = null;
            _documentEvents     = null;
            _buildEvents        = null;
            _events             = null;
        }
    }
}
