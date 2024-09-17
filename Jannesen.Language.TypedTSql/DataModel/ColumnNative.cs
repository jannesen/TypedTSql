using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnNative: Column, ISymbol
    {
        public      override    ISymbol                 Symbol                          => this;
        public                  SymbolType              Type                            => SymbolType.Column;
        public      override    string                  Name                            => _name;
        public      override    object                  Declaration                     => null;
        public      override    ValueFlags              ValueFlags                      => _valueFlags;
        public      override    ISqlType                SqlType                         => _sqlType;
        public      override    string                  CollationName                   => _collationName;

        private                 string                  _name;
        private                 ISqlType                _sqlType;
        private                 string                  _collationName;
        private                 ValueFlags              _valueFlags;

        public                                          ColumnNative(string name, ISqlType sqlType, string collationName=null, bool nullable=true)
        {
            _name          = name;
            _sqlType       = sqlType;
            _collationName = collationName;
            _valueFlags    = nullable ? ValueFlags.Nullable|ValueFlags.Column : ValueFlags.Column;
        }
    }
}
