using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemValues: Item
    {
        public                                                      ItemValues(ItemEntity itemEntity, LTTS_DataModel.ValueRecordList values): base(itemEntity)
        {
            InitTreeViewItem("Values");
            base.IsExpanded = true;

            foreach(var value in values)
                base.Items.Add(new ItemValue(this, value));
        }

        public                  void                                Refresh(LTTS_DataModel.ValueRecordList values)
        {
            int     i = 0;

            for ( ; i < base.Items.Count && i < values.Count ; ++i)
                ((ItemValue)base.Items[i]).Refresh(values[i]);

            for ( ; i < values.Count ; ++i)
                base.Items.Add(new ItemValue(this, values[i]));

            while (i < base.Items.Count)
                base.Items.RemoveAt(i);
        }
    }
}
