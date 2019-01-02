using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class RowSet: ISymbol, ISqlType
    {
        public                  SymbolType                  Type                    { get { return DataModel.SymbolType.RowsetAlias;   } }
        public                  string                      Name                    { get; private set; }
        public                  IColumnList                 Columns                 { get; private set; }
        public                  object                      Declaration             { get; private set; }
        public                  ISymbol                     Parent                  { get { return null; } }
        public                  ISymbol                     SymbolNameReference     { get { return null; } }
        public                  ISymbol                     Source                  { get; private set; }

        public                                              RowSet(string name, IColumnList columns, object declaration = null, DataModel.ISymbol source=null)
        {
            if (columns == null)
                columns = new DataModel.ColumnListErrorStub();

            Name        = name;
            Columns     = columns;
            Declaration = declaration;
            Source      = source;
        }

        public                  SqlTypeFlags                TypeFlags       { get { return SqlTypeFlags.RowSet;   } }
        public                  SqlTypeNative               NativeType      { get { throw new InvalidOperationException(this.GetType().Name + ": has no nativetype.");      } }
        public                  InterfaceList               Interfaces      { get { throw new InvalidOperationException(this.GetType().Name + ": has no interfaces.");      } }
        public                  object                      DefaultValue    { get { return null; } }
        public                  ValueRecordList             Values          { get { throw new InvalidOperationException(this.GetType().Name + ": has no values.");          } }
        public                  IndexList                   Indexes         { get { throw new InvalidOperationException(this.GetType().Name + ": has no indexes.");         } }
        public      virtual     JsonSchema                  JsonSchema      { get { throw new InvalidOperationException(this.GetType().Name + ": has no json-schema.");     } }
        public                  Entity                      Entity          { get { return null;                                                                            } }
        public                  string                      ToSql()
        {
            throw new InvalidOperationException("Can't get sql-type of " + this.GetType().Name + ".");
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
        public                      DataModel.Column        FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;

            Column      foundColumn = null;

            foreach(var r in this) {
                if ((r.Columns.Flags & ColumnListFlags.ErrorStub) == 0) {
                    var     c = r.Columns.FindColumn(name, out bool a);

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
                    if ((r.Columns.Flags & ColumnListFlags.ErrorStub) != 0) {
                        var     c = r.Columns.FindColumn(name, out bool a);

                        if (c != null)
                            return c;
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
