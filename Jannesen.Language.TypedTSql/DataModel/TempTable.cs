using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class TempTable: ITable, ISymbol
    {
        public                  SymbolType              Type                    { get { return SymbolType.TempTable; } }
        public                  string                  Name                    { get; private set; }
        public                  object                  Declaration             { get; private set; }
        public                  ISymbol                 Parent                  { get { return null;                 } }
        public                  ISymbol                 SymbolNameReference     { get { return null;                 } }
        public                  IColumnList             Columns                 { get; private set; }
        public                  IndexList               Indexes                 { get; private set; }


        public                                          TempTable(string name, object declaration, ColumnList columns, IndexList indexes)
        {
            this.Name        = name;
            this.Declaration = declaration;
            this.Columns     = columns;
            this.Indexes     = indexes;
        }
    }

    public class TempTableList: Library.ListHashName<TempTable>
    {
        public                                          TempTableList(int capacity): base(capacity)
        {
        }

        protected   override    string                  ItemKey(TempTable item)
        {
            return item.Name;
        }
    }
}
