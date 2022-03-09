using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum EntityReferenceType
    {
        Unknown,
        Object,
        Table,
        TableOrView,
        StoredProcedure,
        FunctionTable,
        FromReference,
        UserDataType,
        Queue
    }

    public class Node_EntityNameReference: Core.AstParseNode, IDataTarget, IReferencedEntity
    {
        public                  EntityReferenceType             ReferenceType       { get; private set; }
        public                  DataModel.SymbolUsageFlags      Usage               { get; private set; }
        public      readonly    Core.TokenWithSymbol            n_Schema;
        public      readonly    Core.TokenWithSymbol            n_Name;
        public                  DataModel.EntityName            n_EntityName        { get; private set; }
        public                  DataModel.ISymbol               Entity              { get { return _entity;   } }
        public                  DataModel.IColumnList           Columns             { get { return _columns;  } }

                                bool                            IDataTarget.isVarDeclare    { get { return false;     } }
                                DataModel.ISymbol               IDataTarget.Table           { get { return _entity;   } }
                                
        private                 DataModel.ISymbol               _entity;
        private                 DataModel.IColumnList           _columns;
        private                 string                          _addSchema;

        public                                                  Node_EntityNameReference(Core.ParserReader reader, EntityReferenceType type, DataModel.SymbolUsageFlags usage)
        {
            this.ReferenceType = type;
            this.Usage         = usage;

            string  database = null;
            string  schema   = null;
            string  name     = (n_Name = ParseName(reader)).ValueString;

            if (ParseOptionalToken(reader, Core.TokenID.Dot) != null) {
                n_Schema = n_Name;
                schema = name;
                name   = (n_Name = ParseName(reader)).ValueString;

                if (ParseOptionalToken(reader, Core.TokenID.Dot) != null) {
                    n_Schema = n_Name;
                    database = schema;
                    schema   = name;
                    name     = (n_Name = ParseName(reader)).ValueString;
                }
            }
            else {
                if (type != EntityReferenceType.Unknown) {
                    schema = _getSchema(name, reader);
                }
            }

            n_EntityName = new DataModel.EntityName(database, schema, name);
        }

        public                  void                            UpdateType(Core.ParserReader reader, EntityReferenceType type)
        {
            this.ReferenceType = type;

            if (type != EntityReferenceType.FromReference && n_EntityName.Schema == null) {
                string schema = _getSchema(n_EntityName.Name, reader);

                if (schema != null)
                    n_EntityName = new DataModel.EntityName(schema, n_EntityName.Name);
            }
        }
        public                  void                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            Usage = usage;
            n_Name.SymbolData?.UpdateSymbolUsage(_entity, usage);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _entity = null;
            _columns = null;

            Validate.Schema(context, n_Schema);

            switch (ReferenceType) {
            case EntityReferenceType.Table:
            case EntityReferenceType.TableOrView:
            case EntityReferenceType.FunctionTable: { 
                    if (n_EntityName.Schema != null) {
                        var entity = context.Catalog.GetObject(n_EntityName);
                        if (entity == null) {
                            context.AddError(this, "Unknown object '" + n_EntityName + "'.");
                            return;
                        }

                        _validateReference(entity.Type, ReferenceType);

                        if (n_Schema != null)
                            context.CaseWarning(n_Schema, entity.EntityName.Schema);

                        context.CaseWarning(n_Name,   entity.EntityName.Name);
                        _entity = entity;
                    }
                    else {
                        var     name = n_Name.ValueString;

                        if (!name.StartsWith("#", StringComparison.Ordinal)) {
                            context.AddError(n_Name, "Missing '#'.");
                            return;
                        }

                        var tempTable = _findTempTable(context, context.GetDeclarationObjectCode().Entity, name, new List<DataModel.EntityObjectCode>());

                        if (tempTable == null) {
                            context.AddError(n_Name, "Unknown temp table '" + name + "'.");
                            return;
                        }

                        context.CaseWarning(n_Name, tempTable.Name);
                        _entity = tempTable;
                    }

                    if (_entity is DataModel.ITable table) {
                        _columns = table.Columns;
                    }
                    else if (_entity is DataModel.EntityObjectCode entityCode) {
                        var returns = entityCode.Returns;

                        if (returns != null && (returns.TypeFlags & DataModel.SqlTypeFlags.Table) != 0) {
                            _columns = returns.Columns;
                        }
                    }

                    if (_columns == null) {
                        context.AddError(this, n_EntityName.Fullname + " is not a table,view or table-function.");
                    }
                }
                break;

            case EntityReferenceType.StoredProcedure: { 
                    var entity = context.Catalog.GetObject(n_EntityName);
                    if (entity == null) {
                        context.AddError(this, "Unknown object '" + n_EntityName + "'.");
                        return;
                    }

                    _validateReference(entity.Type, ReferenceType);

                    if (n_Schema != null)
                        context.CaseWarning(n_Schema, entity.EntityName.Schema);

                    context.CaseWarning(n_Name,   entity.EntityName.Name);
                    _entity = entity;
                }
                break;

            case EntityReferenceType.UserDataType: {
                    var entity = context.Catalog.GetType(n_EntityName);
                    if (entity == null) {
                        context.AddError(this, "Unknown user-type '" + n_EntityName + "'.");
                        return;
                    }

                    if (n_Schema != null)
                        context.CaseWarning(n_Schema, entity.EntityName.Schema);

                    context.CaseWarning(n_Name,   entity.EntityName.Name);
                    _entity = entity;
                }
                break;

            case EntityReferenceType.Queue:
                Core.TokenWithSymbol.SetNoSymbol(n_Name);
                return;

            default:
                throw new InvalidOperationException("Can't transpile Node_EntityNameReference:" + ReferenceType);
            }

            if (_entity != null) {
                n_Name.SetSymbolUsage(_entity, Usage);
            }
        }
        public                  void                            TranspileAliasTarget(Transpile.Context context, TableSource from, DataModel.SymbolUsageFlags usage)
        {
            var source = from.FindByName(n_Name.ValueString);
            if (source != null) {
                _entity  = source.t_Source;
                _columns = source.ColumnList;
                n_Name.SetSymbolUsage(source.t_RowSet, DataModel.SymbolUsageFlags.Reference);

                if (!source.SetUsage(DataModel.SymbolUsageFlags.Select | usage)) {
                    context.AddError(this, "alias can't not be used as target.");
                }
            }
            else {
                context.AddError(this, "Unknown rowset alias.");
            }
        }
        
                                DataModel.Column                IDataTarget.GetColumnForAssign(string name, DataModel.ISqlType sqlType, string collationName, DataModel.ValueFlags flags, object declaration, DataModel.ISymbol nameReference, out bool declared)
        {
            declared = false;
            return _columns?.FindColumn(name, out var _);
        }

        public                  DataModel.EntityName            getReferencedEntity(DeclarationObjectCode declarationObjectCode)
        {
            switch(ReferenceType) {
            case EntityReferenceType.Object:
            case EntityReferenceType.Table:
            case EntityReferenceType.TableOrView:
            case EntityReferenceType.StoredProcedure:
            case EntityReferenceType.FunctionTable:
            case EntityReferenceType.Queue:
                return n_EntityName;

             default:
                return null;
            }
        }

        private                 string                          _getSchema(string name, Core.ParserReader reader)
        {
            if (name.StartsWith("#", StringComparison.Ordinal))
                return null;

            switch(ReferenceType) {
            case EntityReferenceType.StoredProcedure:
                if (name.StartsWith("sp_", StringComparison.Ordinal) ||
                    name.StartsWith("xp_", StringComparison.Ordinal))
                    return "sys";
                break;
            }

            string schema = reader.Options.Schema;
            if (schema == null)
                throw new ParseException(n_Name, "Schema not defined.");

            return _addSchema = schema;
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (object.ReferenceEquals(node, n_Name) && _addSchema != null)
                    emitWriter.WriteText(Library.SqlStatic.QuoteNameIfNeeded(_addSchema) + ".");

                node.Emit(emitWriter);
            }
        }

        private                 void                            _validateReference(DataModel.SymbolType entityType, EntityReferenceType referenceType)
        {
            switch (referenceType) {
            case EntityReferenceType.Table:
                switch(entityType) {
                case DataModel.SymbolType.TableUser:
                    return;
                }
                break;

            case EntityReferenceType.TableOrView:
                switch(entityType) {
                case DataModel.SymbolType.TableInternal:
                case DataModel.SymbolType.TableSystem:
                case DataModel.SymbolType.TableUser:
                case DataModel.SymbolType.View:
                    return;
                }
                break;

            case EntityReferenceType.StoredProcedure:
                switch(entityType) {
                case DataModel.SymbolType.StoredProcedure:
                case DataModel.SymbolType.StoredProcedure_clr:
                case DataModel.SymbolType.StoredProcedure_extended:
                case DataModel.SymbolType.ServiceMethod:
                    return;
                }
                break;

            case EntityReferenceType.FunctionTable:
                switch(entityType) {
                case DataModel.SymbolType.FunctionInlineTable:
                case DataModel.SymbolType.FunctionMultistatementTable:
                case DataModel.SymbolType.FunctionMultistatementTable_clr:
                    return;
                }
                break;

            case EntityReferenceType.UserDataType:
                switch(entityType) {
                case DataModel.SymbolType.TypeUser:
                case DataModel.SymbolType.TypeExternal:
                case DataModel.SymbolType.TypeTable:
                    return;
                }
                break;
            }

            throw new TranspileException(this, "Invalid object type.");
        }
        private                 DataModel.TempTable             _findTempTable(Transpile.Context context, DataModel.EntityObjectCode entity, string name, List<DataModel.EntityObjectCode> path)
        {
            var tempTable = entity.TempTableGet(name);
            if (tempTable != null) {
                return tempTable;
            }

            var calledby = entity.Calledby;
            if (calledby == null) {
                return null;
            }

            path.Add(entity);
            var pathpos = path.Count;
            var tables    = new DataModel.TempTable[calledby.Count];
            var recursive = new bool[calledby.Count];
            var rtn       = -1;

            for (var i = 0 ; i < calledby.Count ; ++i) {
                if (!path.Contains(calledby[i])) {
                    if (path.Count > pathpos) {
                        path.RemoveRange(pathpos, path.Count - pathpos);
                    }
                    var t = _findTempTable(context, calledby[i], name, path);
                    if (t != null) {
                        tables[i] = t;
                        if (rtn == -1) {
                            rtn = i;
                        }
                    }
                }
                else {
                    recursive[i] = true;
                }
            }

            for (var i = 0 ; i < calledby.Count ; ++i) {
                if (!recursive[i] && tables[i] == null) {
                    if (!context.ReportNeedTranspile && (calledby[i].EntityFlags & DataModel.EntityFlags.NeedsTranspile) != 0) {
                        throw new NeedsTranspileException();
                    }

                    context.AddError(n_Name, "Temp table '" + name + "' not defined in '" + calledby[i].EntityName.ToString() + "'.");
                }
            }

            if (rtn >= 0) {
                for (var i = 0 ; i < calledby.Count ; ++i) {
                    if (i != rtn) { 
                        var t = tables[i];
                        if (t != null) {
                            var columns1 = tables[rtn].Columns;
                            var columns2 = t.Columns;

                            if (columns1.Count == columns2.Count) {
                                for (int j = 0 ; j < columns1.Count ; ++j) {
                                    var col1 = columns1[j];
                                    var col2 = columns2[j];

                                    if (!(col1.Name          == col2.Name         &&
                                          col1.Type          == col2.Type         &&
                                          col1.ValueFlags    == col2.ValueFlags   &&
                                          col1.CollationName == col2.CollationName)) {
                                        context.AddError(n_Name, "Temp table column '" + col1.Name + "' defined in '" + calledby[rtn].EntityName.ToString() + "' and '" + calledby[rtn].EntityName.ToString() + "' are not equal.");
                                    }
                                }
                            }
                            else {
                                context.AddError(n_Name, "Temp table defined in '" + calledby[rtn].EntityName.ToString() + "' and '" + calledby[rtn].EntityName.ToString() + "' has a different number of columns.");
                            }
                        }
                    }
                }
            }

            return rtn >= 0 ? tables[rtn] : null;
        }
    }
}
