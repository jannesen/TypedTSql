using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeAny: SqlType
    {
        public      static      SqlTypeAny              Instance = new SqlTypeAny();
        
        public                                          SqlTypeAny()
        {
        }

        public      override    string                  ToString()
        {
            return "any";
        }
    }
}
