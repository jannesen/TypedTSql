using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnNative: Column
    {
        public      override    string                  Name                            { get { return _name;                                  } }
        public      override    object                  Declaration                     { get { return null;                                   } }
        public      override    ValueFlags              ValueFlags                      { get { return ValueFlags.Nullable|ValueFlags.Column;  } }
        public      override    ISqlType                SqlType                         { get { return _sqlType;                               } }
        public      override    string                  CollationName                   { get { return _collationName;                         } }

        private                 string                  _name;
        private                 ISqlType                _sqlType;
        private                 string                  _collationName;

        public                                          ColumnNative(string name, ISqlType sqlType, string collationName=null)
        {
            _name          = name;
            _sqlType       = sqlType;
            _collationName = collationName;
        }
    }
}
