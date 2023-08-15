using System;
using System.Collections.Generic;
using System.IO;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Emit
{
    internal class JcProxyEmitor: FileEmitor
    {
        abstract class DeclareName
        {
            public                  string                                  Name;

            public  virtual         bool                                    FullEmit            { get { return false; } }

            public  abstract        void                                    EmitFull(StreamWriter streamWriter);
            public  virtual         void                                    EmitReference(StreamWriter streamWriter)
            {
                streamWriter.Write(Name);
            }

            protected               void                                    emitString(StreamWriter streamWriter, string s)
            {
                streamWriter.Write('\"');
                streamWriter.Write(s);
                streamWriter.Write('\"');
            }
        }
        class DeclareNameList<T>: List<T> where T: DeclareName
        {
            public                  void                                    NameOptimalisation(string prefix)
            {
                var i = 1;

                foreach(var item in this) {
                    if (item.FullEmit && item.Name == null) {
                        item.Name = prefix + (i++).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }
            }
            public                  void                                    EmitFull(StreamWriter streamWriter)
            {
                foreach(var item in this) {
                    if (item.FullEmit) {
                        item.EmitFull(streamWriter);
                    }
                }
            }
        }
        class DeclareImport: DeclareName
        {
            public                  string                                  From;

            public  override        bool                                    FullEmit            { get { return true; } }

            public                                                          DeclareImport(string name, string form)
            {
                this.Name = name;
                this.From = form;
            }
            public  override        void                                    EmitFull(StreamWriter streamWriter)
            {
                streamWriter.Write("import * as ");
                streamWriter.Write(Name);
                streamWriter.Write(" from ");
                emitString(streamWriter, From);
                streamWriter.Write(";");
                streamWriter.WriteLine();
            }
        }
        abstract class DeclareType: DeclareName
        {
            public  override        void                                    EmitFull(StreamWriter streamWriter)
            {
                streamWriter.Write("const ");
                streamWriter.Write(Name);
                streamWriter.Write(" = ");
                EmitExpression(streamWriter);
                streamWriter.Write(";");
                streamWriter.WriteLine();
            }
            public  override        void                                    EmitReference(StreamWriter streamWriter)
            {
                if (FullEmit) {
                    base.EmitReference(streamWriter);
                }
                else {
                    EmitExpression(streamWriter);
                }
            }
            public  abstract        void                                    EmitExpression(StreamWriter streamWriter);
        }
        class DeclareSimpleType: DeclareType
        {
            public                  DeclareName                             Import;
            public                  string                                  Expression;

            public  override        bool                                    FullEmit            { get { return Expression.IndexOf('.') > 0; } }

            public                                                          DeclareSimpleType(DeclareImport import, string expression)
            {
                this.Import     = import;
                this.Expression = expression;
            }
            public  override        void                                    EmitExpression(StreamWriter streamWriter)
            {
                Import.EmitReference(streamWriter);
                streamWriter.Write(".");
                streamWriter.Write(Expression);
            }
        }
        class DeclareRequired: DeclareType
        {
            public                  DeclareType                             Type;

            public  override        bool                                    FullEmit            { get { return true; } }

            public                                                          DeclareRequired(DeclareType type)
            {
                this.Type = type;
            }
            public  override        void                                    EmitExpression(StreamWriter streamWriter)
            {
                Type.EmitReference(streamWriter);
                streamWriter.Write(".subClass({ required: true })");
            }
        }
        abstract class DeclareComplexType: DeclareType
        {
        }
        class RecordField
        {
            public  string          Name;
            public  DeclareType     Type;
        }
        class DeclareRecord: DeclareComplexType
        {
            public                  DeclareName                             ImportJT;
            public                  RecordField[]                           Fields;

            public  override        bool                                    FullEmit            { get { return true; } }

            public                                                          DeclareRecord(DeclareName importJT, RecordField[] fields)
            {
                this.ImportJT = importJT;
                this.Fields   = fields;
            }
            public  override        void                                    EmitExpression(StreamWriter streamWriter)
            {
                ImportJT.EmitReference(streamWriter);
                streamWriter.Write(".Record.define({");
                streamWriter.WriteLine();

                int maxNameLength = 0;

                for (var i = 0 ; i < Fields.Length ; ++i) {
                    var f = Fields[i];
                    if (maxNameLength < f.Name.Length)
                        maxNameLength = f.Name.Length;
                }

                for (var i = 0 ; i < Fields.Length ; ++i) {
                    var f = Fields[i];
                    streamWriter.Write("    ");
                    streamWriter.Write("'" + f.Name + "': ");

                    for (int l = maxNameLength - f.Name.Length ; l > 0 ; --l)
                        streamWriter.Write(' ');

                    f.Type.EmitReference(streamWriter);
                    if (i < Fields.Length - 1)
                        streamWriter.Write(",");

                    streamWriter.WriteLine();
                }
                streamWriter.Write("})");
            }
            public                  bool                                    compareEqual(RecordField[] fields)
            {
                if (Fields.Length != fields.Length)
                    return false;

                for (int i = 0 ; i < fields.Length ; ++i) {
                    if (Fields[i].Name != fields[i].Name ||
                        Fields[i].Type != fields[i].Type)
                        return false;
                }

                return true;
            }
        }
        class DeclareSet: DeclareComplexType
        {
            public                  DeclareName                             ImportJT;
            public                  DeclareType                             Type;

            public                                                          DeclareSet(DeclareName importJT, DeclareType type)
            {
                this.ImportJT = importJT;
                this.Type     = type;
            }
            public  override        void                                    EmitExpression(StreamWriter streamWriter)
            {
                ImportJT.EmitReference(streamWriter);
                streamWriter.Write(".Set.define(");
                Type.EmitReference(streamWriter);
                streamWriter.Write(")");
            }
        }
        class DeclareProxy: DeclareName
        {
            public                  string[]                                Methods;
            public                  string                                  Callname;
            public                  int?                                    Timeout;
            public                  DeclareType                             Callargs_type;
            public                  DeclareType                             Request_type;
            public                  DeclareType                             Response_type;

            public  override        bool                                    FullEmit            { get { return true; } }

            public                                                          DeclareProxy()
            {
            }
            public  override        void                                    EmitFull(StreamWriter streamWriter)
            {
                streamWriter.WriteLine();
                streamWriter.Write("export type ");
                    streamWriter.Write(Name);
                    streamWriter.Write(" = typeof ");
                    streamWriter.Write(Name);
                    streamWriter.Write(";");
                    streamWriter.WriteLine();

                streamWriter.Write("export const ");
                    streamWriter.Write(Name);
                    streamWriter.Write(" = {");
                    streamWriter.WriteLine();

                if (Methods != null && Methods.Length > 0) {
                        if (Methods.Length == 1) {
                            streamWriter.Write("    method:        ");
                            emitString(streamWriter, Methods[0]);
                        }
                        else {
                            bool    first = true;
                            foreach(var method in Methods) {
                                streamWriter.Write(first ? "    methods:       [" : ", ");
                                emitString(streamWriter, method);
                                first = false;
                            }

                            streamWriter.Write(" ]");
                        }

                        if (Callname != null || Timeout.HasValue || Callargs_type != null || Request_type != null || Response_type != null)
                            streamWriter.Write(',');
                        streamWriter.WriteLine();
                }

                if (Callname != null) {
                    streamWriter.Write("    callname:      ");
                        streamWriter.Write(Callname);
                        if (Timeout.HasValue || Callargs_type != null || Request_type != null || Response_type != null)
                            streamWriter.Write(',');
                        streamWriter.WriteLine();
                }

                if (Timeout.HasValue) {
                    streamWriter.Write("    timeout:       ");
                        streamWriter.Write(Timeout.Value);
                        if (Callargs_type != null || Request_type != null || Response_type != null)
                            streamWriter.Write(',');
                        streamWriter.WriteLine();
                }

                if (Callargs_type != null) {
                    streamWriter.Write("    callargs_type: ");
                        Callargs_type.EmitReference(streamWriter);
                        if (Request_type != null || Response_type != null)
                            streamWriter.Write(',');
                        streamWriter.WriteLine();
                }

                if (Request_type != null) {
                    streamWriter.Write("    request_type:  ");
                        Request_type.EmitReference(streamWriter);
                        if (Response_type != null)
                            streamWriter.Write(',');
                        streamWriter.WriteLine();
                }

                if (Response_type != null) {
                    streamWriter.Write("    response_type: ");
                        Response_type.EmitReference(streamWriter);
                        streamWriter.WriteLine();
                }

                streamWriter.Write("};");
                    streamWriter.WriteLine();
            }
        }
        class ProxyFile
        {
            public      readonly    string                                  Filename;

            private     readonly    DeclareNameList<DeclareImport>          _imports;
            private     readonly    DeclareNameList<DeclareName>            _simpletypes;
            private     readonly    DeclareNameList<DeclareComplexType>     _complextypes;
            private     readonly    DeclareNameList<DeclareProxy>           _proxys;

            public                                                          ProxyFile(string filename)
            {
                this.Filename       = filename;

                _imports      = new DeclareNameList<DeclareImport>();
                _simpletypes  = new DeclareNameList<DeclareName>();
                _complextypes = new DeclareNameList<DeclareComplexType>();
                _proxys       = new DeclareNameList<DeclareProxy>();

                _imports.Add(new DeclareImport("$JT", "jc3/jannesen.datatype"));
            }

            public                  void                                    AddMethod(Node.WEBSERVICE_EMITOR_JC_PROXY webServiceEmitor, Node.WEBMETHOD webMethod)
            {
                _proxys.Add((new ProcessMethod(this, webServiceEmitor)).Process(webMethod));
            }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
            public                  void                                    Emit(EmitContext emitContext)
            {
                _imports.NameOptimalisation("$");
                _simpletypes.NameOptimalisation("ST");
                _complextypes.NameOptimalisation("CT");
                _proxys.NameOptimalisation("P");

                try {
                    using (var fileData = new MemoryStream()) {
                        using (var writer = new StreamWriter(fileData, System.Text.Encoding.UTF8, 256, true)) {
                            _imports.EmitFull(writer);
                            writer.WriteLine();
                            _simpletypes.EmitFull(writer);
                            writer.WriteLine();
                            _complextypes.EmitFull(writer);
                            _proxys.EmitFull(writer);
                        }

                        FileUpdate.Update(Filename, fileData);
                    }
                }
                catch(Exception err) {
                    emitContext.AddEmitError(new EmitError("Emit '" + Filename + "' failed: " + err.Message));
                }
            }

            public                  DeclareImport                           getImport(string from)
            {
                foreach(var i in _imports) {
                    if (i.From == from)
                        return i;
                }

                var n = new DeclareImport(null, from);
                _imports.Add(n);
                return n;
            }
            public                  DeclareSimpleType                       getSimpleType(string importName, string expression)
            {
                var import = getImport(importName);

                foreach(var i in _simpletypes) {
                    if (i is DeclareSimpleType simpleType && simpleType.Expression == expression && simpleType.Import == import)
                        return simpleType;
                }

                var n = new DeclareSimpleType(import, expression);
                _simpletypes.Add(n);
                return n;
            }
            public                  DeclareRequired                         getRequired(DeclareType type)
            {
                foreach(var i in _simpletypes) {
                    if (i is DeclareRequired requiredType && requiredType.Type == type)
                        return requiredType;
                }

                var n = new DeclareRequired(type);
                _simpletypes.Add(n);
                return n;
            }
            public                  DeclareRecord                           getRecord(RecordField[] fields)
            {
                foreach(var i in _complextypes) {
                    if (i is DeclareRecord declareRecord) {
                        if (declareRecord.compareEqual(fields))
                            return declareRecord;
                    }
                }

                var n = new DeclareRecord(getImport("jc3/jannesen.datatype"), fields);
                _complextypes.Add(n);
                return n;
            }
            public                  DeclareSet                              getSet(DeclareType type)
            {
                var n = new DeclareSet(getImport("jc3/jannesen.datatype"), type);
                _complextypes.Add(n);
                return n;
            }
        }
        class ProcessMethod
        {
            private     readonly    ProxyFile                               _proxyFile;
            private     readonly    Node.WEBSERVICE_EMITOR_JC_PROXY         _webServiceEmitor;
            private                 List<RecordField>                       _callArgs;
            private                 List<RecordField>                       _reqArgs;
            private                 DeclareType                             _textjson;

            public                                                          ProcessMethod(ProxyFile proxyFile, Node.WEBSERVICE_EMITOR_JC_PROXY webServiceEmitor)
            {
                _proxyFile        = proxyFile;
                _webServiceEmitor = webServiceEmitor;
            }

            public                  DeclareProxy                            Process(Node.WEBMETHOD webMethod)
            {
                _callArgs = new List<RecordField>();
                _reqArgs  = new List<RecordField>();

                foreach(Node.WEBMETHOD.ServiceParameter parameter in webMethod.n_Parameters.n_Parameters) {
                    if (parameter.n_Type is Node.JsonType jsonType) {
                        if (parameter.n_Source != null && parameter.n_Source.n_Source != "body:json")
                            throw new InvalidOperationException("json only supported with source='body:json'.");

                        _textjson = _getByJsonScheme(jsonType.n_Schema.n_Schema);
                    }
                    else
                        _processSimpleParameter(parameter);
                }


                var declareProxy = new DeclareProxy();

                declareProxy.Name     = webMethod.n_Declaration.JcProxy.Expression;
                declareProxy.Methods   = webMethod.n_Declaration.n_Methods;
                declareProxy.Callname = "\"" + (_webServiceEmitor.n_BaseUrl ?? "") + webMethod.n_Declaration.n_ServiceMethodName.n_Name.ValueString.Replace("\"", "\\\"") + "\"";

                var timeout = webMethod.n_Declaration.GetWebHandlerOptionValueByName("timeout");
                if (timeout != null)
                    declareProxy.Timeout = int.Parse(timeout, System.Globalization.CultureInfo.InvariantCulture) * 1000;


                if (_callArgs.Count > 0)        declareProxy.Callargs_type = _proxyFile.getRecord(_callArgs.ToArray());
                if (_textjson != null)          declareProxy.Request_type  = _textjson;
                else if (_reqArgs.Count > 0)    declareProxy.Request_type  = _proxyFile.getRecord(_reqArgs.ToArray());

                if (webMethod.n_returns != null) {
                    var expression = webMethod.n_returns[0].n_Expression;

                    if (expression is LTTSQL.Node.IExprResponseNode exprResponseNode)
                        declareProxy.Response_type = _getTypeResponseNode(exprResponseNode);
                    else
                        declareProxy.Response_type = _getType(expression, expression.SqlType);
                }

                return declareProxy;
            }

            private                 void                                    _processSimpleParameter(Node.WEBMETHOD.ServiceParameter parameter)
            {
                string source = parameter.Source;
                string name;
                int    sep = source.IndexOf(':');

                if (sep > 0) {
                    name   = source.Substring(sep + 1);
                    source = source.Substring(0, sep);
                }
                else
                    name = parameter.n_Name.Text.Substring(1);

                switch(source) {
                case "querystring":
                case "textjson": {
                        DeclareType type;

                        if (parameter.n_Type is Node.ComplexType complexType)
                            type = _getAsType(complexType.WebComplexType);
                        else
                            type = _getType(parameter.n_Type, parameter.SqlType);

                        if (parameter.n_Options != null && parameter.n_Options.n_Required)
                            type = _proxyFile.getRequired(type);

                        _addSimpleParameter(source, name, type);
                    }
                    break;
                }
            }
            private                 DeclareType                             _getByJsonScheme(Node.JsonType.JsonSchema.JsonSchemaElement element)
            {
                DeclareType type;

                if (element is Node.JsonType.JsonSchema.JsonSchemaValue value) {
                    if (value.n_Type is Node.ComplexType complexType)
                        type = _getAsType(complexType.WebComplexType);
                    else
                        type = _getType(value.n_Type, ((LTTSQL.Node.Node_Datatype)value.n_Type).SqlType);
                }
                else if (element is Node.JsonType.JsonSchema.JsonSchemaArray array)
                {
                    type = _proxyFile.getSet(_getByJsonScheme(array.n_JsonSchemaElement));
                }
                else if (element is Node.JsonType.JsonSchema.JsonSchemaObject obj)
                {
                    var properties = obj.n_Properties;
                    var fields     = new RecordField[properties.Length];

                    for(int i = 0 ; i < properties.Length ; ++i)
                        fields[i] = new RecordField() { Name=properties[i].n_Name.ValueString, Type=_getByJsonScheme(properties[i].n_JsonSchemaElement) };

                    type = _proxyFile.getRecord(fields);
                }
                else
                    throw new InvalidOperationException("Invalid jsonschema type.");

                if ((element.n_Flags & DataModel.JsonFlags.Required) != 0 && element is Node.JsonType.JsonSchema.JsonSchemaValue)
                    type = _proxyFile.getRequired(type);

                return type;
            }
            private                 DeclareType                             _getTypeResponseNode(LTTSQL.Node.IExprResponseNode exprResponseNode)
            {
                switch(exprResponseNode.ResponseNodeType) {
                case LTTSQL.DataModel.ResponseNodeType.Object:
                case LTTSQL.DataModel.ResponseNodeType.ObjectMandatory:
                    return _getTypeResponseNodeObject(exprResponseNode);

                case LTTSQL.DataModel.ResponseNodeType.ArrayObject:
                    return _proxyFile.getSet(_getTypeResponseNodeObject(exprResponseNode));

                case LTTSQL.DataModel.ResponseNodeType.ArrayValue:
                    var column = exprResponseNode.ResponseColumns[0];
                    return _proxyFile.getSet(_getColumnType(column));

                default:
                    throw new NotImplementedException("ResponseNode " + exprResponseNode.ResponseNodeType + " not implemented.");
                }
            }
            private                 DeclareType                             _getTypeResponseNodeObject(LTTSQL.Node.IExprResponseNode exprResponseNode)
            {
                var columns = exprResponseNode.ResponseColumns;
                var fields  = new RecordField[columns.Length];

                for(int i = 0 ; i < columns.Length ; ++i) {
                    var column = (LTTSQL.Node.Query_Select_ColumnResponse)(columns[i]);
                    var field  = (fields[i] = new RecordField());

                    field.Name = column.n_FieldName.ValueString;
                    field.Type = _getColumnType(column);
                }

                return _proxyFile.getRecord(fields);
            }
            private                 DeclareType                             _getColumnType(LTTSQL.Node.Query_Select_ColumnResponse column)
            {
                if (column.n_Expression is LTTSQL.Node.Expr_ServiceComplexType responseComplexType)
                    return _getAsType((Node.WEBCOMPLEXTYPE)responseComplexType.DeclarationComplexType);

                return (column.n_Expression is LTTSQL.Node.IExprResponseNode columnExprResponseNode)
                                    ? _getTypeResponseNode(columnExprResponseNode)
                                    : _getType(column, column.n_Expression.SqlType);
            }
            private                 DeclareSimpleType                       _getType(object declaration, LTTSQL.DataModel.ISqlType sqlType)
            {
                var typeMapDictionary = _webServiceEmitor.n_TypeMap?.TypeMapDictionary;

                if (typeMapDictionary != null && typeMapDictionary.TryGetValue(sqlType, out var typeMapEntry))
                    return _proxyFile.getSimpleType(typeMapEntry.From, typeMapEntry.Expression);

                if (sqlType is LTTSQL.DataModel.SqlTypeNative nativeType) {
                    switch(nativeType.SystemType) {
                    case LTTSQL.DataModel.SystemType.Bit:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Boolean");

                    case LTTSQL.DataModel.SystemType.TinyInt:
                    case LTTSQL.DataModel.SystemType.SmallInt:
                    case LTTSQL.DataModel.SystemType.Int:
                    case LTTSQL.DataModel.SystemType.BigInt:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Integer");

                    case LTTSQL.DataModel.SystemType.SmallMoney:
                    case LTTSQL.DataModel.SystemType.Money:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Number.subClass({ precision: 2 })");

                    case LTTSQL.DataModel.SystemType.Numeric:
                    case LTTSQL.DataModel.SystemType.Decimal:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Number.subClass({ precision: " + nativeType.Scale + " })");

                    case LTTSQL.DataModel.SystemType.Real:
                    case LTTSQL.DataModel.SystemType.Float:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Number.subClass({ precision: 4 })");

                    case LTTSQL.DataModel.SystemType.Char:
                    case LTTSQL.DataModel.SystemType.NChar:
                    case LTTSQL.DataModel.SystemType.VarChar:
                    case LTTSQL.DataModel.SystemType.NVarChar:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "String.subClass({ maxLength: " + nativeType.MaxLength + " })");

                    case LTTSQL.DataModel.SystemType.Date:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Date");

                    case LTTSQL.DataModel.SystemType.Time:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "Time");

                    case LTTSQL.DataModel.SystemType.SmallDateTime:
                    case LTTSQL.DataModel.SystemType.DateTime:
                    case LTTSQL.DataModel.SystemType.DateTime2:
                        return _proxyFile.getSimpleType("jc3/jannesen.datatype", "DateTime");
                    }
                }

                throw new EmitException(declaration, "No type mapping for '" + sqlType.ToString() + "'.");
            }
            private                 DeclareSimpleType                       _getAsType(Node.WEBCOMPLEXTYPE wct)
            {
                var x = wct.n_As.AsType;
                return _proxyFile.getSimpleType(x.From, x.Expression);
            }
            private                 void                                    _addSimpleParameter(string source, string name, DeclareType type)
            {
                switch(source) {
                case "querystring":
                    _callArgs.Add(new RecordField() { Name = name, Type = type });
                    break;
                case "textjson":
                    _reqArgs.Add(new RecordField() { Name = name, Type = type });
                    break;
                }
            }
        }

        private     readonly        Node.WEBSERVICE_EMITOR_JC_PROXY         _webServiceEmitor;
        private     readonly        string                                  _baseEmitDirectory;
        private     readonly        Dictionary<string, ProxyFile>           _proxyFiles;

        public                                                          JcProxyEmitor(Node.WEBSERVICE_EMITOR_JC_PROXY webServiceEmitor, string baseEmitDirectory)
        {
            _webServiceEmitor  = webServiceEmitor;
            _baseEmitDirectory = baseEmitDirectory;
            _proxyFiles        = new Dictionary<string, ProxyFile>();
        }

        public                  void                                    AddWebMethod(Node.WEBMETHOD webMethod)
        {
            if (webMethod.n_Declaration.JcProxy != null) {
                var filename =  _baseEmitDirectory + "\\" + webMethod.n_Declaration.JcProxy.From.Replace("/", "\\") + ".proxy.ts";

                if (!_proxyFiles.TryGetValue(filename, out var proxyFile)) {
                    proxyFile = new ProxyFile(filename);
                    _proxyFiles.Add(filename, proxyFile);
                }

                proxyFile.AddMethod(_webServiceEmitor, webMethod);
            }
        }
        public                  void                                    AddIndexMethod(string pathname, string procedureName)
        {
        }

        public                  void                                    Emit(EmitContext emitContext)
        {
            foreach(var proxyFileFile in _proxyFiles.Values)
                proxyFileFile.Emit(emitContext);
        }
    }
}
