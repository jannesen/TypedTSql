using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnUnknown: Column
    {
        public      override    string                  Name                    { get { return _name;               } }
        public      override    object                  Declaration             { get { return _declaration;        } }
        public      override    ValueFlags              ValueFlags              { get { return ValueFlags.Error;    } }
        public      override    ISqlType                SqlType                 { get { return new SqlTypeAny();    } }
        public      override    string                  CollationName           { get { return null;                } }

        private                 string                  _name;
        private                 object                  _declaration;

        public                                          ColumnUnknown(string name, object declaration)
        {
            _name        = name;
            _declaration = declaration;
        }
    }
}
