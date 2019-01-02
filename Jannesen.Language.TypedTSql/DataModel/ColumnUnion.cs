using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnUnion: Column
    {
        public      override    string                  Name                    { get { return _name;                } }
        public      override    object                  Declaration             { get { return _declaration;         } }
        public      override    ISymbol                 SymbolNameReference     { get { return _symbolNameReference; } }
        public      override    ISqlType                SqlType                 { get { return _sqlType;             } }
        public      override    string                  CollationName           { get { return _collationName;       } }
        public      override    ValueFlags              ValueFlags              { get { return _flags;               } }
        public                  Node.IExprNode[]        Exprs                   { get { return _exprs;               } }

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
