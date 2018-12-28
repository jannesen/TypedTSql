using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityTypeTable: EntityType, ITable
    {
        public      override    SqlTypeFlags            TypeFlags           { get { return SqlTypeFlags.Table;   } }

        public      override    IColumnList             Columns             { get { testTranspiled(); return _columns; } }
        public      override    IndexList               Indexes             { get { testTranspiled(); return _indexes; } }

        private                 ColumnList              _columns;
        private                 IndexList               _indexes;

        internal                                        EntityTypeTable(DataModel.EntityName name, EntityFlags flags): base(SymbolType.TypeTable, name, flags)
        {
        }
        internal                                        EntityTypeTable(GlobalCatalog catalog, DataModel.EntityName entityName, SqlDataReader dataReader, int coloffset): base(SymbolType.TypeTable, entityName, EntityFlags.SourceDatabase)
        {
            this.EntityFlags |= EntityFlags.PartialLoaded;
        }

        internal    new         void                    TranspileInit(object location)
        {
            base.TranspileInit(location);
            _columns = null;
            _indexes = null;
        }
        internal                void                    Transpiled(ColumnList columns, IndexList indexes)
        {
            _columns = columns;
            _indexes = indexes;
            Transpiled();
        }

        public      override    string                  DatabaseReadFromCmd()
        {
            return "EXEC " + (EntityName.Database!=null ? EntityName.Database+".":"")+ "sys.sp_executesql N'" +
                                    "DECLARE @object_id INT=(select [type_table_object_id] FROM sys.table_types WHERE [schema_id]=SCHEMA_ID(@schemaname) AND [name]=@schemaname AND [is_table_type]=1)\n" +
                                    ColumnDS.SqlStatement + "\n" +
                                    IndexColumn.SqlStatement + "\n" +
                                    Index.SqlStatement + "',\n N'@schemaname sysname, @objectname sysname', @schemaname=" + Library.SqlStatic.QuoteString(EntityName.Schema) + ", @objectname=" + Library.SqlStatic.QuoteString(EntityName.Name);
        }
        public      override    void                    DatabaseReadFromResult(GlobalCatalog catalog, SqlDataReader dataReader)
        {
            _columns = ColumnList.ReadFromDatabase(catalog, EntityName.Database, dataReader);
            _indexes = IndexList.ReadFromDatabase(_columns, dataReader);
            EntityFlags &= ~EntityFlags.PartialLoaded;
        }

        private                 void                    _setParent()
        {
            if (_columns != null) {
                foreach(var column in _columns)
                    (column as ColumnDS)?.SetParent(this);
            }

            if (_indexes != null) {
                foreach(var index in _indexes)
                    index.SetParent(this);
            }
        }
    }
}
