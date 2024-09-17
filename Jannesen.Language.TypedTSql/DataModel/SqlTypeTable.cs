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

        private                 IColumnList             _columns;
        private                 IndexList               _indexes;

        public                                          SqlTypeTable(IColumnList columns, IndexList indexes)
        {
            _columns = columns;
            _indexes = indexes;
        }
        public                                          SqlTypeTable(ISymbol parent, ColumnList columns, IndexList indexes)
        {
            _columns = columns;
            _indexes = indexes;

            if (columns != null) {
                foreach(var column in columns)
                    column.SetParent(parent);
            }
            if (indexes != null) {
                foreach(var index in indexes)
                    index.SetParent(parent);
            }
        }
        public                                          SqlTypeTable(Entity parent, GlobalCatalog catalog, SqlDataReader dataReader)
        {
            var columns = ColumnList.ReadFromDatabase(catalog, parent.EntityName.Database, dataReader);
            _columns = columns;
            _indexes = IndexList.ReadFromDatabase(columns, dataReader);
        }
    }
}
