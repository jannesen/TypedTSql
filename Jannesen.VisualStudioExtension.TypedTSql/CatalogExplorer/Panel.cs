using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    [Guid(Panel.GUID)]
    public class Panel: ToolWindowPane
    {
        public      const           string          GUID = "ed4f63ae-6861-4a7d-b3aa-7a365432aabe";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public                                      Panel(IServiceProvider serviceProvider) : base(null)
        {
            this.Caption = "TypedTSql Catalog Explorer";
            this.Content = new ContentControl(serviceProvider);
        }

        protected   override        void            Dispose(bool disposing)
        {
            if (disposing) {
                ((ContentControl)this.Content).Dispose();
            }

            base.Dispose(disposing);
        }

        public      static          bool            IsCatalogExplorerActive(VSPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ToolWindowPane panel = package.FindToolWindow(typeof(Panel), 0, false) as CatalogExplorer.Panel;
            if (panel == null || panel.Frame == null)
                return false;

            return ((ContentControl)panel.Content).IsLoaded;
        }
        public      static          void            Refresh(VSPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ToolWindowPane panel = package.FindToolWindow(typeof(CatalogExplorer.Panel), 0, false) as CatalogExplorer.Panel;
            if ((panel == null) || (panel.Frame == null))
                return;

            ((ContentControl)panel.Content).Refresh();
        }
    }
}
