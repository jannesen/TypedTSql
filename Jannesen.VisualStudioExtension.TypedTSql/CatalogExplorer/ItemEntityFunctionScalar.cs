using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemEntityFunctionScalar: ItemEntityFunction
    {
        public      override    ItemType                ItemType
        {
            get {
                return ItemType.FunctionScalar;
            }
        }

        public                                          ItemEntityFunctionScalar(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name, entity)
        {
            InitTreeViewItem(entity, Microsoft.VisualStudio.Imaging.KnownMonikers.ScalarFunction);
        }
    }
}
