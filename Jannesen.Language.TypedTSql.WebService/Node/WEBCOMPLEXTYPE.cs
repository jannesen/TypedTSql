using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    [LTTSQL.Library.DeclarationParser("WEBCOMPLEXTYPE")]
    public class WEBCOMPLEXTYPE: LTTSQL.Node.DeclarationServiceComplexType, LTTSQL.Node.IParseContext
    {
        public class Receives: LTTSQL.Core.AstParseNode
        {
            public      readonly    LTTSQL.Core.AstParseNode                n_ReceivesType;

            public                  LTTSQL.DataModel.ISqlType               SqlType
            {
                get {
                    return _sqlType;
                }
            }

            private                 LTTSQL.DataModel.ISqlType               _sqlType;

            public                                                          Receives(Core.ParserReader reader)
            {
                ParseToken(reader);

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

            public      override    void                                    TranspileNode(Transpile.Context context)
            {
                if (n_ReceivesType != null) {
                    _sqlType = ((LTTSQL.Node.ISqlType)n_ReceivesType).SqlType;
                }
                else {
                    _sqlType = new LTTSQL.DataModel.SqlTypeJson(LTTSQL.DataModel.SqlTypeNative.NVarChar_MAX, _convertReponseToJsonSchema(((WEBCOMPLEXTYPE)Parent).ResponseNode.SqlType));
                }
            }

            public      override    void                                    Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
            }                
        }

        public      override    LTTSQL.DataModel.SymbolType             EntityType                  { get { return LTTSQL.DataModel.SymbolType.ServiceComplexType;       } }
        public      override    LTTSQL.DataModel.EntityName             EntityName                  { get { return n_Declaration.n_EntityName;                           } }
        public      override    bool                                    callableFromCode            { get { return true; } }
        public      override    string                                  ComplexTypeName             { get { return n_Declaration.n_ServiceTypeName.n_Name.ValueString;   } }
        public      override    LTTSQL.Node.DeclarationService          DeclarationService          { get { return n_Declaration.n_ServiceTypeName.DeclarationService;   } }
        public                  LTTSQL.DataModel.ISqlType               ReceivesSqlType
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

        public      readonly    Declaration                             n_Declaration;
        public      readonly    LTTSQL.Node.Node_AS                     n_As;
        public      readonly    Receives                                n_Receives;

        public                                                          WEBCOMPLEXTYPE(Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext): base(reader)
        {
            AddLeading(reader);
            n_Declaration = AddChild(new Declaration(reader));
            n_As          = AddChild(new LTTSQL.Node.Node_AS(reader));
            ParseParameters(reader, LTTSQL.Node.Node_SqlParameter.InterfaceType.Function);

            if (reader.CurrentToken.isToken("RECEIVES")) {
                AddChild(n_Receives = new Receives(reader));
            }

            ParseToken(reader, Core.TokenID.RETURNS);
            n_Statement = AddChild(new LTTSQL.Node.Expr_ResponseNode(reader));
        }

        public      override    void                                    TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Declaration.TranspileNode(context);
                n_As.TranspileNode(context);

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
            n_Declaration.n_ServiceTypeName.n_Name.SetSymbol(Entity);
        }
        public      override    void                                    Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (Object.Equals(node, n_Statement))
                    ResponseNode.EmitComplexTypeReturn(emitWriter);
                else
                    node.Emit(emitWriter);
            }
        }

        public      override    Core.IAstNode                           GetNameToken()
        {
            return n_Declaration.n_ServiceTypeName.n_Name;
        }
        public      override    string                                  CollapsedName()
        {
            return "webcomplextype " + n_Declaration.n_ServiceTypeName.n_Name.ValueString;
        }

        private     static      LTTSQL.DataModel.JsonSchema             _convertReponseToJsonSchema(LTTSQL.DataModel.ISqlType type)
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
        private     static      LTTSQL.DataModel.JsonSchema             _convertResponseObjectToJsonSchema(LTTSQL.DataModel.ISqlType type)
        {
            var properties = new LTTSQL.DataModel.JsonSchemaObject.PropertyList(type.Columns.Count);

            foreach(var c in type.Columns) {
                properties.Add(new LTTSQL.DataModel.JsonSchemaObject.Property(c.Name, c.Declaration, _convertReponseToJsonSchema(c.SqlType)));
            }

            return new LTTSQL.DataModel.JsonSchemaObject(properties);
        }
    }
}
