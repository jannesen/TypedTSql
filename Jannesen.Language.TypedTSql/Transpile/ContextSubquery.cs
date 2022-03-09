using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextSubquery: ContextParent
    {
        public      override    Node.IDataTarget                    Target                  => null;

        internal                                                    ContextSubquery(Context parent): base(parent)
        {
        }

        public      override    DataModel.RowSet                    FindRowSet(string name)
        {
            return _parent.FindRowSet(name);
        }
        public      override    DataModel.Column                    FindColumn(string name, out bool ambiguous)
        {
            return _parent.FindColumn(name, out ambiguous);
        }
    }
}
