﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnVarTable: Column
    {
        public      override    string                  Name                    { get { return _name;                } }
        public      override    object                  Declaration             { get { return _declaration;         } }
        public      override    ISymbol                 ParentSymbol            { get { return _parent;              } }
        public      override    ISqlType                SqlType                 { get { return _sqlType;             } }
        public      override    string                  CollationName           { get { return _collationName;       } }
        public      override    ISymbol                 SymbolNameReference     { get { return _nameReference;       } }
        public      override    ValueFlags              ValueFlags              { get { return _flags;               } }

        private                 ISymbol                 _parent;
        private                 string                  _name;
        private                 object                  _declaration;
        private                 ISqlType                _sqlType;
        private                 string                  _collationName;
        private                 ISymbol                 _nameReference;
        private                 ValueFlags              _flags;

        public                                          ColumnVarTable(string name, DataModel.ISqlType sqlType, ISymbol parent, object declaration, string collationName, ISymbol nameReference, ValueFlags flags=ValueFlags.None)
        {
            if (sqlType == null) {
                if ((flags & ValueFlags.Error) == 0)
                    throw new ArgumentNullException(nameof(sqlType));

                sqlType = new SqlTypeAny();
            }

            _parent              = parent;
            _name                = name;
            _sqlType             = sqlType;
            _declaration         = declaration;
            _collationName       = collationName;
            _nameReference       = nameReference;
            _flags               = (flags & ValueFlags.Nullable);
        }
    }
}
