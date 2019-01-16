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
        public                                                  ItemReturnValue(ItemEntity itemObject, LTTS_DataModel.ISqlType sqlType)
        {
            var control = itemObject.ItemProject.Control;
            base.Style  = control.GetStyle("TreeViewItemStyle");
            base.Header = new TextBlock() { Text  = "Returns : " + itemObject.TypeName(sqlType) };
        }

        public                  void                            Refresh(ItemEntity itemObject, LTTS_DataModel.ISqlType sqlType)
        {
            ((TextBlock)base.Header).Text = "Returns : " + itemObject.TypeName(sqlType);
        }
    }
}
