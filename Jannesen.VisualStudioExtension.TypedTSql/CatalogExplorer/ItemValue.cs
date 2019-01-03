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
    public class ItemValue: Item
    {
        public                  string                          EntityName          { get ; private set; }
        public                  string                          ValueName           { get ; private set; }

        public                                                  ItemValue(ItemValues itemValues, LTTS_DataModel.ValueRecord value): base(itemValues)
        {
            this.ValueName  = value.Name;

            InitTreeViewItem(LTTS_Library.SqlStatic.QuoteName(value.Name));
        }

        public                  void                            Refresh(LTTS_DataModel.ValueRecord value)
        {
            var text = LTTS_Library.SqlStatic.QuoteName(value.Name);

            if (((TextBlock)base.Header).Text != text)
                ((TextBlock)base.Header).Text = text;
        }

        protected   override    void                            OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            VSPackage.InsertTextInActiveDocument(LTTS_Library.SqlStatic.QuoteName(ItemEntity.EntityName.Name) + "::" + LTTS_Library.SqlStatic.QuoteName(ValueName), activeDocument:true);
            e.Handled = true;
        }
    }
}
