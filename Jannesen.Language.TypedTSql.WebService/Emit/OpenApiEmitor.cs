using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.WebService.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.WebService.Node;

namespace Jannesen.Language.TypedTSql.WebService.Emit
{
    internal class OpenApiEmitor: FileEmitor
    {
        private class NameNormalizer
        {
            private         string          _name;
            private         int             _pos;
            private         StringBuilder   _rtn;

            public          string          Normalize(string name)
            {
                _name = name;
                _pos = 0;
                _rtn = new StringBuilder();

                _rtn.Append('/');

                while (_pos < _name.Length) {
                    var c = _name[_pos++];
                    switch(c) {
                    case '\\':  _rtn.Append(_getChar());    break;
                    case '{':   _processArg();              break;
                    default:    _rtn.Append(c);             break;
                    }
                }
                return _rtn.ToString();
            }

            private         void            _processArg()
            {
                _rtn.Append('{');

                for (;;) {
                    var c = _getChar();
                    switch(c) {
                    case '\\':  _rtn.Append(_getChar());                    break;
                    case '}':                       _rtn.Append('}');       return;
                    case ':':   _processRegex();    _rtn.Append('}');       return;
                    default:    _rtn.Append(c);                              break;
                    }
                }
            }
            private         void            _processRegex()
            {
                for (;;) {
                    var c = _getChar();
                    switch(c) {
                    case '\\':  _getChar();                 break;
                    case '}':                               return;
                    case '(':   _processRegexSection();     break;
                    case '[':   _processRegexSet();         break;
                    case '{':   _processRegexRange();       break;
                    }
                }
            }
            private         void            _processRegexSection()
            {
                for (;;) {
                    var c = _getChar();
                    switch(c) {
                    case '\\':  _getChar();                 break;
                    case ')':                               return;
                    case '(':   _processRegexSection();     break;
                    case '{':   _processRegexRange();       break;
                    case '[':   _processRegexSet();         break;
                    }
                }
            }
            private         void            _processRegexSet()
            {
                for (;;) {
                    var c = _getChar();
                    switch(c) {
                    case '\\':  _getChar();                 break;
                    case ']':                               return;
                    }
                }
            }
            private         void            _processRegexRange()
            {
                for (;;) {
                    var c = _getChar();
                    switch(c) {
                    case '}':
                        return;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case ',':
                        break;
                    default:
                        throw new FormatException("Invalid name (invalid char in regex-range)");
                    }
                }
            }

            private         char            _getChar()
            {
                if (_pos >= _name.Length)
                    throw new FormatException("Invalid name (unexpected end)");

                return _name[_pos++];
            }
        }

        public      readonly        string                                  Filename;
        public      readonly        Node.WEBSERVICE_EMITOR_OPENAPI           ConfigNode;

        private     readonly        OpenApiDocument                         _openApiDocument;
        private     readonly        Dictionary<object, OpenApiSchemaRef>    _typeSchemaMap;
        private     static          ISerializer                             _yamlSerializer = _initSerializer();

        public                                                          OpenApiEmitor(Node.WEBSERVICE_EMITOR_OPENAPI configNode, string baseEmitDirectory)
        {
            Filename           = System.IO.Path.Combine(baseEmitDirectory, configNode.n_File);
            ConfigNode         = configNode;
            _openApiDocument   = new OpenApiDocument() {
                                     openapi = "3.0.0",
                                     info = new OpenApiInfo() {
                                         title   = configNode.n_Title,
                                         version = configNode.n_Version
                                     },
                                     paths = new OpenApiPaths()
                                 };
            _typeSchemaMap     = new Dictionary<object, OpenApiSchemaRef>(16384);
        }

        public                  void                                    CleanTarget()
        {
            Library.FileHelpers.DeleteFile(Filename);
        }
        public                  void                                    AddWebMethod(Node.WEBMETHOD webMethod)
        {
            if (ConfigNode.n_Path == null || webMethod.n_Name.StartsWith(ConfigNode.n_Path)) {
                var path     = _getPath(webMethod);
                var pathItem = _getCreatePathItem(path);

                foreach (var method in webMethod.n_Declaration.n_Methods) {
                    var operation = _createOperation(pathItem, method);
                    _setOperationExtendedProperties(operation, webMethod);
                    _processParameters(operation, webMethod.n_Parameters.n_Parameters, method);
                    _createResponse(operation, webMethod.n_Declaration.n_WebHttpHandler, webMethod, method);
                    _setAttributes(operation, webMethod, path);

                    if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Object) != 0) {
                        if (operation.requestBody != null) {
                            _optimizeBody(operation.requestBody);
                        }
                        if (operation.responses.TryGetValue("200", out var response)) {
                            _optimizeBody(response);
                        }
                    }
                }
            }
        }
        public                  void                                    AddIndexMethod(string pathname, string procedureName)
        {
        }
        public                  void                                    Emit(EmitContext emitContext)
        {
            using (var fileData = new MemoryStream()) {
                using (var outStream = new StreamWriter(fileData, new UTF8Encoding(false), 4096, true)) {
                    outStream.NewLine = "\n";
                    _yamlSerializer.Serialize(outStream, _openApiDocument);
                }

                FileUpdate.Update(Filename, fileData);
            }
        }

        private                 OpenApiPathItem                         _getCreatePathItem(string path)
        {
            if (!_openApiDocument.paths.TryGetValue(path, out var pathItem)) {
                pathItem = new OpenApiPathItem();
                _openApiDocument.paths.Add(path, pathItem);
            }
            return pathItem;
        }
        private static          OpenApiOperation                        _createOperation(OpenApiPathItem pathItem, string method)
        {
            var operation = new OpenApiOperation();

            switch(method) {
            case "GET":
                if (pathItem.get != null) throw new Exception("Operation '" + method + "' already defined.");
                pathItem.get = operation;
                break;
            case "PUT":
                if (pathItem.put != null) throw new Exception("Operation '" + method + "' already defined.");
                pathItem.put = operation;
                break;
            case "POST":
                if (pathItem.post != null) throw new Exception("Operation '" + method + "' already defined.");
                pathItem.post = operation;
                break;
            case "DELETE":
                if (pathItem.delete != null) throw new Exception("Operation '" + method + "' already defined.");
                pathItem.delete = operation;
                break;
            default:
                throw new Exception("Unsupported operation '" + method + "'.");
            }

            return operation;
        }
        private static          void                                    _setOperationExtendedProperties(OpenApiOperation operation, Node.WEBMETHOD webMethod)
        {
            var timeout = webMethod.n_Declaration.GetWebHandlerOptionValueByName("timeout");
            operation.x_timeout = timeout != null ? int.Parse(timeout) : 30;
            operation.x_kind    = webMethod.n_Declaration.n_Kind;
        }
        private                 void                                    _processParameters(OpenApiOperation operation, LTTSQL.Node.Node_Parameter[] parameters, string method)
        {
            foreach(Node.WEBMETHOD.ServiceParameter parameter in parameters) {
                var options = parameter.n_Options;

                if (!(options != null && options.n_Security) &&
                    !(options != null && options.n_Key && method == "POST")) {
                    if (parameter.n_Type is Node.JsonType jsonType) {
                        if (parameter.Source != "body:json") {
                            throw new EmitException(parameter, "json only supported with source='body:json'.");
                        }

                        if (operation.requestBody == null) {
                            if (method != "DELETE") {
                                if (operation.requestBody != null) {
                                    throw new EmitException(parameter, "requestbody already declared with textjson:");
                                }

                                operation.requestBody = _openApiBodyContentType("DATA", "application/json", _getOpenApiSchema(parameter));
                            }
                        }
                    }
                    else {
                        var sources             = parameter.Source.Split('|');
                        var parameterIsOptional = sources.Length > 1 || parameter.Parameter?.DefaultValue != default;
                        
                        foreach (var x in sources) {
                            var     sn     = x.Split(':');
                            var     source = sn[0];
                            var     name   = (sn.Length >= 2 ? sn[1] : parameter.n_Name.Text.Substring(1));
                            string  @in;
                            bool    required = !parameterIsOptional && parameter.n_Options.n_Required;

                            switch(source) {
                            case "urlpath":
                                @in = "path";
                                goto add_parameter;
                            case "querystring":
                                @in = "query";
add_parameter:                  {
                                    var typeschema = _getOpenApiSchema(parameter);
                                    if (parameter.Parameter?.DefaultValue != default) {
                                        if (typeschema is OpenApiSchemaType s) {
                                            s.@default = parameter.Parameter.DefaultValue;
                                        }
                                    }

                                    if (operation.parameters == null) operation.parameters = new OpenApiParameters();
                                    if (operation.parameters.TryGet(@in, name, out var found)) {
                                        if (required) {
                                            if (found.schema != typeschema) {
                                                throw new EmitException(parameter, "parameter already defined with different type.");
                                            }

                                            found.required = true;
                                        }
                                    }
                                    else {
                                        operation.parameters.Add(new OpenApiParameter() {
                                                                     @in      = @in,
                                                                     name     = name,
                                                                     required = required,
                                                                     schema   = typeschema
                                                                 });
                                    }
                                }
                                break;

                            case "textjson":
                                if (operation.requestBody == null) {
                                    operation.requestBody = new OpenApiBody() {
                                                                description = "DATA",
                                                                required    = true,
                                                                content     = new OpenApiContentTypes() {
                                                                                    { "application/json", new OpenApiContent() {
                                                                                                            schema = new OpenApiSchemaType() {
                                                                                                                            type = "object",
                                                                                                                            properties = new OpenApiSchemaProperties()
                                                                                                            }
                                                                                                        }
                                                                                    }
                                                                                }
                                                            };
                                }

                                _addPropertyToRequest(parameter, operation, name, _getOpenApiSchema(parameter), !parameterIsOptional && parameter.n_Options.n_Required);
                                break;

                            case "header":
                                switch(name) {
                                case "basic-username":
                                    _addSecurityScheme(operation,
                                                       "BasicAuth",
                                                       () => new OpenApiSecurityScheme() {
                                                                 type   = SecuritySchemeType.http,
                                                                 scheme = "basic"
                                                             });
                                    break;

                                case "X-APIKEY":
                                case "X-APPKEY":
                                    _addSecurityScheme(operation,
                                                       "ApiKeyAuth-" + name.Substring(2),
                                                       () => new OpenApiSecurityScheme() {
                                                                 type  = SecuritySchemeType.apiKey,
                                                                 @in   = "header",
                                                                 name  = name
                                                             });
                                    break;

                                }
                                break;

                            case "textjsonxml":
                                if (operation.requestBody == null && method != "DELETE") {
                                    operation.requestBody = _openApiBodyContentType("DATA", "application/json", null);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        private                 void                                    _createResponse(OpenApiOperation operation, string handler, Node.WEBMETHOD webMethod, string method)
        {
            operation.responses = new OpenApiResponses();

            switch(handler) {
            case "sql-json2":
                if (method != "DELETE" && webMethod.n_Returns != null && webMethod.n_Returns.Count > 0) {
                    _addReponse(operation, "application/json", _getOpenApiSchema(webMethod.n_Returns));
                }
                else {
                    operation.responses.Add("200", new OpenApiBody() {
                                                       description = "Accepted"
                                                   });
                }

                if (webMethod.n_Declaration.n_WebHandlerOptions?.FindOption("error-handler") == null) {
                    _addErrorResponse(operation);
                }
                break;

            case "sql-json":
                _addReponse(operation, "application/json");
                break;

            case "sql-raw":
                _addReponse(operation, "*");
                break;

            case "sql-xml":
                _addReponse(operation, "text/xml");
                break;

            case "sql-excelexport":
                _addReponse(operation, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                break;
            }
        }
        private                 void                                    _addPropertyToRequest(object declaration, OpenApiOperation operation, string sjpath, OpenApiSchema typeschema, bool required)
        {
            var schema = operation.requestBody.content["application/json"].schema as OpenApiSchemaType;
            if (schema?.type != "object") {
                throw new EmitException(declaration, "requestbody is not a object");
            }

            var jpath = sjpath.Split('.');

            for (int i=0 ; i < jpath.Length ; ++i) {
                var name = jpath[i];
                OpenApiSchemaType nextSchema = null;

                if (i < jpath.Length - 1) {
                    if (schema.properties.TryGetValue(name, out var p)) {
                        if (p is OpenApiSchemaType st && st.type == "object") {
                            nextSchema = st;
                        }
                        else {
                            throw new EmitException(declaration, "property '" + name + "'already defined as non object.");
                        }
                    }
                    else {
                        nextSchema = new OpenApiSchemaType() {
                                         type       = "object",
                                         properties = new OpenApiSchemaProperties()
                                     };
                        schema.properties.Add(name, nextSchema);
                    }
                }
                else {
                    if (schema.properties.TryGetValue(name, out var p)) {
                        if (p != typeschema) {
                            throw new EmitException(declaration, "property already defined with deferent type.");
                        }
                    }
                    else {
                        schema.properties.Add(name, typeschema);
                    }
                }

                if (required) {
                    schema.AddRequired(name);
                }

                schema = nextSchema;
            }
        }
        private                 void                                    _addSecurityScheme(OpenApiOperation operation, string name, Func<OpenApiSecurityScheme> create)
        {
            if (_openApiDocument.components == null) {
                _openApiDocument.components = new OpenApiComponents();
            }

            if (_openApiDocument.components.securitySchemes == null) {
                _openApiDocument.components.securitySchemes = new OpenApiSecuritySchemes();
            }

            if (!_openApiDocument.components.securitySchemes.TryGetValue(name, out var _)) {
                _openApiDocument.components.securitySchemes.Add(name, create());
            }

            if (operation.security == null) {
                operation.security = new OpenApiSecurityList();
            }

            operation.security.Add(new OpenApiSecurity() {
                                       name    = name,
                                       options = new string[0]
                                   });
        }
        private                 void                                    _setAttributes(OpenApiOperation operation, Node.WEBMETHOD webMethod, string path)
        {
            if (webMethod.n_Declaration.n_Attributes?.Attributes != null) {
                foreach (var a in webMethod.n_Declaration.n_Attributes?.Attributes) {
                    operation.SetAttribute(a.Attr.Name, a.Value);
                }
            }

            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0 &&
                (ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Logical) == 0) {
                var selectValueType = _getSelectValueType(webMethod);
                if (selectValueType != null) {
                    if (!(_typeSchemaMap.TryGetValue(selectValueType, out var schemaRef) && schemaRef.schema is OpenApiSchemaType schema)) {
                        throw new EmitException(webMethod.n_Declaration.n_ServiceMethodName, "Can't find related schema for select-value.");
                    }

                    schema.SetAttribute("x-" + webMethod.n_Declaration.n_Kind, _getPath(webMethod));
                }
            }
        }

        private                 OpenApiSchema                           _getOpenApiSchema(Node.WEBMETHOD.ServiceParameter parameter)
        {
            if (parameter.n_Type is Node.JsonType jsonType) {
                return _getOpenApiSchema(parameter, jsonType.n_Schema.n_Schema);
            }

            if (parameter.n_Type is Node.ComplexType complexType && (ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Logical) != 0) {
                return _getOpenApiSchema(parameter, complexType.WebComplexType);
            }

            return _getOpenApiSchema(parameter, parameter.SqlType);
        }
        private                 OpenApiSchema                           _getOpenApiSchema(List<Node.RETURNS> returns)
        {
            if (returns.Count == 1) {
                return _getOpenApiSchema(returns[0].n_Expression, returns[0].n_Expression);
            }

            var oneOf = new OpenApiSchemaOneOf() {
                            oneOf = new ComparableHashSet<OpenApiSchema>()
                        };

            foreach(var r in returns) {
                oneOf.oneOf.Add(_getOpenApiSchema(r.n_Expression, r.n_Expression));
            }

            return (oneOf.oneOf.Count == 1) ? oneOf.oneOf.First() : oneOf;
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.Node.IExprNode expression)
        {
            if (expression is LTTSQL.Node.Expr_ServiceComplexType complexType) {
                return _getOpenApiSchema(declaration, (Node.WEBCOMPLEXTYPE)(complexType.DeclarationComplexType));
            }

            if (expression is LTTSQL.Node.IExprResponseNode exprResponseNode) {
                switch(exprResponseNode.ResponseNodeType) {
                case LTTSQL.DataModel.ResponseNodeType.Object:
                case LTTSQL.DataModel.ResponseNodeType.ObjectMandatory:
                    return _getOpenApiSchema(exprResponseNode);

                case LTTSQL.DataModel.ResponseNodeType.ArrayObject:
                    return new OpenApiSchemaType() {
                               type  = "array",
                               items = _getOpenApiSchema(exprResponseNode)
                           };

                case LTTSQL.DataModel.ResponseNodeType.ArrayValue:
                    return new OpenApiSchemaType() {
                               type  = "array",
                               items = _getOpenApiSchema(exprResponseNode.ResponseColumns[0], exprResponseNode.ResponseColumns[0].n_Expression)
                           };

                default:
                    throw new NotImplementedException("ResponseNode " + exprResponseNode.ResponseNodeType + " not implemented.");
                }
            }
            else {
                return _getOpenApiSchema(declaration, expression.SqlType);
            }
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, Node.WEBCOMPLEXTYPE complexType)
        {
            if (_typeSchemaMap.TryGetValue(complexType, out var schemaRef)) {
                return schemaRef;
            }

            OpenApiSchemaWithAttributes   schema;

            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Logical) != 0) {
                var responseSchema = _getOpenApiSchema(complexType.ResponseNode);
                var requestSchema  = complexType.n_Receives != null
                                        ? _getOpenApiSchema(declaration, complexType.n_Receives)
                                        : _getOpenApiSchema(declaration, complexType.n_Parameters.n_Parameters[0].Parameter?.SqlType);

                if (requestSchema != null) {
                    var logicalSchema = new OpenApiSchemaOneOf() { oneOf = new ComparableHashSet<OpenApiSchema>() { requestSchema, responseSchema } };
                    logicalSchema.SetAttribute("x-oneOf-mapping", "Request, Response");
                    schema = logicalSchema;
                }
                else {
                    schema = responseSchema;
                }
            }
            else {
                var responseSchema = _getOpenApiSchema(complexType.ResponseNode);
                if (complexType.ReceivesSqlType is LTTSQL.DataModel.EntityTypeUser postUdt) {
                    responseSchema.SetAttribute("x-value-schema", _getOpenApiSchema(declaration, postUdt));
                }
                schema = responseSchema;
            }

            var attributes = complexType.n_Attributes;
            if (attributes != null) {
                foreach(var a in attributes.Attributes) {
                    schema.SetAttribute(a.Attr.Name, a.Value);
                }
            }

            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0) {
                return _addOpenApiSchema(_schemaName(complexType), complexType, schema);
            }
            else {
                return schema;
            }
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, Node.WEBCOMPLEXTYPE.Receives recieves)
        {
            if (recieves.n_ReceivesType == null) {
                return null;
            }

            if (recieves.n_ReceivesType is JsonType jsonType) {
                return _getOpenApiSchema(declaration, jsonType.n_Schema.n_Schema);
            }

            if (recieves.n_ReceivesType is Node.ComplexType complexType) {
                return _getOpenApiSchema(declaration, complexType.WebComplexType);
            }

            return _getOpenApiSchema(declaration,  recieves.SqlType);
        }
        private                 OpenApiSchemaType                       _getOpenApiSchema(LTTSQL.Node.IExprResponseNode exprResponseNode)
        {
            var rtn = new OpenApiSchemaType() {
                          type = "object",
                          properties = new OpenApiSchemaProperties()
                      };

            foreach(var column in exprResponseNode.ResponseColumns) {
                if (!column.ResultColumn.isNullable) {
                    rtn.AddRequired(column.n_FieldName.ValueString);
                }
                rtn.properties.Add(column.n_FieldName.ValueString, _getOpenApiSchema(column, column.n_Expression));
            }

            return rtn;
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.ISqlType sqlType)
        {
            if (sqlType is LTTSQL.DataModel.EntityTypeUser entityTypeUser) {
                return _getOpenApiSchema(declaration, entityTypeUser);
            }

            if (sqlType is LTTSQL.DataModel.EntityTypeExternal entityTypeExternal) {
                return _getOpenApiSchema(declaration, entityTypeExternal);
            }

            if (sqlType is LTTSQL.DataModel.EntityTypeExtend entityTypeExtend) {
                return _getOpenApiSchema(declaration, entityTypeExtend);
            }

            if (sqlType is LTTSQL.DataModel.SqlTypeNative nativeType) {
                return _getOpenApiSchema(declaration, nativeType);
            }

            throw new EmitException(declaration, "Can't create openapischema for " + sqlType.GetType().Name + ".");
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.EntityTypeUser entityTypeUser)
        {
            if (_typeSchemaMap.TryGetValue(entityTypeUser, out var schemaRef)) {
                return schemaRef;
            }

            var udtSchema = _getOpenApiSchema(declaration, entityTypeUser.NativeType);

            if (entityTypeUser.TimeZone != null) {
                udtSchema.SetAttribute("x-timezone", entityTypeUser.TimeZone);
            }

            if (entityTypeUser.Attributes != null) {
                foreach(var attr in entityTypeUser.Attributes) {
                    udtSchema.SetAttribute(attr.Attr.Name, attr.Value);
                }
            }


            if (entityTypeUser.Values != null && entityTypeUser.Values.hasPublic()) {
                var x_values = new OpenApiX_Values();

                bool select_static = udtSchema.GetAttribute("x-select-source") is string s && s == "static";

                foreach(var value in entityTypeUser.Values) {
                    if (value.Public) {
                        var x_value = new OpenApiX_Value() {
                                            name  = value.Name,
                                            value = value.Value
                                        };

                        if (select_static && value.Fields != null) {
                            x_value.fields = new ComparableDictionary<string, object>();
                            foreach (var field in value.Fields) {
                                x_value.fields.Add(field.Name, field.Value);
                            }
                        }

                        x_values.Add(x_value);
                    }
                }

                udtSchema.SetAttribute("x-values", x_values);
            }

            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0) {
                return _addOpenApiSchema(_schemaName(entityTypeUser), entityTypeUser, udtSchema);
            }
            else {
                return udtSchema;
            }
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.EntityTypeExternal entityTypeExternal)
        {
                return new OpenApiSchemaType() {
                           type      = "string",
                           format    = entityTypeExternal.Name
                       };
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.EntityTypeExtend entityTypeExtend)
        {
            if (_typeSchemaMap.TryGetValue(entityTypeExtend, out var schemaRef)) {
                return schemaRef;
            }

            var etschema = _getOpenApiSchema(declaration, entityTypeExtend.ParentType);
            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0) {
                return _addOpenApiSchema(_schemaName(entityTypeExtend), entityTypeExtend, etschema);
            }
            else {
                return etschema;
            }
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, Node.JsonType.JsonSchema.JsonSchemaElement element)
        {
            if (element is Node.JsonType.JsonSchema.JsonSchemaValue value) {
                if (value.n_Type is Node.ComplexType complexType) {
                    if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Logical) == 0) {
                        if (complexType.WebComplexType.n_Receives == null) {
                            return _getOpenApiSchema(declaration, value.SqlType);
                        }

                        if (complexType.WebComplexType.n_Receives.n_ReceivesType != null) {
                           return _getOpenApiSchema(declaration, complexType.WebComplexType.n_Receives);
                        }
                    }

                    return _getOpenApiSchema(declaration, complexType.WebComplexType);
                }

                return _getOpenApiSchema(declaration, value.SqlType);
            }

            if (element is Node.JsonType.JsonSchema.JsonSchemaObject obj)
            {
                var schema = new OpenApiSchemaType() {
                                 type       = "object",
                                 properties = new OpenApiSchemaProperties(),
                             };

                foreach(var property in obj.n_Properties) {
                    schema.properties.Add(property.n_Name.ValueString,
                                          _getOpenApiSchema(declaration, property.n_JsonSchemaElement));

                    if ((property.n_JsonSchemaElement.n_Flags & DataModel.JsonFlags.Required) != 0) {
                        schema.AddRequired(property.n_Name.ValueString);
                    }
                }

                return schema;
            }

            if (element is Node.JsonType.JsonSchema.JsonSchemaArray array)
            {
                return new OpenApiSchemaType() {
                           type  = "array",
                           items = _getOpenApiSchema(declaration, array.n_JsonSchemaElement)
                       };
            }

            throw new InvalidOperationException("Invalid jsonschema type.");
        }
        private static          OpenApiSchemaType                       _getOpenApiSchema(object declaration, LTTSQL.DataModel.SqlTypeNative nativeType)
        {
            switch(nativeType.SystemType) {
            case LTTSQL.DataModel.SystemType.Bit:
                return new OpenApiSchemaType() {
                            type      = "boolean",
                        };

            case LTTSQL.DataModel.SystemType.TinyInt:
            case LTTSQL.DataModel.SystemType.SmallInt:
            case LTTSQL.DataModel.SystemType.Int:
            case LTTSQL.DataModel.SystemType.BigInt:
                return new OpenApiSchemaType() {
                            type      = "integer"
                        };

            case LTTSQL.DataModel.SystemType.SmallMoney:
            case LTTSQL.DataModel.SystemType.Money:
            case LTTSQL.DataModel.SystemType.Numeric:
            case LTTSQL.DataModel.SystemType.Decimal:
            case LTTSQL.DataModel.SystemType.Real:
            case LTTSQL.DataModel.SystemType.Float:
                return new OpenApiSchemaType() {
                            type      = "number"
                        };

            case LTTSQL.DataModel.SystemType.Char:
            case LTTSQL.DataModel.SystemType.NChar:
            case LTTSQL.DataModel.SystemType.VarChar:
            case LTTSQL.DataModel.SystemType.NVarChar:
                return new OpenApiSchemaType() {
                            type      = "string",
                            maxLength = (nativeType.MaxLength>0) ? (int?)nativeType.MaxLength : null
                        };

            case LTTSQL.DataModel.SystemType.Binary:
            case LTTSQL.DataModel.SystemType.VarBinary:
                return new OpenApiSchemaType() {
                            type      = "string",
                            format    = "byte"
                        };

            case LTTSQL.DataModel.SystemType.Date:
                return new OpenApiSchemaType() {
                            type      = "string",
                            format    = "date"
                        };

            case LTTSQL.DataModel.SystemType.Time:
                return new OpenApiSchemaType() {
                            type      = "string",
                            format    = "time"
                        };

            case LTTSQL.DataModel.SystemType.SmallDateTime:
            case LTTSQL.DataModel.SystemType.DateTime:
            case LTTSQL.DataModel.SystemType.DateTime2:
                return new OpenApiSchemaType() {
                            type      = "string",
                            format    = "date-time"
                        };
            default:
                throw new EmitException(declaration, "No type mapping for '" + nativeType.ToString() + "'.");
            }
        }
        private                 OpenApiSchemaRef                        _addOpenApiSchema(string name, object type, OpenApiSchema schema)
        {
            try {
                var @ref = "#/components/schemas/" + name;

                if (_openApiDocument.components == null) {
                    _openApiDocument.components = new OpenApiComponents();
                }
                if (_openApiDocument.components.schemas == null) {
                    _openApiDocument.components.schemas = new OpenApiSchemas();
                }

                _openApiDocument.components.schemas.Add(name, schema);

                var r = new OpenApiSchemaRef() { @ref = @ref, schema = schema };

                _typeSchemaMap.Add(type,  r);

                return r;
            }
            catch(Exception err) {
                throw new InvalidOperationException("Can't create openapi component.schemas." + name, err);
            }
        }
        private                 void                                    _optimizeBody(OpenApiBody body)
        {
            if (body.content != null && body.content.TryGetValue("application/json", out var content)) {
                var r = _getUniqueSchema(content.schema);
                if (r != null) {
                    content.schema = r;
                }
            }
        }
        private                 OpenApiSchema                           _getUniqueSchema(OpenApiSchema schema)
        {
            if (schema is OpenApiSchemaOneOf schemaOneOf) {
                return null;
            }

            if (schema is OpenApiSchemaType schemaType) {
                switch(schemaType.type) {
                case "array": {
                        var r = _getUniqueSchema(schemaType.items);
                        if (r != null) {
                            schemaType.items = r;
                        }
                    }
                    break;

                case "object":
                    foreach(var k in schemaType.properties.Keys.ToArray()) {
                        var r = _getUniqueSchema(schemaType.properties[k]);
                        if (r != null) {
                            schemaType.properties[k] = r;
                        }
                    }

                    if (!_typeSchemaMap.TryGetValue(schemaType, out var ro)) {
                        var name = "object-" + ((uint)(schemaType.GetHashCode())).ToString();

                        if (_openApiDocument.components.schemas.ContainsKey(name)) {
                            name += "-" + _typeSchemaMap.Count.ToString();
                        }
                        ro = _addOpenApiSchema(name, schemaType, schemaType);
                    }

                    return ro;
                }

                return null;
            }

            return null;
        }
        private                 OpenApiSchemaRef                        _getErrorSchema()
        {
            if (_openApiDocument.components == null) {
                _openApiDocument.components = new OpenApiComponents();
            }
            if (_openApiDocument.components.schemas == null) {
                _openApiDocument.components.schemas = new OpenApiSchemas();
            }

            if (!_openApiDocument.components.schemas.ContainsKey("STANDARD500ERROR")) {
                _openApiDocument.components.schemas.Add("STANDARD500ERROR",
                                                        new OpenApiSchemaType() {
                                                            type       = "object",
                                                            properties = new OpenApiSchemaProperties() {
                                                                             { "code",      new OpenApiSchemaType() { type = "string" } },
                                                                             { "details",   new OpenApiSchemaType() {
                                                                                                type       = "array",
                                                                                                items      = new OpenApiSchemaType() {
                                                                                                                  type        = "object",
                                                                                                                  properties  = new OpenApiSchemaProperties() {
                                                                                                                                    { "class",    new OpenApiSchemaType() { type = "string" } },
                                                                                                                                    { "message",  new OpenApiSchemaType() { type = "string" } }
                                                                                                                                }
                                                                                                               }
                                                                                               }
                                                                              }
                                                            }
                                                        });
            }

            return new OpenApiSchemaRef() {
                       @ref="#/components/schemas/STANDARD500ERROR"
                   };
        }
        private static          void                                    _addReponse(OpenApiOperation operation, string contentType, OpenApiSchema schema = null)
        {
            operation.responses.Add("200", _openApiBodyContentType("OK", contentType, schema));
        }
        private                 void                                    _addErrorResponse(OpenApiOperation operation)
        {
            operation.responses.Add("500", _openApiBodyContentType("There was an error on the server-side",
                                                                   "application/json",
                                                                   _getErrorSchema()));
        }
        private static          OpenApiBody                             _openApiBodyContentType(string description, string contentType, OpenApiSchema schema)
        {
            return new OpenApiBody() {
                       description = description,
                       content = new OpenApiContentTypes() {
                                     {  contentType, new OpenApiContent() {
                                                         schema = schema
                                                     }
                                     }
                                 }
                   };
        }
        private static          string                                  _schemaName(Node.WEBCOMPLEXTYPE complexType)
        {
            return "ct." + complexType.n_Declaration.n_ServiceTypeName.n_Name.ValueString.Replace('/', '.').Replace(':', '.');
        }
        private static          string                                  _schemaName(LTTSQL.DataModel.EntityType entityType)
        {
            var rtn = "ut.";
            if (entityType.EntityName.Schema != "dbo") {
                rtn += entityType.EntityName.Schema;
                rtn += ".";
            }
            rtn += entityType.EntityName.Name.Replace('/', '.').Replace(':', '.');
            return rtn;
        }
        private static          DataModel.EntityTypeUser                _getSelectValueType(Node.WEBMETHOD webMethod)
        {
            switch(webMethod.n_Declaration.n_Kind) {
            case "select-lookup": {
                    DataModel.EntityTypeUser found  = null;
                    int                             n      = 0;

                    foreach(WEBMETHOD.ServiceParameter p in webMethod.n_Parameters.n_Parameters) {
                        if (!(p.n_Options != null && p.n_Options.n_Security)) {
                            if (p.Parameter.SqlType is DataModel.EntityTypeUser entityTypeUser) {
                                found = entityTypeUser;
                            }
                            ++n;
                        }
                    }

                    if (found != null && n == 1) {
                        return found;
                    }

                    throw new EmitException(webMethod.n_Declaration, "Can't determin select-type for 'select-lookup'");
                }

            case "select-search": {
                    foreach(var r in webMethod.n_Returns) {
                        if (r.SqlType is LTTSQL.DataModel.SqlTypeResponseNode reponseNodeType) {
                            switch(reponseNodeType.NodeType) {
                            case LTTSQL.DataModel.ResponseNodeType.ArrayValue: {
                                    if (reponseNodeType.Columns?[0].SqlType?.Columns?[0].SqlType is LTTSQL.DataModel.EntityTypeUser entityTypeUser) {
                                        return entityTypeUser;
                                    }
                                }
                                break;

                            case LTTSQL.DataModel.ResponseNodeType.ArrayObject: {
                                    if (reponseNodeType.Columns?[0]?.SqlType is LTTSQL.DataModel.EntityTypeUser entityTypeUser) {
                                        return entityTypeUser;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    throw new EmitException(webMethod.n_Declaration, "Can't determin select-type for 'select-search'");
                }

            default:
                return null;
            }
        }
        private static          string                                  _getPath(Node.WEBMETHOD webMethod)
        {
            return webMethod.n_Name.Contains("{") ? (new NameNormalizer()).Normalize(webMethod.n_Name) : "/" + webMethod.n_Name;
        }

        private static          ISerializer                             _initSerializer()
        {
            var builder = (new SerializerBuilder())
                                .WithQuotingNecessaryStrings()
                                .WithNewLine("\n")
                                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
                                .DisableAliases();
            return builder.Build();
        }
    }
}
