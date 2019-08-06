using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_Library   = Jannesen.Language.TypedTSql.Library;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemColumn: Item
    {
        public                  string                          ColumnName          { get ; private set; }

        public                                                  ItemColumn(ItemColumns itemColumns, LTTS_DataModel.Column column): base(itemColumns)
        {
            this.ColumnName  = column.Name;
            InitTreeViewItem(column.Name + " : " + ItemEntity.TypeName(column.SqlType));
        }

        public                  void                            Refresh(ItemColumns itemColumns, LTTS_DataModel.Column column)
        {
            this.ColumnName  = column.Name;
            var text = column.Name + " : " + ItemEntity.TypeName(column.SqlType);

            if (((TextBlock)base.Header).Text != text)
                ((TextBlock)base.Header).Text = text;
        }

        protected   override    void                            OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (EventIsForThisItem(e)) {
                if (this.ContextMenu == null) {
                    AddContextMenuItem("Find All References", _onFindAllReferences);

                    if (ItemEntity is ItemEntityTableUser)
                        AddContextMenuItem("Rename",              _onRenamer);
                }

                base.IsSelected = true;
                this.ContextMenu.PlacementTarget = this;
                this.ContextMenu.IsOpen          = true;
            }

            e.Handled = true;
        }
        protected   override    void                            OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            VSPackage.InsertTextInActiveDocument(LTTS_Library.SqlStatic.QuoteName(ColumnName), activeDocument:true);
            e.Handled = true;
        }

        private     async       void                            _onFindAllReferences(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReady((project) => {
                        LanguageService.Library.SimpleLibrary.SearchReferences(ServiceProvider, ItemProject.VSProject, project.FindReferences(_getColumn(project)));
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnFindAllReferences", err));
            }
        }
        private     async       void                            _onRenamer(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReady((project) => {
                                                (new Rename.Renamer(ServiceProvider,
                                                                    project,
                                                                    _getColumn(project))).Run();
                                            });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnRenamer failed.", err));
            }
        }
        private                 LTTS_DataModel.Column           _getColumn(LanguageService.Project project)
        {
            var itemEntity  = ItemEntity;
            var entityTable = (LTTS_DataModel.EntityObjectTable)project.GlobalCatalog.GetEntity(itemEntity.EntityType, itemEntity.EntityName);
            var column = entityTable.Columns.FindColumn(ColumnName, out var ambigous);
            if (column == null)
                throw new Exception("Can't find column '" + itemEntity.EntityName.Fullname + "." + ColumnName + "' in global catalog.");

            return column;
        }
    }
}
