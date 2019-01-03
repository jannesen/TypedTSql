using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_Function:
    //      : Objectname '(' Expression ( ',' Expression )* ')'
    public class Expr_PrimativeValue: Expr, IReferencedEntity
    {
        public class Call: Core.AstParseNode
        {
            public      readonly    Core.TokenWithSymbol                n_Name;
            public      readonly    Expr_Collection                     n_Arguments;

            private                 string                              _addSchema;

            public                                                      Call(Core.ParserReader reader)
            {
                n_Name      = ParseName(reader);
                n_Arguments = AddChild(new Expr_Collection(reader, false));
            }

            public      override    void                                TranspileNode(Transpile.Context context)
            {
                n_Arguments.TranspileNode(context);
            }

            internal                void                                AddSchema(string schema)
            {
                _addSchema = schema;
            }

            public      override    void                                Emit(Core.EmitWriter emitWriter)
            {
                foreach(var node in Children) {
                    if (object.ReferenceEquals(node, n_Name) && _addSchema != null)
                        emitWriter.WriteText(Library.SqlStatic.QuoteNameIfNeeded(_addSchema) + ".");

                    node.Emit(emitWriter);
                }
            }
        }

        public      readonly    string                              n_Schema;
        public      readonly    Core.IAstNode[]                     n_Nodes;

        public      override    DataModel.ValueFlags                ValueFlags          { get { return _valueFlags;     } }
        public      override    DataModel.ISqlType                  SqlType             { get { return _sqlType;        } }
        public      override    ExprType                            ExpressionType      { get { return n_Nodes.Length == 1 && n_Nodes[0] is Token.TokenLocalName ? ExprType.Const : ExprType.Complex;   } }
        public      override    bool                                NoBracketsNeeded    { get { return true;            } }
        public                  DataModel.ISymbol                   Referenced          { get; private set; }

        private                 DataModel.ValueFlags                _valueFlags;
        private                 DataModel.ISqlType                  _sqlType;

        public                                                      Expr_PrimativeValue(Core.ParserReader reader, bool localVariable=false)
        {
            var nodes = new List<Core.IAstNode>();

            if (localVariable) {
                nodes.Add(ParseToken(reader, Core.TokenID.LocalName));
            }
            else {
                do {
                    switch(reader.CurrentToken.ID) {
                    case Core.TokenID.Name:
                    case Core.TokenID.QuotedName:
                        if (reader.NextPeek().isToken(Core.TokenID.LrBracket)) {
                            if (nodes.Count == 0) {
                                if ((n_Schema = reader.Options.Schema) == null)
                                    throw new ParseException(reader.CurrentToken, "Schema not defined.");
                            }

                            nodes.Add(AddChild(new Call(reader)));
                        }
                        else
                            nodes.Add(ParseToken(reader));
                        break;

                    case Core.TokenID.LocalName:
                        if (nodes.Count == 0) {
                            nodes.Add(ParseToken(reader));
                            break;
                        }
                        goto default;
                    default:
                        throw new ParseException(reader.CurrentToken, "Expect name or quotednamed.");
                    }
                }
                while (ParseOptionalToken(reader, Core.TokenID.Dot) != null);
            }

            n_Nodes = nodes.ToArray();
        }

        public                  DataModel.EntityName                getReferencedEntity(DeclarationObjectCode declarationObjectCode)
        {
            if (n_Nodes[0] is Call call1)
                return new DataModel.EntityName(n_Schema, call1.n_Name.ValueString);

            if (n_Nodes.Length >= 2 &&
                n_Nodes[1] is Call call2 &&
                n_Nodes[0] is Core.TokenWithSymbol token && token.isNameOrQuotedName)
                return new DataModel.EntityName(token.ValueString, call2.n_Name.ValueString);

            return null;
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            Referenced  = null;
            _valueFlags = DataModel.ValueFlags.Error;
            _sqlType    = null;

            try {
                foreach(var node in n_Nodes) {
                    if (node is Core.AstParseNode astParseNode)
                        astParseNode.TranspileNode(context);
                }

                var transpileExpr = new TranspileExpr(this, context);

                if (transpileExpr.Transpile()) {
                    this._valueFlags = transpileExpr.ValueFlags;
                    this._sqlType    = transpileExpr.SqlType;
                    this.Referenced  = transpileExpr.Referenced;
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
        public      override    bool                                ValidateConst(DataModel.ISqlType sqlType)
        {
            return Referenced is IExprNode referenceExpr? referenceExpr.ValidateConst(sqlType) : false;
        }
        public      override    object                              ConstValue()
        {
            return new TranspileException(this, "Can't calculate constant value.");
        }
        public      override    DataModel.Variable                  GetVariable(Transpile.Context context)
        {
            if (n_Nodes.Length == 1 && n_Nodes[0] is Token.TokenLocalName localName) {
                return context.VariableGet(localName, true);
            }

            throw new InvalidOperationException("Expression is not a variable");
        }

        struct TranspileExpr
        {
            public  readonly    Expr_PrimativeValue     ExprPrimativeValue;
            public  readonly    Core.IAstNode[]         Nodes;
            public  readonly    Transpile.Context       Context;
            public              DataModel.ValueFlags    ValueFlags;
            public              DataModel.ISqlType      SqlType;
            public              DataModel.ISymbol       Referenced;
            private             int                     _nodeindex;

            public                                      TranspileExpr(Expr_PrimativeValue exprPrimativeValue, Transpile.Context context)
            {
                this.ExprPrimativeValue = exprPrimativeValue;
                this.Nodes         = exprPrimativeValue.n_Nodes;
                this.Context       = context;
                this.ValueFlags    = DataModel.ValueFlags.None;
                this.SqlType       = null;
                this.Referenced    = null;
                this._nodeindex    = 0;
            }

            public              bool                    Transpile()
            {
                try {
                    if (!_transpileFirst(Nodes[_nodeindex]))
                        return false;

                    ++_nodeindex;
                    while (_nodeindex < Nodes.Length) {
                        Referenced = null;
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
                            var rowset = _findRowSet(name);
                            if (rowset != null) {
                                Context.CaseWarning(token, rowset.Name);
                                token.SetSymbol(rowset);

                                ValueFlags = DataModel.ValueFlags.RowSet;
                                SqlType    = rowset;
                                Referenced = rowset;
                                return true;
                            }

                            var column = _findColumn(name, out bool ambiguous);
                            if (column != null) {
                                token.SetSymbol(column);
                                column.SetUsed();

                                if (ambiguous)
                                    Context.AddError(token, "Column [" + name + "] is ambiguous.");
                                else
                                    Context.CaseWarning(token, column.Name);

                                ValueFlags = column.ValueFlags | DataModel.ValueFlags.Column;
                                SqlType    = column.SqlType;
                                Referenced = column;
                                return true;
                            }
                        }

                        if (ExprPrimativeValue.n_Nodes.Length > 1 && ExprPrimativeValue.n_Nodes[1] is Call call1) {
                            var schema = Context.Catalog.GetSchema(name);
                            if (schema != null) {
                                token.SetSymbol(schema);
                                Context.CaseWarning(token, schema.Name);
                                ++_nodeindex; // Eat schema
                                _transpileUserFunction(schema.Name, call1, false);
                                return true;
                            }
                        }

                        Context.AddError(node, "Can't resolve '" + name + "'.");
                        return false;
                    }

                    if (token.isToken(Core.TokenID.LocalName)) {
                        var variable = Context.VariableGet(token, allowGlobal:true);
                        if (variable == null)
                            return false;

                        ValueFlags = variable.isNullable ? DataModel.ValueFlags.Variable|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Variable;
                        SqlType    = variable.SqlType;
                        Referenced = variable;
                        variable.setUsed();
                        return true;
                    }
                }

                if (node is Call call2) {
                    return _transpileUserFunction(ExprPrimativeValue.n_Schema, call2, true);
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

                        token.SetSymbol(column);
                        column.SetUsed();

                        if (ambiguous)
                            Context.AddError(token, "Column [" + name + "] is ambiguous.");
                        else
                            Context.CaseWarning(token, column.Name);

                        ValueFlags = column.ValueFlags | DataModel.ValueFlags.Column;
                        SqlType    = column.SqlType;
                        Referenced = column;
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
            private             bool                    _transpileUserFunction(string schema, Call call, bool addSchema)
            {
                var entityName   = new DataModel.EntityName(schema, call.n_Name.ValueString);
                var entityObject = Context.Catalog.GetObject(entityName);
                if (entityObject == null) {
                    Context.AddError(call.n_Name, "Unknown function " + entityName.Fullname + ".");
                    return false;
                }

                if (!(entityObject.Type == DataModel.SymbolType.FunctionScalar || entityObject.Type == DataModel.SymbolType.FunctionScalar_clr)) {
                    Context.AddError(call.n_Name, entityName.Fullname + " is not a scalar-function.");
                    return false;
                }

                var entityObjectCode = (DataModel.EntityObjectCode)entityObject;
                call.AddSchema(addSchema ? schema : null);
                call.n_Name.SetSymbol(entityObjectCode);
                Context.CaseWarning(call.n_Name, entityName.Name);

                Validate.FunctionArguments(Context, call, entityObjectCode, call.n_Arguments.n_Expressions);

                ValueFlags = DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;
                SqlType    = entityObjectCode.Returns;
                return true;
            }
            private             DataModel.RowSet        _findRowSet(string name)
            {
                var context = Context;
                var r = context.RowSets.FindRowSet(name);
                if (r != null)
                    return r;

                for (context = context.Parent ; context != null ; context = context.Parent) {
                    if (context.RowSetPublic && context.RowSets != null) {
                        r = context.RowSets.FindRowSet(name);
                        if (r != null)
                            return r;
                    }
                }

                return null;
            }
            private             DataModel.Column        _findColumn(string name, out bool ambiguous)
            {
                var context = Context;
                var c = context.RowSets.FindColumn(name, out ambiguous);
                if (c != null)
                    return c;

                for (context = context.Parent ; context != null ; context = context.Parent) {
                    if (context.RowSetPublic && context.RowSets != null) {
                        c = context.RowSets.FindColumn(name, out ambiguous);
                        if (c != null)
                            return c;
                    }
                }

                ambiguous = false;
                return null;
            }
            private             DataModel.ISqlType      _transpileFirstXml_method(Call call)
            {
                Core.TokenWithSymbol.SetNoSymbol(call.n_Name);

                return Xml.Transpile(Context, call, call.n_Name.ValueString, call.n_Arguments.n_Expressions);
            }
        }
    }
}
