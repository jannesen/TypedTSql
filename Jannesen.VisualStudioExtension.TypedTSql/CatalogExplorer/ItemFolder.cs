using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemFolder: ItemFolderBase
    {
        public                                          ItemFolder(ItemWithName parent, string name): base(parent, name)
        {
            InitTreeViewItem(Microsoft.VisualStudio.Imaging.KnownMonikers.FolderClosed);
        }

        protected   override    void                    OnCollapsed(RoutedEventArgs e)
        {
            SetMoniker(Microsoft.VisualStudio.Imaging.KnownMonikers.FolderClosed);
        }
        protected   override    void                    OnExpanded(RoutedEventArgs e)
        {
            SetMoniker(Microsoft.VisualStudio.Imaging.KnownMonikers.FolderOpened);
        }
    }
}
