using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeVoid: SqlType
    {
        public                                          SqlTypeVoid()
        {
        }

        public      override    string                  ToString()
        {
            return "void";
        }
    }
}
