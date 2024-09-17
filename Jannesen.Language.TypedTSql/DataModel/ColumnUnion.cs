using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnUnion: Column, ISymbol
    {
        public      override    ISymbol                 Symbol                  => this;
        public                  SymbolType              Type                    => SymbolType.Column;
        public      override    string                  Name                    => _name;
        public      override    object                  Declaration             => _declaration;
        public      override    ISymbol                 SymbolNameReference     => _symbolNameReference;
        public      override    ISqlType                SqlType                 => _sqlType;
        public      override    string                  CollationName           => _collationName;
        public      override    ValueFlags              ValueFlags              => _flags;
        public                  Node.IExprNode[]        Exprs                   => _exprs;

        private                 string                  _name;
        private                 object                  _declaration;
        private                 ISymbol                 _symbolNameReference;
        private                 ISqlType                _sqlType;
        private                 string                  _collationName;
        private                 ValueFlags              _flags;
        private                 Node.IExprNode[]        _exprs;

        public                                          ColumnUnion(string name, Node.IExprNode[] exprs, Logic.FlagsTypeCollation flagsTypeCollation, object declaration=null, ISymbol nameReference=null)
        {
            _name                = name;
            _exprs               = exprs;
            _declaration         = declaration;
            _symbolNameReference = nameReference;
            _sqlType             = flagsTypeCollation.SqlType;
            _collationName       = flagsTypeCollation.CollationName;
            _flags               = flagsTypeCollation.ValueFlags;
        }

        public      override    bool                    ValidateConst(ISqlType sqlType)
        {
            for (int i = 0 ; i < _exprs.Length ; ++i) {
                if (!_exprs[i].ValidateConst(sqlType))
                    return false;
            }

            return true;
        }
    }
}
