﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public class ItemEntityView: ItemEntity
    {
        public      override    ItemType                ItemType
        {
            get {
                return ItemType.View;
            }
        }

        public                                          ItemEntityView(ItemWithName folder, string name, LTTS_DataModel.Entity entity): base(folder, name, entity)
        {
            InitTreeViewItem(entity, Microsoft.VisualStudio.Imaging.KnownMonikers.View);
        }
    }
}
