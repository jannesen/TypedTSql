using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeTable: SqlType
    {
        public      override    SqlTypeFlags            TypeFlags           { get { return SqlTypeFlags.Table;   } }

        public      override    IColumnList             Columns             { get { return _columns; } }
        public      override    IndexList               Indexes             { get { return _indexes; } }

        private                 ColumnList              _columns;
        private                 IndexList               _indexes;

        public                                          SqlTypeTable(ISymbol parent, ColumnList columns, IndexList indexes)
        {
            _columns = columns;
            _indexes = indexes;
            SetParent(parent);
        }
        public                                          SqlTypeTable(Entity parent, GlobalCatalog catalog, SqlDataReader dataReader)
        {
            _columns = ColumnList.ReadFromDatabase(catalog, parent.EntityName.Database, dataReader);
            _indexes = IndexList.ReadFromDatabase(_columns, dataReader);
        }

        internal                void                    SetParent(ISymbol parent)
        {
            if (_columns != null) {
                foreach(var column in _columns)
                    (column as ColumnDS)?.SetParent(parent);
            }
            if (_indexes != null) {
                foreach(var index in _indexes)
                    index.SetParent(parent);
            }
        }

        private                 void                    _testTranspiled()
        {
            if (_columns == null)
                throw new NeedsTranspileException();
        }
    }
}
