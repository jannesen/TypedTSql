using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum ColumnListFlags
    {
        None            = 0,
        UniqueNamed     = 0x0001,
        DynamicList     = 0x0002,
        ErrorStub       = 0x0010
    }

    public interface IColumnList: IEnumerable<Column>
    {
        ColumnListFlags                                 Flags           { get; }
        int                                             Count           { get; }
        Column                                          this[int idx]   { get; }

        Column                                          FindColumn(string name, out bool ambiguous);
        ColumnList                                      GetUniqueNamedList();
    }

    public class ColumnList: Library.ListHashName<Column>, IColumnList
    {
        public                  ColumnListFlags         Flags
        {
            get {
                return ColumnListFlags.UniqueNamed;
            }
        }

        public                                          ColumnList(int capacity): base(capacity)
        {
        }
        public                                          ColumnList(IList<Column> list): base(list)
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
