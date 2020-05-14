using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ValueRecord: ISymbol
    {
        public                  SymbolType          Type                    { get { return SymbolType.UDTValue; } }
        public                  string              Name                    { get ; private set; }
        public      readonly    object              Value;
        public                  object              Declaration             { get ; private set; }
        public                  DataModel.ISymbol   Parent                  { get { return null; } }
        public                  DataModel.ISymbol   SymbolNameReference     { get { return null; } }
        public      readonly    ValueFieldList      Fields;

        public                                      ValueRecord(string name, object value, object declaration, ValueFieldList fields)
        {
            this.Name        = name;
            this.Value       = value;
            this.Declaration = declaration;
            this.Fields      = fields;
        }
    }

    public class ValueRecordList: Library.ListHashName<ValueRecord>
    {
        public                                      ValueRecordList(int capacity): base(capacity)
        {
        }
        public                                      ValueRecordList(IReadOnlyList<ValueRecord> list): base(list)
        {
        }

        protected   override    string              ItemKey(ValueRecord item)
        {
            return item.Name;
        }
    }

}
