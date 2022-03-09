using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemEntityTableUser: ItemEntity
    {
        public      override    ItemType                ItemType
        {
            get {
                return ItemType.Table;
            }
        }

        public                                          ItemEntityTableUser(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name, entity)
        {
            InitTreeViewItem(entity, Microsoft.VisualStudio.Imaging.KnownMonikers.Table);
        }

        protected   override    void                    OnCreateContextMenu2(LTTS_DataModel.Entity entity)
        {
            base.OnCreateContextMenu2(entity);
            AddContextMenuItem("Rename",                _onRename);
        }

        private     async       void                    _onRename(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReadyAndLocked((project) => {
                                                    var entityName  = ItemEntity.EntityName;
                                                    var entityTable = project.GlobalCatalog.GetObject(entityName);
                                                    if (!(entityTable is LTTS_DataModel.EntityObjectTable))
                                                        throw new Exception("Can't find table '" + entityName.Fullname + "' in global catalog");

                                                    (new Rename.Renamer(ServiceProvider,
                                                                        project,
                                                                        entityTable)).Run();
                                            });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnRename failed.", err));
            }
        }
    }
}
