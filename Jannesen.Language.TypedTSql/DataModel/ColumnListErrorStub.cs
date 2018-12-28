using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnListErrorStub: ColumnListDynamic
    {
        public      override        ColumnListFlags                     Flags
        {
            get {
                return ColumnListFlags.ErrorStub;
            }
        }
    }
}
