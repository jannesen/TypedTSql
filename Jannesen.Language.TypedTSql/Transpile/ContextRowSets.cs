using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextRowSets: ContextParent
    {
        public      override    DataModel.RowSetList                RowSets                 { get { return _rowsets;       } }

        private                 DataModel.RowSetList                _rowsets;

        internal                                                    ContextRowSets(Context parent): base(parent)
        {
            _rowsets = new DataModel.RowSetList();
        }
        internal                                                    ContextRowSets(Context parent, DataModel.IColumnList columnList): base(parent)
        {
            _rowsets  = new DataModel.RowSetList { { new DataModel.RowSet("", columnList) } };
        }

        public      override    DataModel.RowSet                    FindRowSet(string name)
        {
            var found = _rowsets.FindRowSet(name);
            if (found != null) {
                return found;
            }

            return _parent.FindRowSet(name);
        }
        public      override    DataModel.Column                    FindColumn(string name, out bool ambiguous)
        {
            var found = _rowsets.FindColumn(name, out ambiguous);
            if (found != null) {
                return found;
            }

            return _parent.FindColumn(name, out ambiguous);
        }

        public      override    void                                SetTarget(Node.IDataTarget target)
        {
            _parent.SetTarget(target);
        }
    }
}
