using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    [Flags]
    public enum ItemType
    {
        None                = 0x0000,
        DataType            = 0x0001,
        Table               = 0x0002,
        View                = 0x0004,
        FunctionScalar      = 0x0008,
        FunctionTable       = 0x0010,
        StoreProcedure      = 0x0020,
        Trigger             = 0x0040,
    }

    public abstract class Item: TreeViewItem
    {
        public                  Item                    ItemParent              { get ; private set; }

        public                  IServiceProvider        ServiceProvider
        {
            get {
                return ItemProject.Control.ServiceProvider;
            }
        }

        public                  ItemEntity              ItemEntity
        {
            get {
                for (var f = this ; f != null ; f = f.ItemParent) {
                    if (f is ItemEntity itemEntity)
                        return itemEntity;
                }

                throw new InvalidOperationException("Internal error can't find ItemEntity");
            }
        }
        public                  ItemProject             ItemProject
        {
            get {
                for (var f = this ; f != null ; f = f.ItemParent) {
                    if (f is ItemProject itemProject)
                        return itemProject;
                }

                throw new InvalidOperationException("Internal error can't find ItemProject");
            }
        }

        public                                          Item(Item parent)
        {
            this.ItemParent = parent;
        }

        protected               void                    InitTreeViewItem(string text)
        {
            var control = ItemProject.Control;
            base.Style      = control.GetStyle("TreeViewItemStyle");
            base.Header     = new TextBlock() { Text  = text };
        }
        protected               bool                    EventIsForThisItem(RoutedEventArgs e)
        {
            if (this.Header == e.OriginalSource)
                return true;

            if (this.Header is StackPanel &&  ((StackPanel)(this.Header)).Children.Contains(e.OriginalSource as UIElement))
                return true;

            return false;
        }
        protected               void                    AddContextMenuItem(string text, RoutedEventHandler click)
        {
            if (base.ContextMenu == null)
                base.ContextMenu = new ContextMenu();

            MenuItem    menuItem = new MenuItem()
                                    {
                                        Header = text,
                                    };

            menuItem.Click += click;

            base.ContextMenu.Items.Add(menuItem);
        }
        protected               void                    AddContextMenuSeperator()
        {
            if (base.ContextMenu != null)
                base.ContextMenu.Items.Add(new Separator());
        }
    }

    public abstract class ItemWithName: Item
    {
        public                  string                  ItemName                { get ; private set; }

        public                                          ItemWithName(Item parent, string name): base(parent)
        {
            this.ItemName   = name;
        }

        protected               void                    InitTreeViewItem(ImageMoniker moniker)
        {
            var control = ItemProject.Control;

            base.Style = control.GetStyle("TreeViewItemStyle");

            var panel = new StackPanel() { Orientation = Orientation.Horizontal };
            panel.Children.Add(new CrispImage() { Width = 16, Height = 16, Margin = new Thickness(0, 0, 6, 0), Moniker = moniker });
            panel.Children.Add(new TextBlock()  { Text = ItemName });
            base.Header = panel;
        }
        protected               void                    SetMoniker(ImageMoniker moniker)
        {
            ((CrispImage)((StackPanel)base.Header).Children[0]).Moniker = moniker;
        }

    }

    public class ItemList: List<ItemWithName>
    {
        public                  ItemWithName           Find(string name)
        {
            for (int i = 0 ; i < this.Count ; ++i) {
                if (this[i].ItemName == name)
                    return this[i];
            }

            return null;
        }

        public      new         void                    Sort()
        {
            base.Sort((i1,i2) => string.Compare(i1.ItemName, i2.ItemName, true));
        }
    }
}
