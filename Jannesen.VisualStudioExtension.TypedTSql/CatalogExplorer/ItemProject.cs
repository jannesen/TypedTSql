using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VSShell              = Microsoft.VisualStudio.Shell;
using VSInterop            = Microsoft.VisualStudio.Shell.Interop;
using LTTS = Jannesen.Language.TypedTSql;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

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

        internal                Task                            WhenReady(LanguageService.ReadyCallback callback)
        {
            var service = ServiceProvider.GetService(typeof(LanguageService.Service)) as LanguageService.Service;
            if (service == null) {
                throw new InvalidOperationException("LanguageService.Service not registrated.");
            }

            return service.GetLanguageService(VSProject).WhenReady(callback);
        }

        public      async       Task                            Refresh()
        {
            try {
                await VSShell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await WhenReady((project) => {
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
