using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_Function:
    //      : Objectname '(' Expression ( ',' Expression )* ')'
    public class Expr_ColumnUserFunction: Expr, IReferencedEntity
    {
        public class Call: Core.AstParseNode
        {
            public      readonly    string                              n_Schema;
            public      readonly    Core.TokenWithSymbol                n_Name;
            public      readonly    Expr_Collection                     n_Arguments;

            public                  DataModel.EntityObjectCode          EntityObjectCode        { get; private set; }

            private                 bool                                _addSchema;

            public                                                      Call(Core.ParserReader reader)
            {
                n_Schema    = reader.Options.Schema;
                n_Name      = ParseName(reader);
                n_Arguments = AddChild(new Expr_Collection(reader, false));
            }

            public                  bool                                TranspileUserFunction(string schema, Transpile.Context context)
            {
                var entityName   = new DataModel.EntityName(schema ?? n_Schema, n_Name.ValueString);
                var entityObject = context.Catalog.GetObject(entityName);
                if (entityObject == null) {
                    context.AddError(n_Name, "Unknown function " + entityName.Fullname + ".");
                    return false;
                }

                if (!(entityObject.Type == DataModel.SymbolType.FunctionScalar || entityObject.Type == DataModel.SymbolType.FunctionScalar_clr)) {
                    context.AddError(n_Name, entityName.Fullname + " is not a scalar-function.");
                    return false;
                }

                EntityObjectCode = (DataModel.EntityObjectCode)entityObject;
                n_Name.SetSymbolUsage(EntityObjectCode, DataModel.SymbolUsageFlags.Reference);

                _addSchema = schema == null;
                context.CaseWarning(n_Name, entityName.Name);

                Validate.FunctionArguments(context, this, EntityObjectCode, n_Arguments.n_Expressions);
                return true;
            }
            public      override    void                                TranspileNode(Transpile.Context context)
            {
                n_Arguments.TranspileNode(context);
            }

            public      override    void                                Emit(Core.EmitWriter emitWriter)
            {
                foreach(var node in Children) {
                    if (object.ReferenceEquals(node, n_Name) && _addSchema)
                        emitWriter.WriteText(Library.SqlStatic.QuoteNameIfNeeded(n_Schema) + ".");

                    node.Emit(emitWriter);
                }
            }
        }

        public      readonly    Core.IAstNode[]                     n_Nodes;

        public      override    DataModel.ValueFlags                ValueFlags          { get { return _valueFlags;       } }
        public      override    DataModel.ISqlType                  SqlType             { get { return _sqlType;          } }
        public      override    ExprType                            ExpressionType      { get { return ExprType.Complex;  } }
        public      override    DataModel.Column                    ReferencedColumn    { get { return _referencedColumn; } }
        public      override    bool                                NoBracketsNeeded    { get { return true;              } }

        private                 DataModel.ValueFlags                _valueFlags;
        private                 DataModel.ISqlType                  _sqlType;
        private                 DataModel.Column                    _referencedColumn;

        public                                                      Expr_ColumnUserFunction(Core.ParserReader reader)
        {
            var nodes = new List<Core.IAstNode>();

            do {
                switch(reader.CurrentToken.ID) {
                case Core.TokenID.Name:
                case Core.TokenID.QuotedName:
                    if (reader.NextPeek().isToken(Core.TokenID.LrBracket)) {
                        nodes.Add(AddChild(new Call(reader)));
                        break;
                    }
                    else
                        nodes.Add(ParseToken(reader));
                    break;

                default:
                    throw new ParseException(reader.CurrentToken, "Expect name or quotednamed.");
                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Dot) != null);

            n_Nodes = nodes.ToArray();
        }

        public                  DataModel.EntityName                getReferencedEntity(DeclarationObjectCode declarationObjectCode)
        {
            // ReferencedEntity for sorting function for transpile.
            if (n_Nodes[0] is Call call1)
                return new DataModel.EntityName(call1.n_Schema, call1.n_Name.ValueString);

            if (n_Nodes.Length == 2 &&
                n_Nodes[0] is Core.TokenWithSymbol token && token.isNameOrQuotedName &&
                n_Nodes[1] is Call call2)
                return new DataModel.EntityName(token.ValueString, call2.n_Name.ValueString);

            return null;
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _referencedColumn  = null;
            _valueFlags = DataModel.ValueFlags.Error;
            _sqlType    = null;

            try {
                foreach(var node in n_Nodes) {
                    if (node is Call call)
                        call.TranspileNode(context);
                }

                var transpileExpr = new TranspileExpr(this, context);

                if (transpileExpr.Transpile()) {
                    this._valueFlags        = transpileExpr.ValueFlags;
                    this._sqlType           = transpileExpr.SqlType;
                    this._referencedColumn  = transpileExpr.ReferencedColumn;
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
        public                  void                                SetColumnSymbol(DataModel.SymbolData symbol)
        {
            if (!(n_Nodes[n_Nodes.Length - 1] is Core.TokenWithSymbol tokenWithSymbol)) {
                throw new InvalidCastException("Internal error Expr_ColumnUserFunction.SetColumnSymbol");
            }

            tokenWithSymbol.SetSymbolData(symbol);
        }
        
        struct TranspileExpr
        {
            public  readonly    Expr_ColumnUserFunction ExprPrimativeValue;
            public  readonly    Core.IAstNode[]         Nodes;
            public  readonly    Transpile.Context       Context;
            public              DataModel.ValueFlags    ValueFlags;
            public              DataModel.ISqlType      SqlType;
            public              DataModel.Column        ReferencedColumn;
            private             int                     _nodeindex;

            public                                      TranspileExpr(Expr_ColumnUserFunction exprPrimativeValue, Transpile.Context context)
            {
                this.ExprPrimativeValue = exprPrimativeValue;
                this.Nodes              = exprPrimativeValue.n_Nodes;
                this.Context            = context;
                this.ValueFlags         = DataModel.ValueFlags.None;
                this.SqlType            = null;
                this.ReferencedColumn   = null;
                this._nodeindex         = 0;
            }

            public              bool                    Transpile()
            {
                try {
                    if (!_transpileFirst(Nodes[_nodeindex]))
                        return false;

                    ++_nodeindex;
                    while (_nodeindex < Nodes.Length) {
                        ReferencedColumn = null;
                        if (!_transpileNext(Nodes[_nodeindex]))
                            return false;

                        ++_nodeindex;
                    }
                }
                catch(Exception err) {
                    Context.AddError(Nodes[_nodeindex], err);
                    return false;
                }

                return true;
            }

            private             bool                    _transpileFirst(Core.IAstNode node)
            {
                if (node is Core.TokenWithSymbol token) {
                    if (token.isNameOrQuotedName) {
                        var name = token.ValueString;

                        if (Context.RowSets != null) {
                            var rowset = Context.FindRowSet(name);
                            if (rowset != null) {
                                Context.CaseWarning(token, rowset.Name);
                                token.SetSymbolUsage(rowset, DataModel.SymbolUsageFlags.Reference);

                                ValueFlags = DataModel.ValueFlags.RowSet;
                                SqlType    = new DataModel.SqlTypeRowSet(rowset);
                                return true;
                            }

                            var column = Context.FindColumn(name, out bool ambiguous);
                            if (column != null) {
                                token.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Read);
                                column.SetUsed();

                                if (ambiguous)
                                    Context.AddError(token, "Column [" + name + "] is ambiguous.");
                                else
                                    Context.CaseWarning(token, column.Name);

                                ValueFlags = column.ValueFlags | DataModel.ValueFlags.Column;
                                SqlType    = column.SqlType;
                                ReferencedColumn = column;
                                return true;
                            }
                        }

                        if (ExprPrimativeValue.n_Nodes.Length > 1 && ExprPrimativeValue.n_Nodes[1] is Call call1) {
                            var schema = Context.Catalog.GetSchema(name);
                            if (schema != null) {
                                token.SetSymbolUsage(schema, DataModel.SymbolUsageFlags.Reference);
                                Context.CaseWarning(token, schema.Name);
                                ++_nodeindex; // Eat schema
                                _transpileUserFunction(schema.Name, call1);
                                return true;
                            }
                        }

                        Context.AddError(node, "Can't resolve '" + name + "'.");
                        return false;
                    }
                }

                if (node is Call call2) {
                    return _transpileUserFunction(null, call2);
                }

                throw new InvalidOperationException(node.GetType() + ": unable to transpile.");
            }
            private             bool                    _transpileNext(Core.IAstNode node)
            {
                if ((SqlType.TypeFlags & DataModel.SqlTypeFlags.RowSet) != 0) {
                    if (node is Core.TokenWithSymbol token && token.isNameOrQuotedName) {
                        var name = token.ValueString;

                        var column = SqlType.Columns.FindColumn(name, out bool ambiguous);
                        if (column == null) {
                            Context.AddError(node, "Unknown column [" + name + "].");
                            return false;
                        }

                        token.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Read);
                        column.SetUsed();

                        if (ambiguous)
                            Context.AddError(token, "Column [" + name + "] is ambiguous.");
                        else
                            Context.CaseWarning(token, column.Name);

                        ValueFlags = column.ValueFlags | DataModel.ValueFlags.Column;
                        SqlType    = column.SqlType;
                        ReferencedColumn = column;
                        return true;
                    }

                    Context.AddError(node, "Expect name or quoted-name.");
                    return false;
                }

                if (SqlType == null || SqlType is DataModel.SqlTypeAny) {
                    ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                    SqlType = new DataModel.SqlTypeAny();
                    return true;
                }

                if ((SqlType as DataModel.SqlTypeNative)?.SystemType == DataModel.SystemType.Xml) {
                    if (node is Call call) {
                        SqlType = _transpileFirstXml_method(call);
                        if (SqlType == null) {
                            Context.AddError(node, "Unknown method '" + call.n_Name.ValueString + "'.");
                            return false;
                        }

                        ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                        return true;
                    }
                    else {
                        Context.AddError(node, "xml datatype has no properties.");
                        return false;
                    }
                }
                else
                if ((SqlType.TypeFlags & DataModel.SqlTypeFlags.Interface) != 0) {
                    if (node is Call call) {
                        SqlType = Validate.Method(SqlType.Interfaces, false, call.n_Name, call.n_Arguments.n_Expressions);
                        ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                        return true;
                    }

                    if (node is Core.TokenWithSymbol token) {
                        SqlType = Validate.Property(SqlType.Interfaces, false, token);
                        ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                        return true;
                    }
                }

                Context.AddError(node, "Type '" + SqlType.ToString() + "' has no properties or methods.");
                return false;
            }
            private             bool                    _transpileUserFunction(string schema, Call call)
            {
                if (!call.TranspileUserFunction(schema, Context)) {
                    return false;
                }

                ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                SqlType    = call.EntityObjectCode.Returns;
                return true;
            }
            private             DataModel.ISqlType      _transpileFirstXml_method(Call call)
            {
                Core.TokenWithSymbol.SetNoSymbol(call.n_Name);

                return Xml.Transpile(Context, call, call.n_Name.ValueString, call.n_Arguments.n_Expressions);
            }
        }
    }
}
