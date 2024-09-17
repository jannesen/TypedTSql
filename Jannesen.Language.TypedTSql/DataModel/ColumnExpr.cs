using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnExpr: Column, ISymbol
    {
        public      override    ISymbol                 Symbol                  => this;
        public                  SymbolType              Type                    => SymbolType.Column;
        public      override    string                  Name                    => _name;
        public      override    object                  Declaration             => _declaration;
        public      override    ISqlType                SqlType                 => _expr.SqlType;
        public      override    string                  CollationName           => _expr.CollationName;
        public      override    ValueFlags              ValueFlags              => _valueFlags;
        public      override    Node.IExprNode          Expr                    => _expr;

        private                 string                  _name;
        private                 Node.IExprNode          _expr;
        public                  ValueFlags              _valueFlags;
        private                 object                  _declaration;

        public                                          ColumnExpr(Node.IExprNode expr, object declaration=null)
        {
            _name        = "";
            _expr        = expr;
            _valueFlags  = expr.ValueFlags | ValueFlags.Unnammed;
            _declaration = declaration;
        }
        public                                          ColumnExpr(Core.TokenWithSymbol name, Node.IExprNode expr, object declaration=null)
        {
            _name        = name.ValueString;
            _expr        = expr;
            _valueFlags  = expr.ValueFlags;
            _declaration = declaration;
        }

        public      override    bool                    ValidateConst(ISqlType sqlType)
        {
            return _expr.ValidateConst(sqlType);
        }
    }
}
