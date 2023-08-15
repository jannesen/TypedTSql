using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityTypeExtend: EntityType
    {
        public  override        SqlTypeFlags            TypeFlags           => ParentType.TypeFlags;
        public  override        SqlTypeNative           NativeType          => ParentType.NativeType;
        public  override        object                  DefaultValue        => ParentType.DefaultValue;
        public  override        InterfaceList           Interfaces          => ParentType.Interfaces;
        public  override        ValueRecordList         Values              => ParentType.Values;
        public  override        IColumnList             Columns             => ParentType.Columns;
        public  override        IndexList               Indexes             => ParentType.Indexes;
        public  override        JsonSchema              JsonSchema          => ParentType.JsonSchema;
        public  override        string                  ToSql()             { return ParentType.ToSql(); }
        public  override        ISqlType                ParentType          { get { testTranspiled(); return _parentType; } }

        private                 EntityType              _parentType;

        internal                                        EntityTypeExtend(DataModel.EntityName name, EntityFlags flags): base(SymbolType.TypeExtend, name, flags)
        {
        }

        internal    override    void                    TranspileBefore()
        {
            _parentType = null;
            base.TranspileBefore();
        }
        internal                void                    Transpiled(EntityType parentType)
        {
            _parentType = parentType;
            Transpiled();
        }
    }
}
