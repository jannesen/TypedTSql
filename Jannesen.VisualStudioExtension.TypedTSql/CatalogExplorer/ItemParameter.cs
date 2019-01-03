using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemParameter: Item
    {
        public                  string                          ParameterName           { get ; private set; }

        public                                                  ItemParameter(ItemParameters itemParameters, LTTS_DataModel.Parameter parameter): base(itemParameters)
        {
            this.ParameterName  = parameter.Name;
            InitTreeViewItem(parameter.Name + " : " + ItemEntity.TypeName(parameter.SqlType));
        }

        public                  void                            Refresh(ItemParameters itemParameters, LTTS_DataModel.Parameter parameter)
        {
            var text = parameter.Name + " : " + ItemEntity.TypeName(parameter.SqlType);

            if (((TextBlock)base.Header).Text != text)
                ((TextBlock)base.Header).Text = text;
        }

        protected   override    void                            OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            VSPackage.InsertTextInActiveDocument(ParameterName + " = ", activeDocument:true);
        }
    }
}
