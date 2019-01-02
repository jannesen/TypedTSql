using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextRowSets: ContextParent
    {
        public      override    DataModel.RowSetList                RowSets                 { get { return _rowsets;       } }
        public      override    bool                                RowSetPublic            { get { return _public;        } }

        private                 DataModel.RowSetList                _rowsets;
        private                 bool                                _public;

        internal                                                    ContextRowSets(Context parent, bool pub): base(parent)
        {
            _rowsets = new DataModel.RowSetList();
            _public  = pub;
        }
        internal                                                    ContextRowSets(Context parent, DataModel.IColumnList columnList): base(parent)
        {
            _rowsets  = new DataModel.RowSetList { { new DataModel.RowSet("", columnList) } };
            _public  = false;
        }
        public                  void                                SetRowsetPublic(bool pub)
        {
            _public = pub;
        }
    }
}
