using System;
using System.Collections;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnListResult: IColumnList
    {
        public                      ColumnListFlags                     Flags
        {
            get {
                return ColumnListFlags.None;
            }
        }
        public                      int                                 Count
        {
            get {
                return _columns.Length;
            }
        }
        public                      Column                              this[int idx]
        {
            get {
                return _columns[idx];
            }
        }

        private                     Column[]                            _columns;

        public                                                          ColumnListResult(Column[] columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            _columns = columns;
        }

        public                      Column                              FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;
            Column  column = null;

            foreach(var c in _columns) {
                if (string.Compare(c.Name, name, StringComparison.OrdinalIgnoreCase) == 0) {
                    if (column == null)
                        column = c;
                    else
                        ambiguous = true;
                }
            }

            return column;
        }
        public                      ColumnList                          GetUniqueNamedList()
        {
            var columnList = new ColumnList(_columns.Length);

            foreach(var c in _columns) {
                if (c.isUnnammed) {
                    if (c.SqlType == null)
                        continue; // Ignore error column.

                    throw new ErrorException("Result has a unnamed column.");
                }

                if (!columnList.TryAdd(c))
                    throw new ErrorException("Column [" + c.Name + "] already defined in result.");
            }

            return columnList;
        }

                                    IEnumerator                         IEnumerable.GetEnumerator()
        {
            return _columns.GetEnumerator();
        }
                                    IEnumerator<Column>                 IEnumerable<Column>.GetEnumerator()
        {
            foreach(var c in _columns)
                yield return c;
        }
    }
}
