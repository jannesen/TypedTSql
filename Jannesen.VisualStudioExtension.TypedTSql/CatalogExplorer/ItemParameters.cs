using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemParameters: Item
    {
        public                                                      ItemParameters(ItemEntity itemObject, LTTS_DataModel.ParameterList parameters): base(itemObject)
        {
            InitTreeViewItem("Parameters");
            base.IsExpanded = true;

            foreach(var parameter in parameters)
                base.Items.Add(new ItemParameter(this, parameter));
        }

        public                  void                                Refresh(LTTS_DataModel.ParameterList parameters)
        {
            int     i = 0;

            for ( ; i < base.Items.Count && i < parameters.Count ; ++i)
                ((ItemParameter)base.Items[i]).Refresh(this, parameters[i]);

            for ( ; i < parameters.Count ; ++i)
                base.Items.Add(new ItemParameter(this, parameters[i]));

            while (i < base.Items.Count)
                base.Items.RemoveAt(i);
        }

        protected   override    void                                OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.Header == e.OriginalSource)
                ItemEntity.InsertParameters();

            e.Handled = true;
        }
    }
}
