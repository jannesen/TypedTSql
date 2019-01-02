using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnExpr: Column
    {
        public      override    string                  Name                    { get { return _name;                } }
        public      override    object                  Declaration             { get { return _declaration;         } }
        public      override    ISqlType                SqlType                 { get { return _expr.SqlType;        } }
        public      override    string                  CollationName           { get { return _expr.CollationName;  } }
        public      override    ValueFlags              ValueFlags              { get { return _valueFlags;          } }
        public                  Core.TokenWithSymbol    NameToken               { get { return _nameToken;           } }
        public                  Node.IExprNode          Expr                    { get { return _expr;                } }

        private                 string                  _name;
        private                 Core.TokenWithSymbol    _nameToken;
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
            _nameToken   = name;
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
