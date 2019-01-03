﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemEntityType: ItemEntity
    {
        public      override    ItemType                ItemType
        {
            get {
                return ItemType.DataType;
            }
        }

        public                                          ItemEntityType(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name, entity)
        {
            InitTreeViewItem(entity, Microsoft.VisualStudio.Imaging.KnownMonikers.UserDefinedDataType);
        }
    }
}
