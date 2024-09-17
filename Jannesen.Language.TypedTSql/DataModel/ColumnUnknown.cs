using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnUnknown: Column, ISymbol
    {
        public      override    ISymbol                 Symbol                  => this;
        public                  SymbolType              Type                    => SymbolType.Column;
        public      override    string                  Name                    => _name;
        public      override    object                  Declaration             => _declaration;
        public      override    ValueFlags              ValueFlags              => ValueFlags.Error;
        public      override    ISqlType                SqlType                 => new SqlTypeAny();
        public      override    string                  CollationName           => null;

        private                 string                  _name;
        private                 object                  _declaration;

        public                                          ColumnUnknown(string name, object declaration)
        {
            _name        = name;
            _declaration = declaration;
        }
    }
}
