using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnListErrorStub: ColumnListDynamic
    {
        public      override        RowSetFlags             RowSetFlags => RowSetFlags.ErrorStub;
    }
}
