using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextSubquery: ContextParent
    {
        public      readonly    DataModel.RowSetList                IncludeRowsets;

        internal                                                    ContextSubquery(Context parent): base(parent)
        {
            IncludeRowsets = parent.RowSets;
        }
    }
}
