using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeAny: SqlType
    {
        public                                          SqlTypeAny()
        {
        }

        public      override    string                  ToString()
        {
            return "any";
        }
    }
}
