using System;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    [LTTSQL.Library.DeclarationParser("WEBCOMPLEXTYPE")]
    public class WEBCOMPLEXTYPE: LTTSQL.Node.DeclarationServiceComplexType, LTTSQL.Node.IParseContext, LTTSQL.Node.ICodeAnalyze
    {
        public class Receives: LTTSQL.Core.AstParseNode
        {
            public      readonly    LTTSQL.Core.AstParseNode                    n_ReceivesType;

            public                  LTTSQL.DataModel.ISqlType                   SqlType
            {
                get {
                    return _sqlType;
                }
            }

            private                 LTTSQL.DataModel.ISqlType                   _sqlType;

            public                                                              Receives(Core.ParserReader reader)
            {
                ParseToken(reader, "RECEIVES");

                if (ParseOptionalToken(reader, Core.TokenID.RETURNS) == null) {
                    if (JsonType.CanParse(reader)) {
                        AddChild(n_ReceivesType = new JsonType(reader, true));
                    }
                    else {
                        n_ReceivesType = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                               : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                    }
                }
            }

            public      override    void                                        TranspileNode(Transpile.Context context)
            {
                if (n_ReceivesType != null) {
                    n_ReceivesType.TranspileNode(context);
                    _sqlType = ((LTTSQL.Node.ISqlType)n_ReceivesType).SqlType;
                }
                else {
                    _sqlType = new LTTSQL.DataModel.SqlTypeJson(LTTSQL.DataModel.SqlTypeNative.NVarChar_MAX, _convertReponseToJsonSchema(((WEBCOMPLEXTYPE)ParentNode).ResponseNode.SqlType));
                }
            }
            public      override    void                                        Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
            }
        }

        public      readonly    Declaration                                 n_Declaration;
        public      readonly    Receives                                    n_Receives;
        public      readonly    LTTSQL.Node.Node_Attributes                 n_Attributes;

        public      override    LTTSQL.DataModel.SymbolType                 EntityType                  { get { return LTTSQL.DataModel.SymbolType.ServiceComplexType;       } }
        public      override    LTTSQL.DataModel.EntityName                 EntityName                  { get { return n_Declaration.n_EntityName;                           } }
        public      override    bool                                        callableFromCode            { get { return true; } }
        public      override    string                                      ComplexTypeName             { get { return n_Declaration.n_ServiceTypeName.n_Name.ValueString;   } }
        public      override    LTTSQL.Node.DeclarationService              DeclarationService          { get { return n_Declaration.n_ServiceTypeName.DeclarationService;   } }
        public                  LTTSQL.DataModel.ISqlType                   ReceivesSqlType
        {
            get {
                Entity.testTranspiled();

                if (n_Receives != null) {
                    return n_Receives.SqlType;
                }
                else {
                    return n_Parameters.n_Parameters[0].Parameter?.SqlType;
                }
            }
        }
        public                  LTTSQL.Node.Query_Select_ColumnResponse[]   ResponseColumns             { get { return ResponseNode.ResponseColumns;                         } }


        public                                                              WEBCOMPLEXTYPE(Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext): base(reader)
        {
            AddLeading(reader);
            n_Declaration = AddChild(new Declaration(reader));

            if (reader.CurrentToken.isToken("ATTRIBUTES")) {
                n_Attributes = AddChild(new LTTSQL.Node.Node_Attributes(reader));
            }

            ParseParameters(reader, LTTSQL.Node.Node_SqlParameter.InterfaceType.Function);

            if (reader.CurrentToken.isToken("RECEIVES")) {
                AddChild(n_Receives = new Receives(reader));
            }

            ParseToken(reader, Core.TokenID.RETURNS);
            n_Statement = AddChild(new LTTSQL.Node.Expr_ResponseNode(reader));
        }

        public      override    void                                        TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Declaration.TranspileNode(context);
                n_Attributes?.TranspileNode(context);

                if (DeclarationService == null || !DeclarationService.IsMember(this))
                    throw new ErrorException("Invalid method for service.");

                n_Parameters.TranspileNode(context);
                if (n_Parameters.n_Parameters.Length == 0)
                    context.AddError(this, "Parameters missing.");

                _declarationTranspiled = true;
            }

            TranspileStatement(context, query:true);

            if (n_Receives != null) {
                n_Receives.TranspileNode(context);
            }
            else {
                if (n_Parameters.n_Parameters.Length != 1) {
                    context.AddError(this, "Missing receives type.");
                }
            }

            Entity.Transpiled(parameters: n_Parameters.t_Parameters, returns: ResponseNode.SqlType);
            n_Declaration.n_ServiceTypeName.n_Name.SetSymbolUsage(Entity, DataModel.SymbolUsageFlags.Declaration);
        }
        public                  void                                        CodeAnalyze(Transpile.AnalyzeContext context)
        {
            if (n_Attributes != null) {
                foreach (var attr in n_Attributes.n_Attributes) {
                    try {
                        switch(attr.t_Attr.Name) {
                        case "select-lookup":   _analyzeSelectLookup(context, attr); break;
                        case "select-search":   _analyzeSelectSearch(context, attr); break;
                        }
                    }
                    catch (Exception err) {
                        context.AddError(attr, err);
                    }
                }
            }
        }
        public      override    void                                        Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (Object.Equals(node, n_Statement))
                    ResponseNode.EmitComplexTypeReturn(emitWriter);
                else
                    node.Emit(emitWriter);
            }
        }

        public      override    Core.IAstNode                               GetNameToken()
        {
            return n_Declaration.n_ServiceTypeName.n_Name;
        }
        public      override    string                                      CollapsedName()
        {
            return "webcomplextype " + n_Declaration.n_ServiceTypeName.n_Name.ValueString;
        }

        public                  void                                        _analyzeSelectLookup(Transpile.AnalyzeContext context, LTTSQL.Node.Node_Attribute attr)
        {
            var webMethod = _getWebMethod(context, (string)attr.t_Value);
            if (webMethod == null) {
                context.AddError(attr, "Unknown webmethod '" + (string)attr.t_Value + "'.");
                return;
            }

            if (webMethod.n_Declaration.n_Kind != "select-lookup") {
                context.AddError(attr, "WEBMETHOD '" + (string)attr.t_Value + "' has invalid kind expect 'select-lookup'.");
            }

            if (webMethod.n_Returns.Count == 1) {
                if (_selectCompareResult(webMethod.n_Returns[0].n_Expression)) {
                    return;
                }
            }

            context.AddError(attr, "WEBMETHOD '" + (string)attr.t_Value + "' returns incompatible select-lookup data for '" + n_Declaration.n_ServiceTypeName.n_Name.ValueString + "'.");
        }
        public                  void                                        _analyzeSelectSearch(Transpile.AnalyzeContext context, LTTSQL.Node.Node_Attribute attr)
        {
            var webMethod = _getWebMethod(context, (string)attr.t_Value);
            if (webMethod == null) {
                context.AddError(attr, "Unknown webmethod '" + (string)attr.t_Value + "'.");
                return;
            }

            if (webMethod.n_Declaration.n_Kind != "select-search") {
                context.AddError(attr, "WEBMETHOD '" + (string)attr.t_Value + "' has invalid kind expect 'select-search'.");
            }

            int nok = 0;

            foreach (var rtn in webMethod.n_Returns) {
                if (rtn.n_Expression is LTTSQL.Node.IExprResponseNode exprResponseNode) {

                    if (exprResponseNode.ResponseNodeType == LTTSQL.DataModel.ResponseNodeType.ArrayValue) {
                        if (_selectCompareResult(exprResponseNode.ResponseColumns[0].n_Expression)) {
                            ++nok;
                            continue;
                        }
                    }

                    if (exprResponseNode.ResponseNodeType == LTTSQL.DataModel.ResponseNodeType.ArrayObject) {
                        if (_selectCompareResult(exprResponseNode.ResponseColumns)) {
                            ++nok;
                            continue;
                        }
                    }
                }

                if ((rtn.n_Expression.SqlType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 &&
                    rtn.n_Expression.SqlType.NativeType.isString) {
                    continue;
                }

                context.AddError(attr, "WEBMETHOD '" + (string)attr.t_Value + "' returns incompatible select-search data for '" + n_Declaration.n_ServiceTypeName.n_Name.ValueString + "'.");
            }

            if (nok == 0) {
                context.AddError(attr, "WEBMETHOD '" + (string)attr.t_Value + "' returns no dataset.");
            }
        }
        public                  WEBMETHOD                                   _getWebMethod(Transpile.AnalyzeContext context, string methodName)
        {
            var x = context.Catalog.GetObject(LTTSQL.Node.Node_ServiceEntityName.BuildEntityName(n_Declaration.n_ServiceTypeName.n_ServiceEntitiyName, methodName, new string [] { "GET" }));

            if (x is DataModel.EntityObjectCode objectCode && objectCode.DeclarationObjectCode is WEBMETHOD webMethod) {
                return webMethod;
            }

            return null;
        }
        public                  bool                                        _selectCompareResult(LTTSQL.Node.IExprNode expr)
        {
            if (expr is LTTSQL.Node.Expr_ServiceComplexType complexType) {
                if (complexType.DeclarationComplexType == this) {
                    return true;
                }
            }

            return false;
        }
        public                  bool                                        _selectCompareResult(LTTSQL.Node.Query_Select_ColumnResponse[] retrurnColumns)
        {
            var expectedColumns = ResponseNode.ResponseColumns;

            if (retrurnColumns.Length < expectedColumns.Length) {
                return false; 
            }

            for (int i = 0 ; i < expectedColumns.Length ; ++i) {
                var expectedColumn = expectedColumns[i];
                var returnColumn   = retrurnColumns[i];

                if (expectedColumn.n_FieldName.ValueString != returnColumn.n_FieldName.ValueString) {
                    return false;
                }

                if (!LTTSQL.Logic.Validate.AssignAllowed(expectedColumn.n_Expression.SqlType, expectedColumn.n_Expression)) {
                    return false; 
                }
            }

            return true;
        }

        private     static      LTTSQL.DataModel.JsonSchema                 _convertReponseToJsonSchema(LTTSQL.DataModel.ISqlType type)
        {
            if (type is LTTSQL.DataModel.SqlTypeResponseNode responseNode) {
                switch(responseNode.NodeType) {
                case DataModel.ResponseNodeType.Object:
                case DataModel.ResponseNodeType.ObjectMandatory:
                    return _convertResponseObjectToJsonSchema(type);

                case DataModel.ResponseNodeType.ArrayValue:
                    return new LTTSQL.DataModel.JsonSchemaArray(_convertReponseToJsonSchema(type.Columns[0].SqlType));

                case DataModel.ResponseNodeType.ArrayObject:
                    return new LTTSQL.DataModel.JsonSchemaArray(_convertResponseObjectToJsonSchema(type));

                default:
                    throw new InvalidOperationException("invalid SqlTypeResponseNode.NodeType");
                }
            }

            return new LTTSQL.DataModel.JsonSchemaValue(type, DataModel.JsonFlags.None);
        }
        private     static      LTTSQL.DataModel.JsonSchema                 _convertResponseObjectToJsonSchema(LTTSQL.DataModel.ISqlType type)
        {
            var properties = new LTTSQL.DataModel.JsonSchemaObject.PropertyList(type.Columns.Count);

            foreach(var c in type.Columns) {
                properties.Add(new LTTSQL.DataModel.JsonSchemaObject.Property(c.Name, c.Declaration, _convertReponseToJsonSchema(c.SqlType)));
            }

            return new LTTSQL.DataModel.JsonSchemaObject(properties);
        }
    }
}
