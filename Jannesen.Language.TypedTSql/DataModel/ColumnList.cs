using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public interface IColumnList: IReadOnlyList<Column>
    {
        RowSetFlags                                     RowSetFlags     { get; }
        Column                                          FindColumn(string name, out bool ambiguous);
        ColumnList                                      GetUniqueNamedList();
    }

    public class ColumnList: Library.ListHashName<Column>, IColumnList
    {
        public                  RowSetFlags             RowSetFlags             => RowSetFlags.None;

        public                                          ColumnList(int capacity): base(capacity)
        {
        }
        public                                          ColumnList(IReadOnlyList<Column> list): base(list)
        {
        }

        public                  Column                  FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;

            TryGetValue(name, out DataModel.Column column);

            return column;
        }
        public                  ColumnList              GetUniqueNamedList()
        {
            return this;
        }

        public      static      ColumnList              ReadFromDatabase(GlobalCatalog catalog, string database, SqlDataReader dataReader)
        {
            var     columns = new List<Column>();

            while (dataReader.Read())
                columns.Add(new ColumnDS(catalog, database, dataReader));

            if (columns.Count == 0)
                throw new ErrorException("0 Columns.");

            return new ColumnList(columns);
        }
        protected   override    string                  ItemKey(Column item)
        {
            return item.Name;
        }
    }
}
