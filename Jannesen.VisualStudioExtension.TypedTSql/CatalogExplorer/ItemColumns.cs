using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemColumns: Item
    {
        public                                              ItemColumns(ItemEntity itemEntity, LTTS_DataModel.IColumnList columns): base(itemEntity)
        {
            InitTreeViewItem((itemEntity.ItemType == ItemType.FunctionTable) ? "Returns table" : "Columns");
            base.IsExpanded = true;

            foreach(var column in columns)
                base.Items.Add(new ItemColumn(this, column));
        }

        public                  void                        Refresh(LTTS_DataModel.IColumnList columns)
        {
            int     i = 0;

            for ( ; i < base.Items.Count && i < columns.Count ; ++i)
                ((ItemColumn)base.Items[i]).Refresh(this, columns[i]);

            for ( ; i < columns.Count ; ++i)
                base.Items.Add(new ItemColumn(this, columns[i]));

            while (i < base.Items.Count)
                base.Items.RemoveAt(i);
        }

        protected   override    void                        OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Header == e.OriginalSource)
                ItemEntity.InsertColumns();

            e.Handled = true;
        }
    }
}
