using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum CursorFlags
    {
        LOCAL               = 0x00001,
        GLOBAL              = 0x00002,
        FORWARD_ONLY        = 0x00010,
        SCROLL              = 0x00020,
        FAST_FORWARD        = 0x00100,
        STATIC              = 0x00200,
        KEYSET              = 0x00400,
        DYNAMIC             = 0x00500,
        READ_ONLY           = 0x01000,
        SCROLL_LOCKS        = 0x02000,
        OPTIMISTIC          = 0x04000,
        TYPE_WARNING        = 0x10000
    }

    public class Cursor: ISymbol
    {
        public                  SymbolType              Type                    { get { return SymbolType.Cursor;    } }
        public                  string                  Name                    { get; private set; }
        public                  object                  Declaration             { get; private set; }
        public                  ISymbol                 Parent                  { get { return null;                 } }
        public                  ISymbol                 SymbolNameReference     { get { return null;                 } }
        public                  CursorFlags             CursorFlags             { get; private set; }
        public                  CursorColumn[]          Columns                 { get; private set; }

        public                                          Cursor(string name, object declaration, CursorFlags cursorFlags, IColumnList columns)
        {
            this.Name        = name;
            this.Declaration = declaration;
            this.CursorFlags = cursorFlags;
            this.Columns     = new CursorColumn[columns.Count];

            for(int i = 0 ; i < columns.Count ; ++i)
                this.Columns[i] = new CursorColumn(columns[i]);
        }
    }

    public class CursorColumn: IExprResult
    {
        public                  ValueFlags              ValueFlags              { get; private set; }
        public                  ISqlType                SqlType                 { get; private set; }
        public                  string                  CollationName           { get; private set; }
        public                  bool                    ValidateConst(ISqlType sqlType)
        {
            return false;
        }

        public                                          CursorColumn(Column colum)
        {
            ValueFlags    = colum.ValueFlags;
            SqlType       = colum.SqlType;
            CollationName = colum.CollationName;
        }
    }

    public class CursorList: Library.ListHashName<Cursor>
    {
        public                                          CursorList(int capacity): base(capacity)
        {
        }

        public                  Cursor                  Define(string name, object declaration, CursorFlags cursorFlags, IColumnList columns)
        {
            if (TryGetValue(name, out var cursor)) {
                if (cursor.CursorFlags != cursorFlags)
                    throw new ErrorException("Redefining cursor with differend cursor options.");

                if (cursor.Columns.Length != columns.Count)
                    throw new ErrorException("Redefining cursor with differend number of columns.");

                for (int i = 0 ; i < cursor.Columns.Length ; ++i) {
                    if (cursor.Columns[i].SqlType != columns[i].SqlType)
                        throw new ErrorException("Redefining cursor with a differend column#" + i + " type.");
                }
            }
            else
                Add(cursor = new Cursor(name, declaration, cursorFlags, columns));

            return cursor;
        }

        protected   override    string                  ItemKey(Cursor item)
        {
            return item.Name;
        }
    }
}
