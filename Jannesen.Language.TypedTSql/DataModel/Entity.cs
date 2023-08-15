using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum EntityFlags
    {
        None                =      0,
        SourceDeclaration   = 0x0001,
        SourceDatabase      = 0x0002,
        SourceCache         = 0x0004,
        PartialLoaded       = 0x0008,
        NeedsTranspile      = 0x0100
    }

    public abstract class Entity: ISymbol
    {
        public                  SymbolType              Type                    { get ; private set; }
        public                  EntityName              EntityName              { get ; private set; }
        public                  EntityFlags             EntityFlags             { get ; protected set; }
        public                  object                  Declaration             { get ; protected set; }
        public                  string                  Name                    { get { return EntityName.Name;     } }
        public                  string                  FullName                { get { return EntityName.Fullname; } }
        public                  DataModel.ISymbol       ParentSymbol            { get { return null; } }
        public                  DataModel.ISymbol       SymbolNameReference     { get { return null; } }

        internal                                        Entity(SymbolType type, DataModel.EntityName entityName, EntityFlags flags)
        {
            this.Type        = type;
            this.EntityName  = entityName;
            this.EntityFlags = flags;
        }

        internal    virtual     void                    TranspileBefore()
        {
            if ((EntityFlags & EntityFlags.SourceDeclaration) != 0) {
                EntityFlags = (EntityFlags & ~EntityFlags.SourceDeclaration) | EntityFlags.SourceCache;
            }
        }
        internal                void                    TranspileInit(object location)
        {
            EntityFlags = (EntityFlags & ~(EntityFlags.SourceCache|EntityFlags.PartialLoaded)) | (EntityFlags.SourceDeclaration | EntityFlags.NeedsTranspile);
            Declaration = location;
        }
        internal                void                    Transpiled()
        {
            EntityFlags &= ~EntityFlags.NeedsTranspile;
        }

        public      virtual     void                    DatabaseReadFromResult(GlobalCatalog catalog, SqlDataReader dataReader)
        {
            throw new InvalidOperationException(this.GetType().Name + ": no data to reade from database");
        }
        public      virtual     string                  DatabaseReadFromCmd()
        {
            throw new InvalidOperationException(this.GetType().Name + ": no data to reade from database");
        }

        public                  void                    testTranspiled()
        {
            if ((EntityFlags & EntityFlags.NeedsTranspile) != 0)
                throw new NeedsTranspileException();
        }

        public      override    string                  ToString()
        {
            return EntityName.Fullname;
        }
    }

    public class EntityList<T>: Library.ListHash<T, DataModel.EntityName> where T:Entity
    {
        public                                          EntityList(int capacity): base(capacity)
        {
        }
        public                                          EntityList(IReadOnlyList<T> list): base(list)
        {
        }

        public                  void                    BeforeTranspile()
        {
            foreach(var entity in this)
                entity.TranspileBefore();
        }
        public                  void                    CleanupTranspile()
        {
            RemoveWhen((entity) => (entity.EntityFlags & (EntityFlags.SourceDeclaration | EntityFlags.SourceDatabase | EntityFlags.SourceCache)) == EntityFlags.SourceCache);
        }

        protected   override    DataModel.EntityName    ItemKey(T item)
        {
            return item.EntityName;
        }
    }
}
