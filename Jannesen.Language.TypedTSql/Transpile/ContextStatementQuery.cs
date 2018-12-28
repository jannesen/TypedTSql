using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextStatementQuery: ContextParent
    {
        public      override    DataModel.RowSet                    Target                  { get { return _target;         } }
        public      override    DataModel.QueryOptions              QueryOptions            { get { return _queryOptions;   } }

        private                 DataModel.RowSet                    _target;
        private                 DataModel.QueryOptions              _queryOptions;

        public                                                      ContextStatementQuery(Context parent): base(parent)
        {
            _queryOptions = parent.QueryOptions;
        }

        public      override    void                                SetQueryOptions(DataModel.QueryOptions options)
        {
            _queryOptions = options;
        }
        public                  void                                SetTarget(DataModel.RowSet target)
        {
            _target = target;
        }
    }
}
