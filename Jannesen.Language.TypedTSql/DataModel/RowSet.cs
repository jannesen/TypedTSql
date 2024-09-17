using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum RowSetFlags
    {
        None            = 0,
        Alias           = 0x0001,
        Target          = 0x0002,
        Nullable        = 0x0004,
        DynamicList     = 0x0100,
        ErrorStub       = 0x0200

    }

    public enum JoinType
    {
        NONE        = 0,
        INNER,
        LEFT_OUTER,
        RIGHT_OUTER,
        FULL_OUTER,
        CROSS_JOIN,
        CROSS_APPLY,
        OUTER_APPLY
    }

    public class RowSet: ISymbol
    {
        public                  SymbolType                  Type                    => DataModel.SymbolType.RowsetAlias;
        public                  string                      Name                    { get; private set; }
        public                  string                      FullName                => SqlStatic.QuoteNameIfNeeded(Name);
        public                  RowSetFlags                 Flags                   { get; private set; }
        public                  object                      Declaration             { get; private set; }
        public                  ISymbol                     ParentSymbol            => null;
        public                  ISymbol                     SymbolNameReference     => null;
        public                  ISymbol                     Source                  { get; private set; }

        public                  bool                        isNullable              => (Flags & RowSetFlags.Nullable) != 0;
        private                 IColumnList                 _columns;

        public                                              RowSet(RowSetFlags flags, IColumnList columns, string name = "", object declaration = null, DataModel.ISymbol source=null)
        {
            _columns    = columns ?? new ColumnListErrorStub();

            Name        = name;
            Flags       = flags | _columns.RowSetFlags;
            Declaration = declaration;
            Source      = source;
        }

        public                  Column                      FindColumn(string name, out bool  ambiguous)
        {
            var c = _columns.FindColumn(name, out ambiguous);

            if (c != null && (Flags & RowSetFlags.Nullable) != 0 && !c.isNullable) {
                c = new ColumnNullable(c);
            }

            return c;
        }
        public                  IEnumerable<Column>         GetColumns()
        {
            for (int i = 0 ; i < _columns.Count ; ++i) {
                var c = _columns[i];

                if ((Flags & RowSetFlags.Nullable) != 0 && !c.isNullable) {
                    c = new ColumnNullable(c);
                }

                yield return c;
            }
        }
    }

    public class RowSetList: Library.ListHashName<RowSet>
    {
        public                                              RowSetList(): base(16)
        {
        }

        public                      RowSet                  FindRowSet(string name)
        {
            TryGetValue(name, out RowSet rtn);

            return rtn;
        }
        public                      Column                  FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;

            Column   foundColumn = null;

            foreach(var r in this) {
                if ((r.Flags & RowSetFlags.ErrorStub) == 0) {
                    var     c = r.FindColumn(name, out bool a);

                    if (c != null) {
                        if (foundColumn == null)
                            foundColumn = c;
                        else
                            ambiguous = true;
                    }

                    if (a)
                        ambiguous = true;
                }
            }

            if (foundColumn == null) {
                foreach(var r in this) {
                    if ((r.Flags & RowSetFlags.ErrorStub) != 0) {
                        var     c = r.FindColumn(name, out bool a);

                        if (c != null) {
                            return c;
                        }
                    }
                }
            }

            return foundColumn;
        }

        protected   override        string                  ItemKey(RowSet item)
        {
            return item.Name;
        }
    }
}
