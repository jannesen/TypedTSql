using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnWith: Column
    {
        public      override    string                  Name                            { get { return _name;                                  } }
        public      override    object                  Declaration                     { get { return _declaration;                           } }
        public      override    ValueFlags              ValueFlags                      { get { return ValueFlags.Nullable|ValueFlags.Column;  } }
        public      override    ISqlType                SqlType                         { get { return _sqlType;                               } }
        public      override    string                  CollationName                   { get { return null;                                   } }
        public                  bool                    isUsed                          { get; private set; }

        private                 string                  _name;
        private                 object                  _declaration;
        private                 ISqlType                _sqlType;

        public                                          ColumnWith(string name, object declaration, ISqlType sqlType)
        {
            _name        = name;
            _declaration = declaration;
            _sqlType     = sqlType;
        }

        public      override    void                    SetUsed()
        {
            isUsed = true;
        }
    }
}
