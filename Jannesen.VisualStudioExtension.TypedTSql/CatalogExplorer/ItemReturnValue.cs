using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemReturnValue: TreeViewItem
    {
        public                  ItemEntity                      ItemObject              { get ; private set; }

        public                                                  ItemReturnValue(ItemEntity itemObject, LTTS_DataModel.ISqlType sqlType)
        {
            this.ItemObject = itemObject;

            var control = itemObject.ItemProject.Control;
            base.Style  = control.GetStyle("TreeViewItemStyle");
            base.Header = new TextBlock() { Text  = "Returns : " + ItemObject.TypeName(sqlType) };
        }

        public                  void                                Refresh(LTTS_DataModel.ISqlType sqlType)
        {
            ((TextBlock)base.Header).Text = "Returns : " + ItemObject.TypeName(sqlType);
        }
    }
}
