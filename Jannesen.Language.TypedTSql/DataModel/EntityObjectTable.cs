using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityObjectTable: EntityObject, ITable
    {
        public                  IColumnList             Columns         { get { return _columns; } }
        public                  IndexList               Indexes         { get { return _indexes; } }

        public                  ColumnList              ColumnList       { get { return _columns; } }

        private                 ColumnList              _columns;
        private                 IndexList               _indexes;

        internal                                        EntityObjectTable(SymbolType type, DataModel.EntityName name, EntityFlags flags): base(type, name, flags)
        {
        }

        public      override    string                  DatabaseReadFromCmd()
        {
            return "EXEC " + (EntityName.Database!=null ? EntityName.Database+".":"")+ "sys.sp_executesql " +
                                    Library.SqlStatic.QuoteNString("DECLARE @object_id INT=OBJECT_ID(@objectname)\n" +
                                                                   ColumnDS.SqlStatement + "\n" +
                                                                   IndexColumn.SqlStatement + "\n" +
                                                                   Index.SqlStatement) +
                                ",\n N'@objectname nvarchar(1024)', @objectname="+Library.SqlStatic.QuoteString(EntityName.SchemaName);
        }
        public      override    void                    DatabaseReadFromResult(GlobalCatalog catalog, SqlDataReader dataReader)
        {
            _columns = ColumnList.ReadFromDatabase(catalog, EntityName.Database, dataReader);
            _indexes = IndexList.ReadFromDatabase(_columns, dataReader);
            _setParent();
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
