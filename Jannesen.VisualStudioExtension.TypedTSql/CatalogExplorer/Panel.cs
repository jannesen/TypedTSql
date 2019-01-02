using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    [Guid("ed4f63ae-6861-4a7d-b3aa-7a365432aabe")]
    public class Panel: ToolWindowPane
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public                                      Panel() : base(null)
        {
            this.Caption = "TypedTSql Catalog Explorer";
            this.Content = new ContentControl();
        }

        protected   override        void            Dispose(bool disposing)
        {
            if (disposing) {
                ((ContentControl)this.Content).Dispose();
            }

            base.Dispose(disposing);
        }

        public      static          void            ShowCatalogExplorer(VSPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ToolWindowPane panel = package.FindToolWindow(typeof(Panel), 0, true);
            if (panel == null || panel.Frame == null)
                throw new NotSupportedException("Cannot create TypedTSql Catalog Explorer tool window.");

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(((IVsWindowFrame)panel.Frame).Show());
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
