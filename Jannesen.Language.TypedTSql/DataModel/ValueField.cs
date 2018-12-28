using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ValueField
    {
        public      readonly    string              Name;
        public      readonly    object              Value;

        public                                      ValueField(string name, object value)
        {
            this.Name  = name;
            this.Value = value;
        }
    }

    public class ValueFieldList: Library.ListHashName<ValueField>
    {
        public                                      ValueFieldList(int capacity): base(capacity)
        {
        }
        public                                      ValueFieldList(IList<ValueField> list): base(list)
        {
        }

        protected   override    string              ItemKey(ValueField item)
        {
            return item.Name;
        }
    }
}
