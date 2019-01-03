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
    public class ItemEntityStoredProcedure: ItemEntity
    {
        public      override    ItemType                ItemType
        {
            get {
                return ItemType.StoreProcedure;
            }
        }

        public                                          ItemEntityStoredProcedure(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name, entity)
        {
            InitTreeViewItem(entity, Microsoft.VisualStudio.Imaging.KnownMonikers.StoredProcedure);
        }

        protected   override    void                    OnCreateContextMenu1(LTTS_DataModel.Entity entity)
        {
            base.OnCreateContextMenu1(entity);
            AddContextMenuItem("Insert procedure call with arguments",  _onInsertProcedureCall);
        }
        protected   async       void                    _onInsertProcedureCall(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReady((project) => {
                        var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName, true);
                        StringBuilder   rtn = new StringBuilder();

                        rtn.Append("exec ");
                        rtn.Append(LTTS_Library.SqlStatic.QuoteName(EntityName.Name));
                        rtn.Append(" ");

                        bool    next = false;
                        string  prefix = ",\n" + new string(' ', rtn.Length);

                        int     plength = 2;

                        if (((LTTS_DataModel.EntityObjectCode)entity).Parameters != null) {
                            foreach(var parameters in ((LTTS_DataModel.EntityObjectCode)entity).Parameters) {
                                if (plength < parameters.Name.Length)
                                    plength = parameters.Name.Length;
                            }

                            foreach(var parameters in ((LTTS_DataModel.EntityObjectCode)entity).Parameters) {
                                if (next)
                                    rtn.Append(prefix);

                                rtn.Append(parameters.Name);
                                rtn.Append(' ', plength - parameters.Name.Length);
                                rtn.Append(" = ");
                                next = true;
                            }
                        }

                        VSPackage.InsertTextInActiveDocument(rtn.ToString(), activeDocument:true);
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnInsertProcedureCall failed.", err));
            }
        }
    }
}
