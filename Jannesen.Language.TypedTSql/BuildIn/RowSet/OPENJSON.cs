using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;


namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/openjson-transact-sql
    public class OPENJSON: TableSource_RowSetBuildIn
    {
        public class WithScheme: Core.AstParseNode, Node.IWithDeclaration
        {
            class Emitor
            {
                class Column
                {
                    public  string              Name;
                    public  DataModel.ISqlType  Type;
                    public  string              Path;

                    public  bool                needsCast
                    {
                        get {
                            if ((Type.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
                                return false;

                            return true;
                        }
                    }
                    public  string              withType
                    {
                        get {
                            if ((Type.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
                                return Type.NativeType.ToSql();

                            if ((Type.TypeFlags & DataModel.SqlTypeFlags.Interface) != 0) {
                                foreach(var intf in Type.Interfaces) {
                                    if (intf.Name == "Parse" && intf.Type == DataModel.SymbolType.ExternalStaticMethod && intf.Parameters.Count == 1)
                                        return intf.Parameters[0].SqlType.ToSql();
                                }
                            }

                            throw new Exception("No cast posible for " + Type.ToSql());
                        }
                    }
                    public  string              cast
                    {
                        get {
                            if ((Type.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0)
                                return Library.SqlStatic.QuoteName(Name);

                            if ((Type.TypeFlags & DataModel.SqlTypeFlags.Interface) != 0) {
                                foreach(var intf in Type.Interfaces) {
                                    if (intf.Name == "Parse" && intf.Type == DataModel.SymbolType.ExternalStaticMethod && intf.Parameters.Count == 1)
                                        return Type.ToSql() + "::Parse(" + Library.SqlStatic.QuoteName(Name) + ")";
                                }
                            }

                            throw new Exception("No cast posible for " + Type.ToSql());
                        }
                    }
                }

                private                 Core.EmitWriter         _emitWriter;
                private     readonly    OPENJSON                _openjson;
                private     readonly    WithScheme              _withschema;
                private     readonly    Core.AstNodeList        _openjson_children;
                private                 int                     _openjson_pos;
                private     readonly    List<Column>            _columns;

                public                                          Emitor(OPENJSON openjson, WithScheme withschema, DataModel.JsonSchema jsonSchema, DataModel.IColumnList columnList)
                {
                    _openjson          = openjson;
                    _withschema        = withschema;
                    _openjson_children = openjson.Children;
                    _columns           = new List<Column>(columnList.Count);

                    if (jsonSchema is DataModel.JsonSchemaObject) {
                        for (int i = 0 ; i < columnList.Count ; ++i) {
                            var c = columnList[i];

                            if (((DataModel.ColumnWith)c).isUsed) {
                                _columns.Add(new Column(){ Name=c.Name, Type=c.SqlType, Path="$.\"" + c.Name + "\"" });
                            }
                        }
                    }
                    else if (jsonSchema is DataModel.JsonSchemaValue valueSchema)
                    {
                        _columns.Add(new Column(){ Name="$value", Type=valueSchema.SqlType, Path="$" });
                    }
                }
                public                  void                    Emit(Core.EmitWriter emitWriter)
                {
                    _emitWriter = emitWriter;

                    _openjson_pos = 0;

                    while (_openjson_children[_openjson_pos].isWhitespaceOrComment)
                        _openjson_children[_openjson_pos++].Emit(_emitWriter);

                    int indent = _emitWriter.Linepos;

                    if (_needsCast())
                        _emitSELECT(indent);
                    else
                        _emitOPENJSON_WITH(indent);

                    while (_openjson_pos < _openjson_children.Count)
                        _openjson_children[_openjson_pos++].Emit(_emitWriter);
                }

                private                 bool                    _needsCast()
                {
                    foreach(var c in _columns) {
                        if (c.needsCast)
                            return true;
                    }

                    return false;
                }
                private                 void                    _emitSELECT(int indent)
                {
                    _emitWriter.WriteText("(SELECT ");

                    bool f = false;

                    foreach(var c in _columns) {
                        if (f)
                            _emitWriter.WriteText(",");
                        else
                            f = true;

                        _emitWriter.WriteText(Library.SqlStatic.QuoteName(c.Name));
                        _emitWriter.WriteText("=");
                        _emitWriter.WriteText(c.cast);
                    }

                    _emitWriter.WriteNewLine(indent+3, "FROM ");

                    _emitOPENJSON_WITH(indent + 8);

                    _emitWriter.WriteNewLine(indent, ")");

                    if (_openjson.n_Alias == null)
                        _emitWriter.WriteText(" _@_");
                }
                private                 void                    _emitOPENJSON_WITH(int indent)
                {
                    while (!Object.ReferenceEquals(_openjson_children[_openjson_pos], _withschema))
                        _openjson_children[_openjson_pos++].Emit(_emitWriter);

                    _emitWriter.WriteNewLine(indent, "WITH (");

                    bool    f = false;

                    foreach(var c in _columns) {
                        if (f)
                            _emitWriter.WriteText(",");
                        else
                            f = true;

                        _emitWriter.WriteNewLine(indent + 4, Library.SqlStatic.QuoteName(c.Name), " ", c.withType, " '", c.Path, "\'");

                        if (c.Type is DataModel.SqlTypeJson)
                            _emitWriter.WriteText(" AS JSON");
                    }

                    _emitWriter.WriteNewLine(indent, ") ");
                }
            }

            private                 DataModel.JsonSchema            _jsonSchema;
            private                 DataModel.IColumnList           _columnList;

            public                                                  WithScheme()
            {
            }

            public                  DataModel.IColumnList           getColumnList(Transpile.Context context, Node.IExprNode docexpr, Node.IExprNode pathexpr)
            {
                var sqlType = docexpr.SqlType;
                if (sqlType == null || sqlType is DataModel.SqlTypeAny)
                    return null;

                if (!(sqlType is DataModel.SqlTypeJson jsonType))
                    throw new TranspileException(docexpr, "expect json type.");

                var jsonSchema = pathexpr != null ? new JsonPathParser().Parse(jsonType.JsonSchema, Validate.ConstString(pathexpr)) : jsonType.JsonSchema;

                {
                    if (jsonSchema is DataModel.JsonSchemaObject objScheme)
                        return _columnList = _objectColumns(docexpr, objScheme);
                }

                if (jsonSchema is DataModel.JsonSchemaArray arr) {
                    if (arr.JsonSchema is DataModel.JsonSchemaObject objScheme)
                        return _columnList = _objectColumns(docexpr, objScheme);

                    if (arr.JsonSchema is DataModel.JsonSchemaValue valueScheme)
                        return _columnList = _valueColumn(valueScheme);
                }

                throw new TranspileException(pathexpr, "result of json-path is not a object, array-object, array-value");
            }

            public      override    void                            TranspileNode(Transpile.Context context)
            {
            }
            public                  void                            Emit(Core.EmitWriter emitWriter, OPENJSON openjson)
            {
                try {
                    new Emitor(openjson, this, _jsonSchema, _columnList).Emit(emitWriter);
                }
                catch(Exception err) {
                    throw new EmitException(openjson, err.Message);
                }
            }

            private                 DataModel.IColumnList           _objectColumns(Node.IExprNode docexpr, DataModel.JsonSchemaObject objectSchema)
            {
                _jsonSchema = objectSchema;

                var columnList = new DataModel.ColumnList(objectSchema.Properties.Count);

                foreach(var c in objectSchema.Properties) {
                    DataModel.ISqlType  sqlType;

                    if (c.JsonSchema is DataModel.JsonSchemaValue value)
                        sqlType = value.SqlType;
                    else
                        sqlType = new DataModel.SqlTypeJson(((DataModel.SqlTypeJson)(docexpr.SqlType)).NativeType, c.JsonSchema);

                    var column = new DataModel.ColumnWith(c.Name, c.Declaration, sqlType);

                    if (!columnList.TryAdd(column))
                        throw new Exception("Column [" + c.Name + "] already declared.");
                }

                return columnList;
            }
            private                 DataModel.IColumnList           _valueColumn(DataModel.JsonSchemaValue valueScheme)
            {
                _jsonSchema = valueScheme;

                var columnList = new DataModel.ColumnList(1);

                columnList.Add(new DataModel.ColumnWith("$value", null, valueScheme.SqlType));

                return columnList;
            }
        }

        public      readonly    Node.IExprNode                      n_Json;
        public      readonly    Node.IExprNode                      n_Path;
        public      readonly    Node.IWithDeclaration               n_With;
        public      override    DataModel.IColumnList               ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList               _t_ColumnList;

        internal                                                    OPENJSON(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Json = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null)
                n_Path = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);

            if (reader.CurrentToken.isToken(Core.TokenID.WITH)) {
                n_With = AddChild(new TableSource_WithDeclaration(reader, TableSourceWithType.Json));
            }
            else
                AddBeforeWhitespace(n_With = new WithScheme());

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            n_Json.TranspileNode(context);
            n_Path?.TranspileNode(context);
            n_With.TranspileNode(context);

            try {
                Validate.ValueString(n_Json);

                if (n_Path != null)
                    Validate.ValueString(n_Path);

                _t_ColumnList = n_With.getColumnList(context, n_Json, n_Path);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }

            TranspileRowSet(context);
        }
        public      override    void                                Emit(Core.EmitWriter emitWriter)
        {
            if (n_With is WithScheme withScheme)
                withScheme.Emit(emitWriter, this);
            else
                base.Emit(emitWriter);
        }
    }
}
