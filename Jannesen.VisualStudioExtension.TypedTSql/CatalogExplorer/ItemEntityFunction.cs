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
    public abstract class ItemEntityFunction: ItemEntity
    {
        public                                          ItemEntityFunction(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name, entity)
        {
        }

        protected   override    void                    OnCreateContextMenu1(LTTS_DataModel.Entity entity)
        {
            base.OnCreateContextMenu1(entity);
            AddContextMenuItem("Insert function call with arguments",   _onInsertFunctionCall);
        }
        protected   async       void                    _onInsertFunctionCall(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReadyAndLocked((project) => {
                        var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName, true);

                        StringBuilder   rtn = new StringBuilder();

                        rtn.Append(LTTS_Library.SqlStatic.QuoteName(entity.EntityName.Name));
                        rtn.Append('(');

                        bool    next = false;

                        if (((LTTS_DataModel.EntityObjectCode)entity).Parameters != null) {
                            foreach(LTTS_DataModel.Parameter parameters in ((LTTS_DataModel.EntityObjectCode)entity).Parameters) {
                                if (next)
                                    rtn.Append(", ");

                                rtn.Append("/*");
                                rtn.Append(parameters.Name);
                                rtn.Append("=*/");
                                next = true;
                            }
                        }

                        rtn.Append(')');

                        VSPackage.InsertTextInActiveDocument(rtn.ToString(), activeDocument:true);
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnInsertFunctionCall failed.", err));
            }
        }
    }
}
