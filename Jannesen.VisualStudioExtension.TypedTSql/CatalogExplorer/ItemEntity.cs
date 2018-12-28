using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Imaging.Interop;
using LTTS_Library   = Jannesen.Language.TypedTSql.Library;
using LTTS_DataModel = Jannesen.Language.TypedTSql.DataModel;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.CatalogExplorer
{
    public abstract class ItemEntity: ItemWithName
    {
        struct EntityData
        {
            public          LTTS_DataModel.ParameterList        Parameters;
            public          LTTS_DataModel.IColumnList          Columns;
            public          LTTS_DataModel.ISqlType             ReturnValue;
            public          LTTS_DataModel.ValueRecordList      Values;

            public                                              EntityData(LTTS_DataModel.Entity entity)
            {
                this.Parameters  = null;
                this.Columns     = null;
                this.ReturnValue = null;
                this.Values      = null;

                switch(entity)
                {
                case LTTS_DataModel.EntityObjectCode entityCode:
                    {
                        this.Parameters = entityCode.Parameters;

                        var returns = entityCode.Returns;
                        if (returns != null) {
                            if ((returns.TypeFlags & LTTS_DataModel.SqlTypeFlags.Table) != 0)
                                this.Columns = returns.Columns;
                            else
                                this.ReturnValue = returns;
                        }
                    }
                    break;

                case LTTS_DataModel.EntityObjectTable entityTable:
                    {
                        this.Columns = entityTable.Columns;
                    }
                    break;

                case LTTS_DataModel.EntityTypeUser entityType:
                    {
                        if ((entityType.TypeFlags & LTTS_DataModel.SqlTypeFlags.Values) != 0 && entityType.Values != null)
                            this.Values = entityType.Values;

                        if ((entityType.TypeFlags & LTTS_DataModel.SqlTypeFlags.Table) != 0)
                            this.Columns = entityType.Columns;
                    }
                    break;
                }
            }
        }

        public      abstract    ItemType                    ItemType                { get ; }
        public      readonly    LTTS_DataModel.SymbolType   EntityType;
        public      readonly    LTTS_DataModel.EntityName   EntityName;
        private                 bool                        _objectDataLoaded;

        protected                                           ItemEntity(ItemWithName parent, string name, LTTS_DataModel.Entity entity): base(parent, name)
        {
            this.EntityType = entity.Type;
            this.EntityName = entity.EntityName;
        }

        public      static      bool                        hasItemObject(LTTS_DataModel.SymbolType type)
        {
            switch(type)
            {
            case LTTS_DataModel.SymbolType.TypeUser:
            case LTTS_DataModel.SymbolType.TypeExternal:
            case LTTS_DataModel.SymbolType.TypeTable:
            case LTTS_DataModel.SymbolType.TableUser:
            case LTTS_DataModel.SymbolType.View:
            case LTTS_DataModel.SymbolType.FunctionScalar:
            case LTTS_DataModel.SymbolType.FunctionScalar_clr:
            case LTTS_DataModel.SymbolType.FunctionInlineTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable_clr:
            case LTTS_DataModel.SymbolType.StoredProcedure:
            case LTTS_DataModel.SymbolType.StoredProcedure_clr:
            case LTTS_DataModel.SymbolType.StoredProcedure_extended:
            case LTTS_DataModel.SymbolType.Trigger:
            case LTTS_DataModel.SymbolType.Trigger_clr:
                return true;

            default:
                return false;
            }

        }
        public      static      ItemEntity                  Create(ItemWithName parent, string name, LTTS_DataModel.Entity entity)
        {
            switch(entity.Type)
            {
            case LTTS_DataModel.SymbolType.TypeUser:                            return new ItemEntityType(parent, name, entity);
            case LTTS_DataModel.SymbolType.TypeExternal:                        return new ItemEntityType(parent, name, entity);
            case LTTS_DataModel.SymbolType.TypeTable:                           return new ItemEntityType(parent, name, entity);
            case LTTS_DataModel.SymbolType.TableUser:                           return new ItemEntityTableUser(parent, name, entity);
            case LTTS_DataModel.SymbolType.View:                                return new ItemEntityView(parent, name, entity);
            case LTTS_DataModel.SymbolType.FunctionScalar:
            case LTTS_DataModel.SymbolType.FunctionScalar_clr:                  return new ItemEntityFunctionScalar(parent, name, entity);
            case LTTS_DataModel.SymbolType.FunctionInlineTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable_clr:
            case LTTS_DataModel.SymbolType.FunctionAggregateFunction_clr:       return new ItemEntityFunctionTable(parent, name, entity);
            case LTTS_DataModel.SymbolType.StoredProcedure:
            case LTTS_DataModel.SymbolType.StoredProcedure_clr:
            case LTTS_DataModel.SymbolType.StoredProcedure_extended:            return new ItemEntityStoredProcedure(parent, name, entity);
            case LTTS_DataModel.SymbolType.Trigger:
            case LTTS_DataModel.SymbolType.Trigger_clr:
            default:                                                            return new ItemEntityTrigger(parent, name, entity);
            }
        }

        public                  void                        RefreshEntity(LTTS_DataModel.Entity entity, ItemType filter, bool expanded)
        {
            try {
                if (_objectDataLoaded) {
                    if (expanded && (ItemType & filter) != 0 && base.IsExpanded) {
                        base.ContextMenu = null;
                        OnCreateContextMenu1(entity);
                        OnCreateContextMenu2(entity);

                        var data = new EntityData(entity);
                        var curParameters  = _getChild<ItemParameters>();
                        var curValues      = _getChild<ItemValues>();
                        var curColumns     = _getChild<ItemColumns>();
                        var curReturnValue = _getChild<ItemReturnValue>();
                        var items = new List<TreeViewItem>();

                        if (data.Parameters  != null) {
                            if (curParameters != null) {
                                curParameters.Refresh(data.Parameters);
                                items.Add(curParameters);
                            }
                            else
                                items.Add(new ItemParameters (this, data.Parameters));
                        }
                        if (data.Values      != null) {
                            if (curValues != null) {
                                curValues.Refresh(data.Values);
                                items.Add(curValues);
                            }
                            else
                                items.Add(new ItemValues     (this, data.Values));
                        }
                        if (data.Columns     != null) {
                            if (curColumns != null) {
                                curColumns.Refresh(data.Columns);
                                items.Add(curColumns);
                            }
                            else
                                items.Add(new ItemColumns    (this, data.Columns));
                        }
                        if (data.ReturnValue != null) {
                            if (curReturnValue != null) {
                                curReturnValue.Refresh(data.ReturnValue);
                                items.Add(curReturnValue);
                            }
                            else
                                items.Add(new ItemReturnValue(this, data.ReturnValue));
                        }

                        int i = 0;
                        while (i < items.Count) {
                            if (!(i < base.Items.Count && object.ReferenceEquals(base.Items[i], items[i]))) {
                                if (items[i].Parent != null)
                                    base.Items.Remove(items[i]);

                                base.Items.Insert(i, items[i]);
                            }
                            ++i;
                        }
                        while (i < base.Items.Count)
                            base.Items.RemoveAt(i);
                    }
                    else {
                        _objectDataLoaded = false;
                        base.IsExpanded  = false;
                        base.ContextMenu = null;
                        base.Items.Clear();
                        _setChildrenNotLoaded(entity);
                    }
                }
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("Refresh '" + ItemName + "' failed.", err));
            }
        }

        public      async       Task                        LoadObjectData()
        {
            if (!_objectDataLoaded) {
                _objectDataLoaded = true;

                try {
                    await ItemProject.WhenReady((project) => {
                            var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName, true);
                            var data = new EntityData(entity);

                            base.Items.Clear();
                            if (data.Parameters  != null)   base.Items.Add(new ItemParameters (this, data.Parameters));
                            if (data.Values      != null)   base.Items.Add(new ItemValues     (this, data.Values));
                            if (data.Columns     != null)   base.Items.Add(new ItemColumns    (this, data.Columns));
                            if (data.ReturnValue != null)   base.Items.Add(new ItemReturnValue(this, data.ReturnValue));

                            OnCreateContextMenu1(entity);
                            OnCreateContextMenu2(entity);
                        });
                }
                catch(Exception err) {
                    VSPackage.DisplayError(new Exception("LoadObjectData '" + ItemName + "' failed.", err));

                    base.Items.Clear();
                    base.Items.Add(new TreeViewItem()
                                        {
                                            Style  = ItemProject.Control.GetStyle("TreeViewItemStyle"),
                                            Header = "Load failed..."
                                        });

                }
            }
        }
        public                  void                        InsertObjectName()
        {
            try {
                VSPackage.InsertTextInActiveDocument(LTTS_Library.SqlStatic.QuoteName(EntityName.Name), activeDocument:true);
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("InsertObjectName failed.", err));
            }
        }
        public      async       void                        InsertParameters(string separator = ", ")
        {
            try {
                await ItemProject.WhenReady((project) => {
                        var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName, true);

                        if (entity is LTTS_DataModel.EntityObjectCode entityCode) {
                            if (entityCode.Parameters != null) {
                                StringBuilder s = new StringBuilder();

                                foreach(var parameters in entityCode.Parameters) {
                                    if (s.Length > 0)
                                        s.Append(separator);

                                    s.Append(parameters.Name);
                                    s.Append(" = ");
                                }

                                VSPackage.InsertTextInActiveDocument(s.ToString(), activeDocument:true);
                            }
                        }
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("InsertParameters failed.", err));
            }
        }
        public      async       void                        InsertColumns(string separator = ", ")
        {
            try {
                await ItemProject.WhenReady((project) => {
                        var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName, true);
                        var columns = _getColumns(entity);
                        if (columns != null) {
                            StringBuilder s = new StringBuilder();

                            foreach(var column in columns) {
                                if (s.Length > 0)
                                    s.Append(separator);

                                s.Append(LTTS_Library.SqlStatic.QuoteName(column.Name));
                            }

                            VSPackage.InsertTextInActiveDocument(s.ToString(), activeDocument:true);
                        }
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("InsertColumns failed.", err));
            }
        }

        public                  string                      TypeName(LTTS_DataModel.ISqlType type)
        {
            if (type.Entity != null) {
                return type.Entity.EntityName.Schema == this.EntityName.Schema
                        ? LTTS_Library.SqlStatic.QuoteName(type.Entity.EntityName.Name)
                        : type.Entity.EntityName.Fullname;

            }

            return type.ToString();
        }
        protected   virtual     void                        OnCreateContextMenu1(LTTS_DataModel.Entity entity)
        {
            AddContextMenuItem("Insert object-name", _onInsertObjectName);

            if (entity is LTTS_DataModel.EntityObjectCode entityCode && entityCode.Parameters != null) {
                AddContextMenuItem("Insert all parameters",         _onInsertParameters);
                AddContextMenuItem("Insert all parameters newline", _onInsertParametersNewLine);
            }

            if (_getColumns(entity) != null) {
                AddContextMenuItem("Insert all columns-names",         _onInsertColumns);
                AddContextMenuItem("Insert all columns-names newline", _onInsertColumnsNewLine);
            }
        }
        protected   virtual     void                        OnCreateContextMenu2(LTTS_DataModel.Entity entity)
        {
            AddContextMenuSeperator();

            if (entity.Declaration != null)
                AddContextMenuItem("Go To Declaration", _onGotoDeclaration);

            AddContextMenuItem("Find All References",   _onFindAllReferences);
        }
        protected               void                        InitTreeViewItem(LTTS_DataModel.Entity entity, ImageMoniker moniker)
        {
            base.InitTreeViewItem(moniker);
            base.ToolTip =  entity.EntityName.Fullname;
            _setChildrenNotLoaded(entity);
        }

        protected   override    void                        OnExpanded(RoutedEventArgs e)
        {
            var t = LoadObjectData();
        }
        protected   override    void                        OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EventIsForThisItem(e))
                InsertObjectName();

            e.Handled = true;
        }
        protected   override    void                        OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (EventIsForThisItem(e)) {
                _openContextMenu();
                base.IsSelected = true;
            }

            e.Handled = true;
        }

        protected   async       void                        _openContextMenu()
        {
            await LoadObjectData();
            this.ContextMenu.PlacementTarget = this;
            this.ContextMenu.IsOpen          = true;
        }
        protected               void                        _onInsertObjectName(object s,RoutedEventArgs e)
        {
            InsertObjectName();
        }
        protected               void                        _onInsertParameters(object s,RoutedEventArgs e)
        {
            InsertParameters();
        }
        protected               void                        _onInsertParametersNewLine(object s,RoutedEventArgs e)
        {
            InsertParameters(",\n");
        }
        protected               void                        _onInsertColumns(object s,RoutedEventArgs e)
        {
            InsertColumns();
        }
        protected               void                        _onInsertColumnsNewLine(object s,RoutedEventArgs e)
        {
            InsertColumns(",\n");
        }
        protected   async       void                        _onGotoDeclaration(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReady((project) => {
                        var entity = project.GlobalCatalog.GetEntity(EntityType, EntityName);
                        if (entity.Declaration != null)
                            VSPackage.NavigateTo(null, project.GetDocumentSpan(entity.Declaration));
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("GotoDeclaration failed.", err));
            }
        }
        protected   async       void                        _onFindAllReferences(object s,RoutedEventArgs e)
        {
            try {
                await ItemProject.WhenReady((project) => {
                        project.Service.Library.SearchReferences(VSPackage.ServiceProvider, ItemProject.VSProject, project.FindReferences(project.GlobalCatalog.GetEntity(EntityType, EntityName)));
                    });
            }
            catch(Exception err) {
                VSPackage.DisplayError(new Exception("OnFindAllReferences", err));
            }
        }

        private                 void                        _setChildrenNotLoaded(LTTS_DataModel.Entity entity)
        {
            if ((entity.EntityFlags & LTTS_DataModel.EntityFlags.PartialLoaded) != 0 || _entityHasData(entity)) {
                base.Items.Add(new TreeViewItem()
                                    {
                                        Style  = ItemProject.Control.GetStyle("TreeViewItemStyle"),
                                        Header = "Loading..."
                                    });
            }
        }
        private                 T                           _getChild<T>()
        {
            foreach(object c in this.Items) {
                if (c.GetType() == typeof(T))
                    return (T)c;
            }

            return default(T);
        }

        private     static      LTTS_DataModel.IColumnList  _getColumns(LTTS_DataModel.Entity entity)
        {
            if (entity is LTTS_DataModel.EntityObjectCode  entityCode &&
                entityCode.Returns != null &&
                (entityCode.Returns.TypeFlags & LTTS_DataModel.SqlTypeFlags.Table) != 0)
                return entityCode.Returns.Columns;

            if (entity is LTTS_DataModel.EntityObjectTable entityTable)
                return entityTable.Columns;

            if (entity is LTTS_DataModel.EntityTypeTable   entityTypeTable)
                return entityTypeTable.Columns;

            return null;
        }
        private     static      bool                        _entityHasData(LTTS_DataModel.Entity entity)
        {
            if (entity is LTTS_DataModel.EntityObjectTable entityTable)
                return true;

            if (entity is LTTS_DataModel.EntityObjectCode  entityCode) {
                return (entityCode.Parameters != null && entityCode.Parameters.Count > 0) ||
                       (entityCode.Returns != null                                  );
            }
            if (entity is LTTS_DataModel.EntityType        entityType) {
                return ((entityType.TypeFlags & LTTS_DataModel.SqlTypeFlags.Values) != 0 && entityType.Values != null && entityType.Values.Count > 0) ||
                       ((entityType.TypeFlags & LTTS_DataModel.SqlTypeFlags.Table) != 0);
            }

            return false;
        }
    }

    public class ItemObjectEqualityComparer: IEqualityComparer<ItemEntity>
    {
        public  bool    Equals(ItemEntity o1, ItemEntity o2)
        {
            return object.Equals(o1, o2);
        }
        public  int     GetHashCode(ItemEntity o)
        {
            return ((int)o.EntityType) ^ o.EntityName.GetHashCode();
        }
    }
}
