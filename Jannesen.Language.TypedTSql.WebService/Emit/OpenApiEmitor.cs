using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

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

        public      readonly        string                              Filename;
        public      readonly        Node.WEBSERVICE_EMITOR_OPENAPI      ConfigNode;
        private     readonly        OpenApiDocument                     _openApiDocument;
        private     readonly        Dictionary<object, string>          _typeSchemaMap;

        private     static          ISerializer                         _yamlSerializer = _initSerializer();

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
            _typeSchemaMap = new Dictionary<object, string>(4096);
        }

        public                  void                                    CleanTarget()
        {
            Library.FileHelpers.DeleteFile(Filename);
        }
        public                  void                                    AddWebMethod(Node.WEBMETHOD webMethod)
        {
            if (ConfigNode.n_Path == null || webMethod.n_Name.StartsWith(ConfigNode.n_Path)) {
                var pathItem = _getCreatePathItem(webMethod.n_Name);

                foreach (var method in webMethod.n_Declaration.n_Methods) {
                    var operation = _createOperation(pathItem, method);
                    _setOperationExtendedProperties(operation, webMethod);
                    _processParameters(operation, webMethod.n_Parameters.n_Parameters, method);
                    _createResponse(operation, webMethod.n_Declaration.n_WebHttpHandler, webMethod);
                }
            }
        }
        public                  void                                    AddIndexMethod(string pathname, string procedureName)
        {
        }

        public                  void                                    Emit(EmitContext emitContext)
        {
            using (var fileData = new MemoryStream()) {
                using (var outStream = new StreamWriter(fileData, Encoding.UTF8, 4096, true)) {
                    _yamlSerializer.Serialize(outStream, _openApiDocument);
                }

                FileUpdate.Update(Filename, fileData);
            }
        }

        private                 OpenApiPathItem                         _getCreatePathItem(string name)
        {
            var path = name.Contains("{") ? (new NameNormalizer()).Normalize(name) : "/" + name;

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
        }
        private                 void                                    _processParameters(OpenApiOperation operation, LTTSQL.Node.Node_Parameter[] parameters, string method)
        {
            foreach(Node.WEBMETHOD.ServiceParameter parameter in parameters) {
                var options = parameter.n_Options;

                if (!(options != null && options.n_Key && method == "POST")) {
                    if (parameter.n_Type is Node.JsonType jsonType) {
                        if (parameter.Source != "body:json") {
                            throw new EmitException(parameter, "json only supported with source='body:json'.");
                        }

                        if (operation.requestBody == null) {
                            if (method != "DELETE") {
                                if (operation.requestBody != null) {
                                    throw new EmitException(parameter, "requestbody already declared with textjson:");
                                }

                                operation.requestBody = _openApiBodyContentType("DATA", "application/json", _getOpenApiSchema(parameter.n_Type, jsonType.SqlType.JsonSchema));
                            }
                        }
                    }
                    else {
                        foreach (var x in parameter.Source.Split('|')) {
                            var     sn     = x.Split(':');
                            var     source = sn[0];
                            var     name   = (sn.Length >= 2 ? sn[1] : parameter.n_Name.Text.Substring(1));
                            string  @in;
                            bool    required;

                            switch(source) {
                            case "urlpath":
                                @in = "path";
                                required = parameter.n_Options.n_Required;
                                goto add_parameter;
                            case "querystring":
                                @in = "query";
                                required = true;

add_parameter:                  {
                                    var typeschema = _getOpenApiSchema(parameter, parameter.SqlType);

                                    if (operation.parameters == null) operation.parameters = new OpenApiParameters();
                                    if (operation.parameters.TryGet(@in, name, out var found)) {
                                        if (required) {
                                            if (found.schema != typeschema) {
                                                throw new EmitException(parameter, "parameter already defined with deferent type.");
                                            }

                                            found.required = true;
                                        }
                                    }
                                    else {
                                        operation.parameters.Add(new OpenApiParameter() {
                                                                     @in      = @in,
                                                                     name     = name,
                                                                     required = true,
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

                                _addPropertyToRequest(parameter, operation, name, _getOpenApiSchema(parameter, parameter.SqlType), parameter.n_Options.n_Required);
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
        private                 void                                    _createResponse(OpenApiOperation operation, string handler, Node.WEBMETHOD webMethod)
        {
            operation.responses = new OpenApiResponses();

            switch(handler) {
            case "sql-json2":
                if (webMethod.n_returns != null && webMethod.n_returns.Count > 0) {
                    _addReponse(operation, "application/json", _getOpenApiSchema(webMethod.n_returns));
                }
                else {
                    operation.responses.Add("201", new OpenApiBody() {
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
                    if (schema.required == null) {
                        schema.required = new HashSet<string>();
                    }
                    schema.required.Add(name);
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

        private                 OpenApiSchema                           _getOpenApiSchema(List<Node.RETURNS> returns)
        {
            if (returns.Count == 1) {
                return _getOpenApiSchema(returns[0].n_Expression, returns[0].n_Expression);
            }

            var oneOf = new OpenApiSchemaOneOf() {
                            oneOf = new HashSet<OpenApiSchema>()
                        };

            foreach(var r in returns) {
                oneOf.oneOf.Add(_getOpenApiSchema(r.n_Expression, r.n_Expression));
            }

            return oneOf;
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.Node.IExprNode expression)
        {
            if (expression is LTTSQL.Node.Expr_ServiceComplexType complexType) {
                if (_typeSchemaMap.TryGetValue(complexType.DeclarationComplexType, out var schemaRef)) {
                    return new OpenApiSchemaRef() { @ref = schemaRef };
                }

                var schema = new OpenApiSchemaType() {
                              type       = "object",
                              properties = new OpenApiSchemaProperties()
                           };

                foreach(var column in complexType.ResponseColumns) {
                    if (!column.ResultColumn.isNullable) {
                        if (schema.required == null) {
                            schema.required = new HashSet<string>();
                        }
                        schema.required.Add(column.n_FieldName.ValueString);
                    }
                    schema.properties.Add(column.n_FieldName.ValueString, _getOpenApiSchema(column, column.n_Expression));
                }

                if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0) {
                    if ((complexType.DeclarationComplexType as Node.WEBCOMPLEXTYPE).ReceivesSqlType is LTTSQL.DataModel.EntityTypeUser postUdt) {
                        schema.x_post_schema = _schemaName(postUdt);
                    }

                    return _addOpenApiSchema(declaration, _schemaName(complexType), complexType.DeclarationComplexType, schema);
                }
                else {
                    return schema;
                }
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
        private                 OpenApiSchemaType                       _getOpenApiSchema(LTTSQL.Node.IExprResponseNode exprResponseNode)
        {
            var rtn = new OpenApiSchemaType() {
                          type = "object",
                          properties = new OpenApiSchemaProperties()
                      };

            foreach(var column in exprResponseNode.ResponseColumns) {
                if (!column.ResultColumn.isNullable) {
                    if (rtn.required == null) {
                        rtn.required = new HashSet<string>();
                    }
                    rtn.required.Add(column.n_FieldName.ValueString);
                }
                rtn.properties.Add(column.n_FieldName.ValueString, _getOpenApiSchema(column, column.n_Expression));
            }

            return rtn;
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.ISqlType sqlType)
        {
            if (_typeSchemaMap.TryGetValue(sqlType, out var @ref)) {
                return new OpenApiSchemaRef() { @ref = @ref };
            }

            if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.Json) != 0) {
                return _getOpenApiSchema(declaration, sqlType.JsonSchema);
            }

            if (sqlType is LTTSQL.DataModel.EntityTypeUser     entityTypeUser)
            {
                var udtSchema = _getOpenApiSchema(declaration, entityTypeUser.NativeType);

                if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.Values) != 0) {
                    udtSchema.x_values = new OpenApiX_Values();

                    foreach(var value in sqlType.Values) {
                        var x_value = new OpenApiX_Value() {
                                          name  = value.Name,
                                          value = value.Value
                                      };

                        if (value.Fields != null) {
                            x_value.fields = new Dictionary<string, object>();
                            foreach (var field in value.Fields) {
                                x_value.fields.Add(field.Name, field.Value);
                            }
                        }

                        udtSchema.x_values.Add(x_value);
                    }
                }

                return _addOpenApiSchema(declaration, entityTypeUser, udtSchema);
            }

            if (sqlType is LTTSQL.DataModel.EntityTypeExternal entityTypeExternal)
            {
                return new OpenApiSchemaType() {
                           type      = "string",
                           format    = entityTypeExternal.Name
                       };
            }

            if (sqlType is LTTSQL.DataModel.EntityTypeExtend entityTypeExtend)
            {
                return _addOpenApiSchema(declaration, entityTypeExtend, _getOpenApiSchema(declaration, entityTypeExtend.ParentType));
            }

            if (sqlType is LTTSQL.DataModel.SqlTypeNative      nativeType)
            {
                return _getOpenApiSchema(declaration, nativeType);
            }

            throw new EmitException(declaration, "Can't create openapischema for " + sqlType.GetType().Name + ".");
        }
        private                 OpenApiSchema                           _getOpenApiSchema(object declaration, LTTSQL.DataModel.JsonSchema jsonSchema)
        {
            if (jsonSchema is LTTSQL.DataModel.JsonSchemaObject jsonObject) {
                var schema = new OpenApiSchemaType() {
                                 type       = "object",
                                 properties = new OpenApiSchemaProperties(),
                             };

                foreach(var property in jsonObject.Properties) {
                    schema.properties.Add(property.Name, _getOpenApiSchema(declaration, property.JsonSchema));

                    if (property.JsonSchema is LTTSQL.DataModel.JsonSchemaValue jsv && (jsv.Flags & DataModel.JsonFlags.Required) != 0) {
                        if (schema.required == null) {
                            schema.required   = new HashSet<string>();
                        }
                        schema.required.Add(property.Name);
                    }
                }

                return schema;
            }

            if (jsonSchema is LTTSQL.DataModel.JsonSchemaArray jsonArray) {
                return new OpenApiSchemaType() {
                           type  = "array",
                           items = _getOpenApiSchema(declaration, jsonArray.JsonSchema)
                       };
            }

            if (jsonSchema is LTTSQL.DataModel.JsonSchemaValue jsonValue) {
                return _getOpenApiSchema(declaration, jsonValue.SqlType);
            }

            throw new EmitException(declaration, "Can't create openapischema for " + jsonSchema.GetType().Name + ".");
        }
        private static          OpenApiSchemaType                       _getOpenApiSchema(object declaration, LTTSQL.DataModel.SqlTypeNative nativeType)
        {
            switch(nativeType.SystemType) {
            case LTTSQL.DataModel.SystemType.Bit:
                return new OpenApiSchemaType() {
                            type      = "boolean"
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
                            format    = "base64"
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
        private                 OpenApiSchema                           _addOpenApiSchema(object declaration, LTTSQL.DataModel.EntityType entityType, OpenApiSchema schema)
        {
            if ((ConfigNode.n_Component & Node.WEBSERVICE_EMITOR_OPENAPI.OptimizeComponent.Type) != 0) {
                return _addOpenApiSchema(declaration, _schemaName(entityType), entityType, schema);
            }
            else {
                return schema;
            }
        }
        private                 OpenApiSchemaRef                        _addOpenApiSchema(object declaration, string name, object type, OpenApiSchema schema)
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

                _typeSchemaMap.Add(type,  @ref);

                return new OpenApiSchemaRef() { @ref = @ref };
            }
            catch(Exception err) {
                throw new EmitException(declaration, "Can't create openapi component.schemas.", err);
            }
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
                                                                             { "code",      new OpenApiSchemaType() { type = "string" }
                                                                             },
                                                                             { "details",   new OpenApiSchemaType() {
                                                                                                type        = "object",
                                                                                                properties  = new OpenApiSchemaProperties() {
                                                                                                                  { "class",    new OpenApiSchemaType() { type = "string" } },
                                                                                                                  { "message",  new OpenApiSchemaType() { type = "string" } }
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
        private static          string                                  _schemaName(LTTSQL.Node.Expr_ServiceComplexType complexType)
        {
            return "ct." + complexType.n_Name.ValueString.Replace('/', '.').Replace(':', '.');
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
