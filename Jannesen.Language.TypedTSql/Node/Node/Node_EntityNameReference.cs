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

    public class Node_EntityNameReference: Core.AstParseNode, ITableSource, IReferencedEntity
    {
        public                  EntityReferenceType             ReferenceType       { get; private set; }
        public      readonly    Core.TokenWithSymbol            n_Schema;
        public      readonly    Core.TokenWithSymbol            n_Name;
        public                  DataModel.EntityName            n_EntityName        { get; private set; }
        public                  DataModel.ISymbol               Entity              { get; private set; }

        private                 string                          _addSchema;

        public                                                  Node_EntityNameReference(Core.ParserReader reader, EntityReferenceType type)
        {
            this.ReferenceType = type;

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
        public                  DataModel.ISymbol               getDataSource()
        {
            return Entity;
        }
        public                  DataModel.IColumnList           getColumnList(Transpile.Context context)
        {
            if (Entity != null) {
                if (Entity is DataModel.ITable entityTable) {
                    return entityTable.Columns;
                }
                else
                if (Entity is DataModel.EntityObjectCode entityCode) {
                    if (entityCode.Returns != null && (entityCode.Returns.TypeFlags & DataModel.SqlTypeFlags.Table) != 0)
                        return entityCode.Returns.Columns;
                    else
                        context.AddError(this, " is not a table,view or table-function.");
                }
                else
                    context.AddError(this, " is not a database object.");
            }

            return new DataModel.ColumnListErrorStub();
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            Entity = null;

            Validate.Schema(context, n_Schema);

            switch (ReferenceType) {
            case EntityReferenceType.Table:
            case EntityReferenceType.TableOrView:
            case EntityReferenceType.StoredProcedure:
            case EntityReferenceType.FunctionTable:
                if (n_EntityName.Schema != null) {
                    var entity = context.Catalog.GetObject(n_EntityName);
                    if (entity == null) {
                        context.AddError(this, "Unknown object '" + n_EntityName + "'.");
                        return;
                    }

                    _validateReference(entity.Type, ReferenceType);
                    n_Name.SetSymbol(entity);

                    if (n_Schema != null)
                        context.CaseWarning(n_Schema, entity.EntityName.Schema);

                    context.CaseWarning(n_Name,   entity.EntityName.Name);
                    Entity = entity;
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
                    Entity = tempTable;
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
                    Entity = entity;
                }
                break;

            case EntityReferenceType.FromReference:
                break;

            case EntityReferenceType.Queue:
                Core.TokenWithSymbol.SetNoSymbol(n_Name);
                return;

            default:
                throw new NotImplementedException("Can't transpile Node_EntityNameReference:" + ReferenceType);
            }

            n_Name.SetSymbol(Entity);
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
