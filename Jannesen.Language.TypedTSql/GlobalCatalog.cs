using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.Language.TypedTSql
{
    public class GlobalCatalog
    {
        public                  SqlDatabase                                     Database                    { get; private set; }

        private                 string                                          _defaultCollation;
        private                 DatabaseSchemaList                              _schemas;
        private                 EntityList<EntityAssembly>                      _assemblies;
        private                 EntityList<EntityType>                          _types;
        private                 EntityList<EntityObject>                        _objects;
        private                 DatabasePrincipalList                           _principals;
        private                 CursorList                                      _globalCursors;

        public                  string                                          DefaultCollation
        {
            get {
                return _defaultCollation;
            }
        }
        public                  DatabaseSchema[]                                Schemas
        {
            get {
                return _schemas.ToArray();
            }
        }
        public                  Entity[]                                        Entities
        {
            get {
                var     entities = new List<Entity>(0x4000);

                entities.AddRange(_assemblies);
                entities.AddRange(_types);
                entities.AddRange(_objects);

                return entities.ToArray();
            }
        }
        public                  EntityAssembly[]                                Assemblies
        {
            get {
                return _assemblies.ToArray();
            }
        }
        public                  EntityType[]                                    Usertypes
        {
            get {
                return _types.ToArray();
            }
        }
        public                  EntityObject[]                                  Objects
        {
            get {
                return _objects.ToArray();
            }
        }
        public                  DatabasePrincipal[]                             DatabasePrincipals
        {
            get {
                return _principals.ToArray();
            }
        }

        public                                                                  GlobalCatalog(string databaseName)
        {
            Database = new SqlDatabase(databaseName);

            try {
                _load();
            }
            catch(Exception) {
                Database.Dispose();
                throw;
            }
        }
        public                                                                  GlobalCatalog(SqlDatabase database)
        {
            this.Database = database;
            _load();
        }

        public                  DatabaseSchema                                  GetSchema(string name)
        {
            _schemas.TryGetValue(name, out var rtn);
            return rtn;
        }
        public                  EntityAssembly                                  GetAssembly(EntityName name)
        {
            _assemblies.TryGetValue(name, out var rtn);
            return rtn;
        }
        public                  EntityType                                      GetType(EntityName name, bool loadDatabase=true)
        {
            if (_types.TryGetValue(name, out var rtn)) {
                if ((rtn.EntityFlags & EntityFlags.PartialLoaded) != 0 && loadDatabase)
                    _loadFromDatabase(rtn);
            }

            return rtn;
        }
        public                  EntityObject                                    GetObject(EntityName name, bool loadDatabase=true)
        {
            if (_objects.TryGetValue(name, out var rtn)) {
                if (rtn != null && (rtn.EntityFlags & EntityFlags.PartialLoaded) != 0 && loadDatabase)
                    _loadFromDatabase(rtn);
            }
            else {
                if ((name.Database != null || name.Schema == "sys") && loadDatabase) {
                    rtn = _loadFromDatabase(name);

                    if (rtn != null) {
                        _objects.Add(rtn);
                        _loadFromDatabase(rtn);
                    }
                }
            }

            return rtn;
        }
        public                  Entity                                          GetEntity(SymbolType type, EntityName name, bool loadDatabase=true)
        {
            switch(type) {
            case SymbolType.Assembly:
                {
                    var rtn = GetAssembly(name);
                    if (rtn == null || rtn.Type != type)
                        throw new GlobalCatalogException("Unknown assembly '" + name + "'.");
                    return rtn;
                }

            case SymbolType.TypeUser:
            case SymbolType.TypeExternal:
            case SymbolType.TypeTable:
                {
                    var rtn = GetType(name, loadDatabase);
                    if (rtn == null || rtn.Type != type)
                        throw new GlobalCatalogException("Unknown type '" + name + "'.");
                    return rtn;
                }

            default:
                {
                    var rtn = GetObject(name, loadDatabase);
                    if (rtn == null || rtn.Type != type)
                        throw new GlobalCatalogException("Unknown object '" + name + "'.");
                    return rtn;
                }
            }
        }
        public                  DatabasePrincipal                               GetPrincipal(string name)
        {
            _principals.TryGetValue(name, out var rtn);
            return rtn;
        }
        public                  CursorList                                      GetGlobalCursorList()
        {
            if (_globalCursors == null) {
                _globalCursors = new CursorList(64);
            }

            return _globalCursors;
        }

        internal                EntityAssembly                                  DefineAssembly(EntityName name)
        {
            if (_assemblies.TryGetValue(name, out var entityAssembly)) {
                if ((entityAssembly.EntityFlags & EntityFlags.SourceDeclaration) != 0)
                    return null;
            }
            else
                _assemblies.Add(entityAssembly = new EntityAssembly(SymbolType.Assembly, name, EntityFlags.SourceDeclaration));

            return entityAssembly;
        }
        internal                EntityTypeUser                                  DefineTypeUser(EntityName name)
        {
            if (_types.TryGetValue(name, out var entityType)) {
                if ((entityType.EntityFlags & EntityFlags.SourceDeclaration) != 0)
                    return null;

                if (entityType.Type != SymbolType.TypeUser)
                    throw new InvalidOperationException("Not allowed to change the type from " + entityType.Type + " to TypeUser.");
            }
            else
                _types.Add(entityType = new EntityTypeUser(name, EntityFlags.SourceDeclaration));

            return (EntityTypeUser)entityType;
        }
        internal                EntityTypeTable                                 DefineTypeTable(EntityName name)
        {
            if (_types.TryGetValue(name, out var entityType)) {
                if ((entityType.EntityFlags & EntityFlags.SourceDeclaration) != 0)
                    return null;

                if (entityType.Type != SymbolType.TypeTable)
                    throw new InvalidOperationException("Not allowed to change the type from " + entityType.Type + " to TypeTable.");
            }
            else
                _types.Add(entityType = new EntityTypeTable(name, EntityFlags.SourceDeclaration));

            return (EntityTypeTable)entityType;
        }
        internal                EntityTypeExternal                              DefineTypeExternal(EntityName name)
        {
            if (_types.TryGetValue(name, out var entityType)) {
                if ((entityType.EntityFlags & EntityFlags.SourceDeclaration) != 0)
                    return null;

                if (entityType.Type != SymbolType.TypeExternal)
                    throw new InvalidOperationException("Not allowed to change the type from " + entityType.Type + " to TypeExternal.");
            }
            else
                _types.Add(entityType = new EntityTypeExternal(name, EntityFlags.SourceDeclaration));

            return (EntityTypeExternal)entityType;
        }
        internal                EntityObjectCode                                DefineObjectCode(SymbolType type, EntityName name)
        {
            if (_objects.TryGetValue(name, out var entityObject)) {
                if ((entityObject.EntityFlags & EntityFlags.SourceDeclaration) != 0)
                    return null;

                if (entityObject.Type != type) {
                    if ((type == SymbolType.ServiceMethod      && entityObject.Type == SymbolType.StoredProcedure) ||
                        (type == SymbolType.ServiceComplexType && entityObject.Type == SymbolType.FunctionInlineTable))
                    {
                        _objects.Update(entityObject = new EntityObjectCode(type, name, EntityFlags.SourceDeclaration));
                    }
                    else
                        throw new InvalidOperationException("Not allowed to change the type from " + entityObject.Type + " to " + type + ".");
                }
            }
            else
                _objects.Add(entityObject = new EntityObjectCode(type, name, EntityFlags.SourceDeclaration));

            return (EntityObjectCode)entityObject;
        }

        public                  void                                            BeforeTranspile()
        {
            _objects.BeforeTranspile();
            _types.BeforeTranspile();
            _assemblies.BeforeTranspile();
            _globalCursors = null;
        }
        public                  void                                            CleanupTranspile()
        {
            _objects.CleanupTranspile();
            _types.CleanupTranspile();
            _assemblies.CleanupTranspile();
        }
        internal                ISqlType                                        GetSqlType(string database, SqlDataReader dataReader, int coloffset)
        {
            if (dataReader.IsDBNull(coloffset + 0) && !dataReader.GetBoolean(coloffset + 2) && dataReader.IsDBNull(coloffset + 3))
                return SqlTypeNative.ReadFromDatabase(dataReader, coloffset + 5);

            var         name = new DataModel.EntityName(database, dataReader.GetString(coloffset + 0), dataReader.GetString(coloffset + 1));

            if (!_types.TryGetValue(name, out EntityType rtn)) {
                 rtn = EntityType.NewEntityType(this, dataReader, coloffset);
                _types.Add(rtn);
            }

            return rtn;
        }

        private                 void                                            _load()
        {
            this._schemas    = new DatabaseSchemaList(16);
            this._assemblies = new EntityList<EntityAssembly>(16);
            this._types      = new EntityList<EntityType>(1024);
            this._objects    = new EntityList<EntityObject>(4096);
            this._principals = new DatabasePrincipalList(64);

            lock(Database)
            {
                try {
                    using (SqlDataReader dataReader = Database.ExecuteDataReader("SELECT DATABASEPROPERTYEX(DB_NAME(), 'Collation')\n"+
                                                                                 DatabaseSchema.SqlStatementCatalog + "\n" +
                                                                                 DatabasePrincipal.SqlStatementCatalog + "\n" +
                                                                                 EntityAssembly.SqlStatementCatalog    + "\n" +
                                                                                 EntityType.SqlStatementCatalog        + "\n" +
                                                                                 EntityObject.SqlStatementCatalog))
                    {
                        if (!dataReader.Read())
                            throw new GlobalCatalogException("Failed to read database options.");

                        _defaultCollation = dataReader.GetString(0);

                        if (!dataReader.NextResult())
                            throw new GlobalCatalogException("Missing principal dataset.");

                        while (dataReader.Read())
                            _schemas.TryAdd(new DatabaseSchema(dataReader));

                        if (!dataReader.NextResult())
                            throw new GlobalCatalogException("Missing principal dataset.");

                        while (dataReader.Read())
                            _principals.TryAdd(new DatabasePrincipal(dataReader));

                        if (!dataReader.NextResult())
                            throw new GlobalCatalogException("Missing Assembly dataset.");

                        while (dataReader.Read())
                            _assemblies.Add(new EntityAssembly(dataReader));

                        if (!dataReader.NextResult())
                            throw new GlobalCatalogException("Missing type dataset.");

                        while (dataReader.Read())
                            _types.Add(EntityType.NewEntityType(this, dataReader, 0));

                        if (!dataReader.NextResult())
                            throw new GlobalCatalogException("Missing object dataset.");

                        while (dataReader.Read())
                            _objects.Add(EntityObject.ReadFromDatabase(null, dataReader));
                    }

                    foreach(var obj in _objects) {
                        if (obj.Type == SymbolType.TableUser)
                            _loadFromDatabase(obj);
                    }
                }
                catch(Exception err) {
                    throw new CatalogCacheException("Failed to get catalog from database.", err);
                }
            }
        }
        private                 EntityObject                                    _loadFromDatabase(EntityName name)
        {
            try {
                lock(Database)
                {
                    using (var dataReader = Database.ExecuteDataReader(EntityObject.SqlStatementByName(name)))
                    {
                        if (dataReader.Read()) {
                            return EntityObject.ReadFromDatabase(name.Database, dataReader);
                        }
                    }
                }
                return null;
            }
            catch(Exception err) {
                throw new CatalogCacheException("Failed to get object '" + name + "' from database.", err);
            }
        }
        private                 void                                            _loadFromDatabase(Entity entity)
        {
            try {
                lock(Database)
                {
                    using (SqlDataReader dataReader = Database.ExecuteDataReader(entity.DatabaseReadFromCmd()))
                        entity.DatabaseReadFromResult(this, dataReader);
                }
            }
            catch(Exception err) {
                throw new CatalogCacheException("Failed to get object '" + entity.EntityName + "' from database.", err);
            }
        }
    }
}
