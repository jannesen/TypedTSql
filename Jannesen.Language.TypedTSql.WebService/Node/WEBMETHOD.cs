using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    [LTTSQL.Library.DeclarationParser("WEBMETHOD")]
    public class WEBMETHOD: LTTSQL.Node.DeclarationServiceMethod, LTTSQL.Node.IParseContext
    {
        public class ServiceDeclaration: LTTSQL.Core.AstParseNode
        {
            public class OptionValue: LTTSQL.Core.AstParseNode
            {
                public      string                                              n_Name          { get; private set; }
                public      string                                              n_Value         { get; private set; }

                internal                                                        OptionValue(LTTSQL.Core.ParserReader reader)
                {
                    n_Name = Core.TokenWithSymbol.SetNoSymbol(ParseName(reader)).ValueString;
                    ParseToken(reader, Core.TokenID.Equal);
                    n_Value = ParseToken(reader, Core.TokenID.String).ValueString;
                }
                public      override        void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    switch(n_Name)
                    {
                    case "timeout":
                        if (!int.TryParse(n_Value, out var value) || value < 5 || value > 300)
                            context.AddError(this, "Invalid timeout value.");
                        break;
                    }
                }
            }
            public      readonly        LTTSQL.Node.Node_ServiceEntityName  n_ServiceMethodName;
            public      readonly        string[]                            n_Methods;
            public      readonly        string                              n_HttpHandler;
            public      readonly        LTTSQL.Core.Token                   n_Proxy;
            public      readonly        OptionValue[]                       n_Options;
            public      readonly        string[]                            n_DependentAssembly;
            public      readonly        XmlElement                          n_HandlerConfig;
            public      readonly        LTTSQL.DataModel.EntityName         n_EntityName;
            public                      Emit.FromExpression                 Proxy               { get; private set; }

            public                                                          ServiceDeclaration(LTTSQL.Core.ParserReader reader)
            {
                List<OptionValue>       options = null;
                List<string>            dependentAssembly = null;

                ParseToken(reader, "WEBMETHOD");
                n_ServiceMethodName = AddChild(new LTTSQL.Node.Node_ServiceEntityName(reader));
                ParseToken(reader, LTTSQL.Core.TokenID.METHOD);

                {
                    var methods = new List<string>();

                    do {
                        methods.Add(ParseToken(reader, Core.TokenID.String).ValueString.ToUpper());
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_Methods = methods.ToArray();
                }

                switch(Core.TokenWithSymbol.SetKeyword(ParseToken(reader, "HANDLER", "PROXY")).Text.ToUpper())
                {
                case "HANDLER":
                    n_HttpHandler = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "PROXY":
                    n_HttpHandler = "sql-json2";
                    n_Proxy = ParseToken(reader, LTTSQL.Core.TokenID.String);
                    break;
                }

                while (reader.CurrentToken.isToken(LTTSQL.Core.TokenID.QuotedName)) {
                    if (options == null)
                        options = new List<OptionValue>();

                    options.Add(AddChild(new OptionValue(reader)));
                }

                while (reader.CurrentToken.isToken(LTTSQL.Core.TokenID.DEPENDENTASSEMBLY)) {
                    ParseToken(reader);
                    if (dependentAssembly == null)
                        dependentAssembly = new List<string>();

                    dependentAssembly.Add(ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString);
                }

                if (reader.CurrentToken.isToken(LTTSQL.Core.TokenID.HANDLERCONFIG)) {
                    ParseToken(reader);
                    n_HandlerConfig = ParseToken(reader, LTTSQL.Core.TokenID.DataIsland).ValueXmlFragment;
                }

                n_Options           = options?.ToArray();
                n_DependentAssembly = dependentAssembly?.ToArray();

                string name = n_ServiceMethodName.n_ServiceEntitiyName.Name + "/" + _sqlName();
                foreach(var method in n_Methods)
                    name += ":" + method.ToUpper();

                n_EntityName        = new LTTSQL.DataModel.EntityName(n_ServiceMethodName.n_ServiceEntitiyName.Schema, name);
            }

            public      override        void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_ServiceMethodName.TranspileNode(context);
                n_Options?.TranspileNodes(context);

                if (n_Proxy != null) {
                    try {
                        Proxy   = new Emit.FromExpression(n_Proxy.ValueString);
                    }
                    catch(Exception err) {
                        context.AddError(n_Proxy, err);
                    }
                }
            }

            public                      string                              GetOptionValueByName(string name)
            {
                if (n_Options != null) {
                    foreach(var o in n_Options) {
                        if (o.n_Name == name)
                            return o.n_Value;
                    }
                }

                return null;
            }
            private                     string                              _sqlName()
            {
                string path = n_ServiceMethodName.n_Name.ValueString;

                if (path.IndexOf('{') < 0)
                    return path;

                StringBuilder   rtn = new StringBuilder(path.Length);
                int             n = 0;

                for (int i = 0 ; i < path.Length ; ++i) {
                    char c = path[i];

                    switch(c)
                    {
                    case '{':
                        if (n == 0)
                            rtn.Append("{X}");

                        ++n;
                        break;

                    case '}':
                        if (n > 0)
                            --n;
                        break;

                    default:
                        if (c < ' ')
                            throw new TranspileException(n_ServiceMethodName.n_Name, "Invalid character in name");

                        if (n == 0) {
                            if (!(('A' <= c && c <= 'Z') ||
                                  ('a' <= c && c <= 'z') ||
                                  ('0' <= c && c <= '9') ||
                                  (c == '-' || c == '_' || c == '~' || c == ':' || c == '/' || c == '.')))
                                throw new TranspileException(n_ServiceMethodName.n_Name, "Invalid character in name");

                            rtn.Append(c);
                        }
                        break;
                    }
                }

                return rtn.ToString();
            }
        }
        public class ServiceParameter:  LTTSQL.Node.Node_Parameter
        {
            public      readonly    LTTSQL.Core.AstParseNode            n_Type;
            public      readonly    ParameterSource                     n_Source;
            public      readonly    ParameterOptions                    n_Options;
            public      readonly    LTTSQL.Node.Node_AS                 n_As;

            public                  LTTSQL.DataModel.ISqlType           SqlType             { get { return GetSqlType(n_Type);  } }
            public      override    LTTSQL.DataModel.Parameter          Parameter           { get { return _parameter;          } }
            public                  string                              Source
            {
                get {
                    if (n_Source != null)
                        return n_Source.n_Source;

                    if (n_Type is JsonType)
                        return "body:json";

                    throw new InvalidOperationException("Can't determin source.");
                }
            }

            private                 LTTSQL.DataModel.Parameter          _parameter;

            public                  bool                                DefaultValue;

            public                                                      ServiceParameter(LTTSQL.Core.ParserReader reader): base(reader)
            {
                if (JsonType.CanParse(reader)) {
                    var jsonType = new JsonType(reader);
                    n_Type   = AddChild(jsonType);

                    if (reader.CurrentToken.isToken(Core.TokenID.SOURCE))
                        n_Source = AddChild(new ParameterSource(reader, true));

                    jsonType.SetSchema(AddChild(new JsonSchema(reader)));
                }
                else {
                    n_Type   = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                     : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                    n_Source  = AddChild(new ParameterSource(reader, false));
                    n_Options = AddChild(new ParameterOptions(reader));

                    if (reader.CurrentToken.isToken(LTTSQL.Core.TokenID.AS))
                        n_As   = AddChild(new LTTSQL.Node.Node_AS(reader));
                }
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                _parameter = null;

                try {
                    n_Type.TranspileNode(context);
                    n_Source?.TranspileNode(context);
                    n_Options?.TranspileNode(context);
                    n_As?.TranspileNode(context);

                    var    sqlType      = SqlType;
                    var    flags        = DataModel.VariableFlags.Nullable;
                    object defaultvalue = null;;

                    if (n_Options != null) {
                        if (n_Options.n_Required && !n_Options.n_Key)
                            flags &= ~DataModel.VariableFlags.Nullable;

                        if (n_Options.n_Default != null) {
                            defaultvalue = n_Options.n_Default.getConstValue(sqlType);
                            flags |= DataModel.VariableFlags.HasDefaultValue;

                            if (defaultvalue != null)
                                flags &= ~DataModel.VariableFlags.Nullable;
                        }
                    }

                    _parameter = new LTTSQL.DataModel.Parameter(n_Name.Text,
                                                                sqlType ?? new DataModel.SqlTypeAny(),
                                                                n_Name,
                                                                flags,
                                                                defaultvalue);
                    n_Name.SetSymbol(_parameter);
                }
                catch(Exception err) {
                    context.AddError(this, err);
                }
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                foreach(var c in Children) {
                    c.Emit(emitWriter);

                    if (c == n_Type && DefaultValue) {
                        emitWriter.WriteText(" = ");
                        emitWriter.WriteValue(n_Options?.n_Default != null
                                                ? n_Options.n_Default.getConstValue()
                                                : (n_Type as LTTSQL.Node.Node_Datatype)?.SqlType.DefaultValue);
                    }
                }
            }
        }
        public class ParameterSource: LTTSQL.Core.AstParseNode
        {
            public class CustomSourceType:  LTTSQL.Core.AstParseNode
            {
                public      readonly        Core.Token                      n_Name;
                public      readonly        LTTSQL.Node.Node_AS             n_As;

                public                                                      CustomSourceType(LTTSQL.Core.ParserReader reader)
                {
                    n_Name = ParseToken(reader, Core.TokenID.String);
                    n_As = AddChild(new LTTSQL.Node.Node_AS(reader));
                }

                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    n_As.TranspileNode(context);
                }
            }

            public      readonly    string                              n_Source;
            public      readonly    CustomSourceType[]                  n_CustomSource;

            public                                                      ParameterSource(LTTSQL.Core.ParserReader reader, bool jsontype)
            {
                ParseToken(reader, LTTSQL.Core.TokenID.SOURCE);
                n_Source = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;

                if (!jsontype) {
                    if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                        var customSource = new List<CustomSourceType>();

                        do {
                            customSource.Add(AddChild(new CustomSourceType(reader)));
                        }
                        while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                        n_CustomSource = customSource.ToArray();
                        ParseToken(reader, Core.TokenID.RrBracket);
                    }
                }
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_CustomSource?.TranspileNodes(context);
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                EmitCommentNewine(emitWriter);
            }
        }
        public class ParameterOptions: LTTSQL.Core.AstParseNode
        {
            public      readonly    bool                                n_Key;
            public      readonly    bool                                n_Required;
            public      readonly    LTTSQL.Node.IExprNode               n_Default;

            public                                                      ParameterOptions(LTTSQL.Core.ParserReader reader)
            {
                if (ParseOptionalToken(reader, Core.TokenID.KEY) != null)
                    n_Key = true;

                switch(reader.CurrentToken.ID)
                {
                case LTTSQL.Core.TokenID.REQUIRED:
                    ParseToken(reader);
                    n_Required = true;
                    break;

                case LTTSQL.Core.TokenID.DEFAULT:
                    ParseToken(reader);
                    n_Default  = ParseExpression(reader);
                    AddBeforeWhitespace(null);
                    n_Required = false;
                    break;
                }
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_Default?.TranspileNode(context);
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                EmitCommentNewine(emitWriter);
            }
        }
        public class ComplexType: LTTSQL.Core.AstParseNode
        {
            public      readonly    LTTSQL.Core.TokenWithSymbol         n_Name;

            public                  LTTSQL.DataModel.EntityName         EntityName      { get; private set; }
            public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return WebComplexType?.n_Parameters.n_Parameters[0].Parameter?.SqlType;                } }

            public                  Node.WEBCOMPLEXTYPE                 WebComplexType  { get; private set; }

            public      static      bool                                CanParse(LTTSQL.Core.ParserReader reader)
            {
                return reader.CurrentToken.isToken(Core.TokenID.DoubleColon);
            }
            public                                                      ComplexType(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.DoubleColon);
                n_Name = ParseName(reader);
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                WebComplexType = null;

                var name                  = n_Name.ValueString;
                var complexTypeEntityName = LTTSQL.Node.DeclarationServiceComplexType.BuildEntityName(context.GetDeclarationObject<LTTSQL.Node.DeclarationServiceMethod>().ServiceName, name);
                var webComplexTypeEntity  = (context.Catalog.GetObject(complexTypeEntityName, false) as DataModel.EntityObjectCode);
                var webComplexType        = webComplexTypeEntity?.DeclarationObjectCode as Node.WEBCOMPLEXTYPE;

                if (webComplexType == null) {
                    context.AddError(n_Name, "Unknown WEBCOMPLEXTYPE '" + name + "'.");
                    return;
                }

                if (webComplexType.n_Parameters.n_Parameters.Length != 1) {
                    context.AddError(n_Name, "WEBCOMPLEXTYPE '" + name + "' not supported.");
                    return;
                }

                n_Name.SetSymbol(webComplexType.Entity);
                context.CaseWarning(n_Name, webComplexType.ComplexTypeName);

                WebComplexType = webComplexType;
            }

            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                foreach(var c in Children) {
                    if (object.ReferenceEquals(c, n_Name))
                        emitWriter.WriteText(SqlType.ToSql());
                    else
                    if (c.isWhitespaceOrComment)
                        c.Emit(emitWriter);
                }
            }
        }
        public class JsonType: LTTSQL.Core.AstParseNode
        {
            public                  LTTSQL.Core.Token                   n_json;
            public                  JsonSchema                          n_Schema        { get; private set; }

            public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return n_Schema.SqlType;                        } }

            public      static      bool                                CanParse(LTTSQL.Core.ParserReader reader)
            {
                return reader.CurrentToken.isToken("JSON");
            }
            public                                                      JsonType(LTTSQL.Core.ParserReader reader)
            {
                reader.CurrentToken.validateToken("JSON");
                n_json = Core.TokenWithSymbol.SetKeyword(reader.ReadToken(this, true));
            }
            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_Schema.TranspileNode(context);
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                foreach(var c in Children) {
                    if (object.ReferenceEquals(c, n_json))
                        emitWriter.WriteText("nvarchar(max)");
                    else
                    if (c.isWhitespaceOrComment)
                        c.Emit(emitWriter);
                }
            }

            public                  void                                SetSchema(JsonSchema schema)
            {
                n_Schema = schema;
            }
        }
        public class JsonSchema: LTTSQL.Core.AstParseNode
        {
            public      readonly    JsonSchemaElement                   n_Schema;

            public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return _sqlType;        } }

            public                  LTTSQL.DataModel.ISqlType           _sqlType;

            public abstract class JsonSchemaElement: LTTSQL.Core.AstParseNode
            {
                public                  DataModel.JsonFlags                 n_Flags     { get; private set; }
                public                  LTTSQL.Node.Node_AS                 n_As        { get; private set; }

                public                  LTTSQL.DataModel.JsonSchema         JsonSchema  { get { return _jsonSchema;         } }

                protected               LTTSQL.DataModel.JsonSchema         _jsonSchema;

                public      static      JsonSchemaElement                   Parse(LTTSQL.Core.ParserReader reader)
                {
                    if (reader.CurrentToken.isToken("OBJECT"))  return new JsonSchemaObject(reader);
                    if (reader.CurrentToken.isToken("ARRAY"))   return new JsonSchemaArray(reader);
                    return new JsonSchemaValue(reader, false);
                }

                protected               void                                ReadElement(LTTSQL.Core.ParserReader reader)
                {
                    if (ParseOptionalToken(reader, LTTSQL.Core.TokenID.REQUIRED) != null)
                        n_Flags |= DataModel.JsonFlags.Required;

                    if (reader.CurrentToken.isToken(LTTSQL.Core.TokenID.AS))
                        n_As = AddChild(new LTTSQL.Node.Node_AS(reader));
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    n_As?.TranspileNode(context);
                }
            }
            public class JsonSchemaObject: JsonSchemaElement
            {
                public class JsonSchemaObjectProperty: LTTSQL.Core.AstParseNode
                {
                    public      readonly    Core.TokenWithSymbol                n_Name;
                    public      readonly    JsonSchemaElement                   n_JsonSchemaElement;

                    public                  LTTSQL.DataModel.JsonSchemaObject.Property  JsonProperty        { get; private set; }

                    public                                                      JsonSchemaObjectProperty(LTTSQL.Core.ParserReader reader)
                    {
                        n_Name = ParseName(reader);
                        n_JsonSchemaElement = JsonSchemaElement.Parse(reader);
                    }
                    public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                    {
                        JsonProperty = null;
                        n_JsonSchemaElement.TranspileNode(context);
                        JsonProperty = new LTTSQL.DataModel.JsonSchemaObject.Property(n_Name.ValueString, n_Name, n_JsonSchemaElement.JsonSchema);
                        n_Name.SetSymbol(JsonProperty);
                    }
                }

                public      readonly    JsonSchemaObjectProperty[]          n_Properties;

                public                                                      JsonSchemaObject(LTTSQL.Core.ParserReader reader)
                {
                    ParseToken(reader, "OBJECT");
                    ParseToken(reader, Core.TokenID.LrBracket);

                    var properties = new List<JsonSchemaObjectProperty>();

                    do {
                        properties.Add(AddChild(new JsonSchemaObjectProperty(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    ParseToken(reader, Core.TokenID.RrBracket);
                    n_Properties = properties.ToArray();
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    n_Properties.TranspileNodes(context);
                    base.TranspileNode(context);

                    var properties = new LTTSQL.DataModel.JsonSchemaObject.PropertyList(n_Properties.Length);

                    foreach (var p in n_Properties) {
                        if (p.JsonProperty.JsonSchema != null) {
                            if (!properties.TryAdd(p.JsonProperty))
                                context.AddError(p.n_Name, "Property '" + p.JsonProperty.Name + "' already defined.");
                        }
                    }

                    _jsonSchema = new LTTSQL.DataModel.JsonSchemaObject(properties);
                }
            }
            public class JsonSchemaArray: JsonSchemaElement
            {
                public      readonly    JsonSchemaElement                   n_JsonSchemaElement;

                public                                                      JsonSchemaArray(LTTSQL.Core.ParserReader reader)
                {
                    ParseToken(reader, "ARRAY");

                    switch(reader.CurrentToken.validateToken("OBJECT", "VALUE"))
                    {
                    case "OBJECT":
                        n_JsonSchemaElement = new JsonSchemaObject(reader);
                        break;

                    case "VALUE":
                        n_JsonSchemaElement = new JsonSchemaValue(reader, true);
                        break;
                    }

                    ReadElement(reader);
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    base.TranspileNode(context);
                    n_JsonSchemaElement.TranspileNode(context);
                    _jsonSchema = new LTTSQL.DataModel.JsonSchemaArray(n_JsonSchemaElement.JsonSchema);
                }
            }
            public class JsonSchemaValue: JsonSchemaElement
            {
                public      readonly    LTTSQL.Core.AstParseNode            n_Type;

                public                  LTTSQL.DataModel.ISqlType           SqlType             { get { return GetSqlType(n_Type);  } }

                public                                                      JsonSchemaValue(LTTSQL.Core.ParserReader reader, bool hasValue)
                {
                    if (hasValue)
                        ParseToken(reader, "VALUE");

                    n_Type   = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                     : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                    ReadElement(reader);
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    n_Type.TranspileNode(context);
                    base.TranspileNode(context);
                    _jsonSchema = new DataModel.JsonSchemaValue(SqlType, n_Flags);
                }
            }

            public                                                      JsonSchema(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, "WITH");
                n_Schema = AddChild(JsonSchemaElement.Parse(reader));
            }
            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                _sqlType = null;
                n_Schema.TranspileNode(context);
                _sqlType = new LTTSQL.DataModel.SqlTypeJson(LTTSQL.DataModel.SqlTypeNative.NVarChar_MAX, n_Schema.JsonSchema);
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                EmitCommentNewine(emitWriter);
            }
        }

        public      readonly    ServiceDeclaration                      n_Declaration;
        public      readonly    string                                  n_Name;
        public                  List<RETURNS>                           n_returns                   { get; private set; }

        public      override    LTTSQL.DataModel.EntityName             EntityName                  { get { return n_Declaration.n_EntityName;                           } }
        public      override    LTTSQL.DataModel.EntityName             ServiceName                 { get { return n_Declaration.n_ServiceMethodName.n_ServiceEntitiyName; } }
        public      override    LTTSQL.Node.DeclarationService          DeclarationService          { get { return n_Declaration.n_ServiceMethodName.DeclarationService; } }

        public                                                          WEBMETHOD(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext)
        {
            reader.ReadLeading(this);
            n_Declaration = AddChild(new ServiceDeclaration(reader));
            n_Name = n_Declaration.n_ServiceMethodName.n_Name.ValueString;
            ParseParameters(reader, (r) => new ServiceParameter(r));
            ParseWith(reader, LTTSQL.DataModel.SymbolType.ServiceMethod);
            ParseGrant(reader, LTTSQL.DataModel.SymbolType.ServiceMethod);
            ParseOptionalAS(reader);
            ParseStatementBlock(reader, true);
        }

        public      override    void                                    TranspileNode(LTTSQL.Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Declaration.TranspileNode(context);
                if (DeclarationService == null || !DeclarationService.IsMember(this))
                    throw new ErrorException("Invalid method for service.");

                n_Parameters?.TranspileNode(context);
                TranspileOptions(context);

                Entity.Transpiled(parameters: n_Parameters?.t_Parameters);
                n_Declaration.n_ServiceMethodName.n_Name.SetSymbol(Entity);
                _declarationTranspiled = true;
            }

            {
                bool    defaultValue = false;

                for (int i = 0 ; i < n_Parameters.n_Parameters.Length ; ++i) {
                    var     parameter = (ServiceParameter)n_Parameters.n_Parameters[i];

                    if (parameter.n_Options == null || !parameter.n_Options.n_Required || parameter.n_Options.n_Key)
                        defaultValue = true;

                    parameter.DefaultValue = defaultValue;
                }
            }

            TranspileStatement(context);

            if (n_Declaration.n_HttpHandler == "sql-json2") {
                if (n_Declaration.n_Proxy != null && n_returns != null && n_returns.Count != 1)
                    context.AddError(this, "sql-json2 and proxy only support 1 RETURNS");
            }

            Transpiled = true;
        }

                                bool                                    LTTSQL.Node.IParseContext.StatementCanParse(LTTSQL.Core.ParserReader reader)
        {
            return RETURNS.CanParse(reader) ||
                   reader.Transpiler.StatementParsers.CanParse(reader, this);
        }
                                LTTSQL.Node.Statement                   LTTSQL.Node.IParseContext.StatementParse(LTTSQL.Core.ParserReader reader)
        {
            if (RETURNS.CanParse(reader)) {
                var returns = new RETURNS(reader);

                if (n_returns == null)
                    n_returns = new List<RETURNS>();

                n_returns.Add(returns);

                return returns;
            }

            return reader.Transpiler.StatementParsers.Parse(reader, this);
        }

        public      override    void                                    Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (Object.ReferenceEquals(node, n_Declaration)) {
                    n_Declaration.EmitCustom(emitWriter, (ew) =>
                                                {
                                                    if (!ew.EmitOptions.DontEmitCustomComment)
                                                        ew.WriteText("\r\n");

                                                    ew.WriteText("CREATE PROCEDURE " + EntityName.Fullname);
                                                });
                }
                else
                    node.Emit(emitWriter);
            }
        }

        public      override    Core.IAstNode                           GetNameToken()
        {
            return n_Declaration;
        }
        public      override    string                                  CollapsedName()
        {
            return "webmethod " + n_Declaration.n_ServiceMethodName.n_Name.ValueString;
        }

        public      static      LTTSQL.DataModel.ISqlType               GetSqlType(LTTSQL.Core.AstParseNode node)
        {
            if (node is LTTSQL.Node.Node_Datatype dataType)       return dataType.SqlType;
            if (node is ComplexType               complexType)    return complexType.SqlType;
            if (node is JsonType                  jsonType)       return jsonType.SqlType;

            throw new InvalidOperationException();
        }

    }
}
