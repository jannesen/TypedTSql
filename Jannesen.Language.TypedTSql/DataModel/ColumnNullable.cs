using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnNullable: Column
    {
        public      override    ISymbol                 Symbol                              => _parent.Symbol;
        public      override    string                  Name                                => _parent.Name;
        public      override    object                  Declaration                         => _parent.Declaration;
        public      override    ISymbol                 ParentSymbol                        => _parent.ParentSymbol;
        public      override    ISymbol                 SymbolNameReference                 => _parent.SymbolNameReference;
        public      override    ValueFlags              ValueFlags                          => _parent.ValueFlags | ValueFlags.Nullable;
        public      override    ISqlType                SqlType                             => _parent.SqlType;
        public      override    string                  CollationName                       => _parent.CollationName;
        public      override    Node.IExprNode          Expr                                => _parent.Expr;

        public      override    bool                    ValidateConst(ISqlType sqlType)     => _parent.ValidateConst(sqlType);
        public      override    void                    SetUsed()                           => _parent.SetUsed();
        internal    override    void                    SetParent(DataModel.ISymbol parent) => _parent.SetParent(parent);

        private                 Column                  _parent;

        public                                          ColumnNullable(Column parent)
        {
            _parent = parent;
        }
    }
}
