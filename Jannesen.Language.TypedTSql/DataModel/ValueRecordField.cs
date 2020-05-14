using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ValueRecordField: ISymbol
    {
        public                  SymbolType          Type                    { get { return SymbolType.UDTValueField; } }
        public                  string              Name                    { get ; private set; }
        public                  object              Declaration             { get ; private set; }
        public                  DataModel.ISymbol   Parent                  { get { return null; } }
        public                  DataModel.ISymbol   SymbolNameReference     { get { return null; } }

        public                                      ValueRecordField(string name)
        {
            this.Name  = name;
        }
    }

    public class ValueRecordFieldList: Library.ListHashName<ValueRecordField>
    {
        public                                      ValueRecordFieldList(int capacity): base(capacity)
        {
        }
        public                                      ValueRecordFieldList(IReadOnlyList<ValueRecordField> list): base(list)
        {
        }

        protected   override    string              ItemKey(ValueRecordField item)
        {
            return item.Name;
        }
    }
}
