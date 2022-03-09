using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VSShell              = Microsoft.VisualStudio.Shell;
using VSInterop            = Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemProject: ItemFolderBase
    {
        public                  ContentControl                  Control             { get ; private set; }
        public                  VSInterop.IVsProject            VSProject           { get ; private set; }
        public                  string                          ProjectFilename     { get ; private set; }

        public                                                  ItemProject(ContentControl control, VSInterop.IVsProject project, string projectfile): base(null, System.IO.Path.GetFileNameWithoutExtension(projectfile))
        {
            this.Control         = control;
            this.VSProject       = project;
            this.ProjectFilename = projectfile;

            InitTreeViewItem(Microsoft.VisualStudio.Imaging.KnownMonikers.Database);
        }

        internal                Task                            WhenReadyAndLocked(LanguageService.ReadyCallback callback)
        {
            if (!(ServiceProvider.GetService(typeof(LanguageService.Service)) is LanguageService.Service service)) {
                throw new InvalidOperationException("LanguageService.Service not registrated.");
            }

            return service.GetLanguageService(VSProject).WhenReadyAndLocked(callback, CancellationToken.None);
        }

        public      async       Task                            Refresh()
        {
            try {
                await VSShell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await WhenReadyAndLocked((project) => {
                        var     catalog      = project.GlobalCatalog;
                        var     definedItems = new HashSet<object>();

                        foreach(var entity in catalog.Entities) {
                            if (ItemEntity.hasItemObject(entity.Type) && entity.EntityName.Database == null) {
                                ItemFolderBase  folder = this.GetChild(entity.EntityName.Schema);

                                string[]    nameParts = entity.EntityName.Name.Split('/');

                                for (int i = 0 ; i < nameParts.Length - 1 ; ++i)
                                    folder = folder.GetChild(nameParts[i]);

                                definedItems.Add(folder.AddEntity(nameParts[nameParts.Length - 1], entity, Control.Filter, true));
                            }
                        }

                        SortItems();
                        AfterRefresh(definedItems);
                        SelectItems(Control.Filter);
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("Failed to refresh database entities '"+ProjectFilename+"'.", err));
            }
        }
    }
}
