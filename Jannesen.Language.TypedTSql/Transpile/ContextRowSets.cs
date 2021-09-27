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
    }
}
