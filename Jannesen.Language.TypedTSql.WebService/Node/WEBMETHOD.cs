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
            public class WebOptions:  LTTSQL.Core.AstParseNode
            {
                public      readonly        OptionValue[]                       n_Options;

                internal                                                        WebOptions(LTTSQL.Core.ParserReader reader)
                {
                    var options = new List<OptionValue>();

                    ParseToken(reader, "WEB_OPTIONS");

                    do {
                        options.Add(AddChild(new OptionValue(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_Options = options.ToArray();
                }
                public      override        void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    n_Options.TranspileNodes(context);
                }

                public                      string                              FindOption(string name)
                {
                    for(int i = 0 ; i < n_Options.Length ; ++i) {
                        if (n_Options[i].n_Name == name) {
                            return n_Options[i].n_Value;
                        }
                    }

                    return null;
                }
            }
            public class WebAssemblies:  LTTSQL.Core.AstParseNode
            {
                public      readonly        string[]                            n_Assemblies;

                internal                                                        WebAssemblies(LTTSQL.Core.ParserReader reader)
                {
                    var assemblies = new List<string>();

                    ParseToken(reader, "WEB_ASSEMBLIES");

                    do {
                        assemblies.Add(ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString);
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_Assemblies = assemblies.ToArray();
                }
                public      override        void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                }
            }
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
                    switch(n_Name) {
                    case "timeout":
                        if (!int.TryParse(n_Value, out var value) || value < 5 || value > 300)
                            context.AddError(this, "Invalid timeout value.");
                        break;
                    }
                }
            }

            public      readonly        LTTSQL.Node.Node_ServiceEntityName  n_ServiceMethodName;
            public      readonly        string[]                            n_Methods;
            public      readonly        string                              n_WebHttpHandler;
            public      readonly        WebOptions                          n_WebHandlerOptions;
            public      readonly        WebAssemblies                       n_WebHandlerAssemblies;
            public      readonly        XmlElement                          n_WebHandlerConfig;
            public      readonly        LTTSQL.DataModel.EntityName         n_EntityName;
            public      readonly        LTTSQL.Core.Token                   n_JcProxy;
            public                      Emit.JcNSExpression                 JcProxy             { get; private set; }

            public                                                          ServiceDeclaration(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, "WEBMETHOD");
                n_ServiceMethodName = AddChild(new LTTSQL.Node.Node_ServiceEntityName(reader));
                ParseToken(reader, LTTSQL.Core.TokenID.METHOD);

                {
                    var methods = new List<string>();

                    do {
                        methods.Add(ParseToken(reader, Core.TokenID.String).ValueString.ToUpperInvariant());
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    n_Methods = methods.ToArray();
                }

                while (reader.CurrentToken.isToken("WEB_HANDLER", "WEB_OPTIONS", "WEB_ASSEMBLIES", "WEB_HANDLERCONFIG", "JC_PROXY")) {
                    switch(reader.CurrentToken.Text.ToUpperInvariant()) {
                    case "WEB_HANDLER":
                        ParseToken(reader, "WEB_HANDLER");
                        n_WebHttpHandler = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                        break;

                    case "WEB_OPTIONS":
                        n_WebHandlerOptions = AddChild(new WebOptions(reader));
                        break;

                    case "WEB_ASSEMBLIES":
                        n_WebHandlerAssemblies = AddChild(new WebAssemblies(reader));
                        break;

                    case "WEB_HANDLERCONFIG":
                        ParseToken(reader, "WEB_HANDLERCONFIG");
                        n_WebHandlerConfig = ParseToken(reader, LTTSQL.Core.TokenID.DataIsland).ValueXmlFragment;
                        break;

                    case "JC_PROXY":
                        ParseToken(reader, "JC_PROXY");
                        n_WebHttpHandler = "sql-json2";
                        n_JcProxy = ParseToken(reader, LTTSQL.Core.TokenID.String);
                        break;
                    }
                }

                string name = n_ServiceMethodName.n_ServiceEntitiyName.Name + "/" + _sqlName();
                foreach(var method in n_Methods)
                    name += ":" + method.ToUpperInvariant();

                n_EntityName        = new LTTSQL.DataModel.EntityName(n_ServiceMethodName.n_ServiceEntitiyName.Schema, name);
            }

            public      override        void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_ServiceMethodName.TranspileNode(context);
                n_WebHandlerOptions?.TranspileNode(context);

                if (n_JcProxy != null) {
                    try {
                        JcProxy   = new Emit.JcNSExpression(n_JcProxy.ValueString);
                    }
                    catch(Exception err) {
                        context.AddError(n_JcProxy, err);
                    }
                }
            }

            public                      string                              GetWebHandlerOptionValueByName(string name)
            {
                if (n_WebHandlerOptions != null) {
                    foreach(var o in n_WebHandlerOptions.n_Options) {
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

                    switch(c) {
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
        public class ServiceParameter: LTTSQL.Node.Node_Parameter
        {
            public      override    Core.TokenWithSymbol                n_Name              => _Name;
            public      readonly    LTTSQL.Core.AstParseNode            n_Type;
            public      readonly    ParameterSource                     n_Source;
            public      readonly    ParameterOptions                    n_Options;

            public                  LTTSQL.DataModel.ISqlType           SqlType             { get { return ((LTTSQL.Node.ISqlType)n_Type).SqlType;  } }
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

            private                 Core.TokenWithSymbol                _Name;
            private                 LTTSQL.DataModel.Parameter          _parameter;

            public                  bool                                DefaultValue;

            public                                                      ServiceParameter(LTTSQL.Core.ParserReader reader)
            {
                if (reader.CurrentToken.isToken(Core.TokenID.LocalName)) {
                    _Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
                }

                if (JsonType.CanParse(reader)) {
                    var jsonType = new JsonType(reader, false);
                    n_Type   = AddChild(jsonType);

                    if (reader.CurrentToken.isToken(Core.TokenID.SOURCE))
                        n_Source = AddChild(new ParameterSource(reader));

                    AddChild(jsonType.ParseSchema(reader));
                }
                else {
                    n_Type   = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                     : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                    n_Source  = AddChild(new ParameterSource(reader));
                    n_Options = AddChild(new ParameterOptions(reader));
                }
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                _parameter = null;

                try {
                    n_Type.TranspileNode(context);
                    n_Source?.TranspileNode(context);
                    n_Options?.TranspileNode(context);

                    var    sqlType      = SqlType;
                    var    flags        = DataModel.VariableFlags.Nullable;
                    object defaultvalue = null;

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

                    if (n_Name != null) {
                        _parameter = new LTTSQL.DataModel.Parameter(n_Name.Text,
                                                                    sqlType ?? new DataModel.SqlTypeAny(),
                                                                    n_Name,
                                                                    flags,
                                                                    defaultvalue);
                        n_Name.SetSymbolUsage(_parameter, DataModel.SymbolUsageFlags.Declaration);
                    }
                    else {
                        if (n_Source == null || !n_Source.n_Source.Contains(":")) {
                            context.AddError((Core.IAstNode)n_Source ?? (Core.IAstNode)this, "Invalid source");
                        }
                    }
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
            public      readonly    string                              n_Source;

            public                                                      ParameterSource(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, LTTSQL.Core.TokenID.SOURCE);
                n_Source = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
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

                switch(reader.CurrentToken.ID) {
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
                n_Declaration.n_ServiceMethodName.n_Name.SetSymbolUsage(Entity, DataModel.SymbolUsageFlags.Declaration);
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

            if (n_Declaration.n_WebHttpHandler == "sql-json2") {
                if (n_Declaration.n_JcProxy != null && n_returns != null && n_returns.Count != 1)
                    context.AddError(this, "sql-json2 and proxy only support 1 RETURNS");
            }

            Transpiled = true;
        }

                                LTTSQL.Node.Statement                   LTTSQL.Node.IParseContext.StatementParent          => null;
                                bool                                    LTTSQL.Node.IParseContext.StatementCanParse(LTTSQL.Core.ParserReader reader)
        {
            return RETURNS.CanParse(reader) ||
                   reader.Transpiler.StatementParsers.CanParse(reader, this);
        }
                                LTTSQL.Node.Statement                   LTTSQL.Node.IParseContext.StatementParse(LTTSQL.Core.ParserReader reader)
        {
            if (RETURNS.CanParse(reader)) {
                var returns = new RETURNS(reader, this);

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
    }
}
