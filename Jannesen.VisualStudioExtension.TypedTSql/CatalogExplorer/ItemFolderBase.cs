using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemFolderBase: ItemWithName
    {
        public                  ItemList                Children                { get ; private set; }
        public                  ItemList                Entities                { get ; private set; }

        public                                          ItemFolderBase(ItemWithName parent, string name): base(parent, name)
        {
            Children = new ItemList();
            Entities = new ItemList();
        }

        public                  void                    AfterRefresh(HashSet<object> definedItems)
        {
            foreach (ItemFolder itemFolder in Children.ToArray()) {
                itemFolder.AfterRefresh(definedItems);

                if (itemFolder.Children.Count == 0 && itemFolder.Entities.Count == 0) {
                    base.Items.Remove(itemFolder);
                    Children.Remove(itemFolder);
                }
            }

            foreach (ItemEntity itemObject in Entities.ToArray()) {
                if (!definedItems.Contains(itemObject)) {
                    base.Items.Remove(itemObject);
                    Entities.Remove(itemObject);
                }
            }
        }
        public                  ItemFolderBase          GetChild(string name)
        {
            ItemFolderBase  child = (ItemFolderBase)Children.Find(name);

            if (child == null)
                Children.Add(child = new ItemFolder(this, name));

            return child;
        }
        public                  ItemEntity              AddEntity(string name, LTTS_DataModel.Entity entity, ItemType filter, bool expanded)
        {
            for (int i = 0 ; i < Entities.Count ; ++i) {
                ItemEntity  itemEntity = (ItemEntity)Entities[i];

                if (itemEntity.EntityType == entity.Type &&
                    itemEntity.EntityName == entity.EntityName)
                {
                    itemEntity.RefreshEntity(entity, filter, expanded && this.IsExpanded);
                    return itemEntity;
                }
            }

            ItemEntity newItemObject = ItemEntity.Create(this, name, entity);

            if (newItemObject != null)
                Entities.Add(newItemObject);

            return newItemObject;
        }
        public                  void                    SortItems()
        {
            foreach(ItemFolder folder in Children)
                folder.SortItems();

            Children.Sort();
            Entities.Sort();
        }
        public                  void                    SelectItems(ItemType filter)
        {
            try {
                int                 n     = 0;

                foreach (ItemFolder itemFolder in Children) {
                    itemFolder.SelectItems(filter);
                    _setItem(ref n, itemFolder, itemFolder.Items.Count > 0);
                }

                foreach (ItemEntity itemObject in Entities) {
                    _setItem(ref n, itemObject, (itemObject.ItemType & filter) != ItemType.None);
                }

                while(n > base.Items.Count)
                    base.Items.RemoveAt(n);
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("SelectItems '" + ItemName + "' failed.", err));
            }
        }

        private                 void                    _setItem(ref int n, TreeViewItem treeViewItem, bool selected)
        {
            if (selected) {
                if (!(n < base.Items.Count && object.ReferenceEquals(base.Items[n], treeViewItem))) {
                    if (treeViewItem.Parent != null)
                        base.Items.Remove(treeViewItem);

                    base.Items.Insert(n, treeViewItem);
                }

                ++n;
            }
            else {
                if (n < base.Items.Count && object.ReferenceEquals(base.Items[n], treeViewItem))
                    base.Items.RemoveAt(n);
            }
        }
    }
}
