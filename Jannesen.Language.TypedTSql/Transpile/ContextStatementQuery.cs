using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextStatementQuery: ContextParent
    {
        public      override    Node.IDataTarget                    Target                  { get { return _target;         } }
        public      override    DataModel.QueryOptions              QueryOptions            { get { return _queryOptions;   } }

        private                 Node.IDataTarget                    _target;
        private                 DataModel.QueryOptions              _queryOptions;

        public                                                      ContextStatementQuery(Context parent): base(parent)
        {
            _queryOptions = parent.QueryOptions;
        }

        public      override    void                                SetQueryOptions(DataModel.QueryOptions options)
        {
            _queryOptions = options;
        }
        public      override    void                                SetTarget(Node.IDataTarget target)
        {
            if (_target != null) {
                throw new Exception("Target already defined.");
            }

            _target = target;
        }
    }
}
