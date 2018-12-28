using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnListDynamic: IColumnList
    {
        public      virtual         ColumnListFlags                     Flags
        {
            get {
                return ColumnListFlags.DynamicList;
            }
        }
        public                      int                                 Count
        {
            get {
                return _columns.Count;
            }
        }
        public                      Column                              this[int idx]
        {
            get {
                return _columns[idx];
            }
        }

        private                     ColumnList                          _columns;

        public                                                          ColumnListDynamic()
        {
            _columns = new ColumnList(16);
        }

        public                      Column                              FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;

            if (!_columns.TryGetValue(name, out Column column))
                _columns.Add(column = new ColumnDS(name,
                                                   new DataModel.SqlTypeAny(),
                                                   flags:DataModel.ValueFlags.Nullable));

            return column;
        }
        public                      ColumnList                          GetUniqueNamedList()
        {
            throw new InvalidOperationException("Can't convert ColumnListAny to NamedList.");
        }

        public                      System.Collections.IEnumerator      GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
                                    IEnumerator<Column>                 IEnumerable<Column>.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
    }
}
