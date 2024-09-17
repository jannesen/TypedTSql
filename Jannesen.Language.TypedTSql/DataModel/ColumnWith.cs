using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnWith: Column, ISymbol
    {
        public      override    ISymbol                 Symbol                  => this;
        public                  SymbolType              Type                    => SymbolType.Column;
        public      override    string                  Name                    => _name;
        public      override    object                  Declaration             => _declaration;
        public      override    ValueFlags              ValueFlags              => ValueFlags.Nullable|ValueFlags.Column;
        public      override    ISqlType                SqlType                 => _sqlType;
        public      override    string                  CollationName           => null;
        public                  bool                    isUsed                  { get; private set; }

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
