using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum ObjectReturnOption
    {
        Nothing     = 0,
        Optional,
        Required
    }

    public interface IReferencedEntity
    {
        DataModel.EntityName            getReferencedEntity(DeclarationObjectCode declarationObjectCode);
    }

    public abstract class DeclarationObjectCode: DeclarationEntity, IParseContext
    {
        public                  Node_ProgrammabilityOptions         n_Options                   { get; private set; }
        public                  Node_ParameterList                  n_Parameters                { get; protected set; }
        public                  Core.AstParseNode                   n_Statement                 { get; protected set; }
        public                  Node_ObjectGrantList                n_GrantList                 { get; private set; }
        public      virtual     ObjectReturnOption                  ReturnOption                { get { return ObjectReturnOption.Nothing;                                } }
        public      virtual     DataModel.ISqlType                  ReturnType                  { get { throw new InvalidOperationException("Object returns nothing.");   } }
        public      virtual     DataModel.VariableLocal             ReturnVariable              { get { throw new InvalidOperationException("Object returns nothing.");   } }

        public                  DataModel.EntityObjectCode          Entity                      { get; private set; }

        public                  void                                ParseParameters(Core.ParserReader reader, Node_SqlParameter.InterfaceType interfaceType)
        {
            n_Parameters = AddChild(new Node_ParameterList(reader, interfaceType));
        }
        public                  void                                ParseParameters(Core.ParserReader reader, Node_ParameterList.CreateNodeParameter createNodeParameter)
        {
            n_Parameters = AddChild(new Node_ParameterList(reader, createNodeParameter));
        }

        public                  void                                ParseWith(Core.ParserReader reader, DataModel.SymbolType type)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.WITH))
                n_Options = AddChild(new Node_ProgrammabilityOptions(reader, type));
        }
        public                  void                                ParseGrant(Core.ParserReader reader, DataModel.SymbolType type)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.GRANT))
                n_GrantList = AddChild(new Node_ObjectGrantList(reader, type));
        }

        public                  void                                ParseOptionalAS(Core.ParserReader reader)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.BEGIN, Core.TokenID.EXTERNAL)) {
                AddLeading(reader);
                AddChild(new Node.Node_CustomNode("AS\r\n"));
            }
            else
                ParseToken(reader, Core.TokenID.AS);
        }
        public                  void                                ParseStatementBlock(Core.ParserReader reader, bool standardSettings)
        {
            n_Statement = AddChild(new Statement_BEGIN_END_code(reader, this, standardSettings));
        }
        public                  void                                ParseStatementQuery(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            n_Statement = AddChild(new Query_Select(reader, selectContext));
        }

        public      override    DataModel.EntityName[]              ObjectReferences()
        {
            HashSet<DataModel.EntityName>       rtn = new HashSet<DataModel.EntityName>();

            _externalObjectReferences_walk(this, rtn);

            return (new List<DataModel.EntityName>(rtn)).ToArray();
        }

        public      override    void                                TranspileInit(Transpile.TranspileContext transpileContext, SourceFile sourceFile)
        {
            Transpiled             = false;
            _declarationTranspiled = false;

            var entityName = EntityName;
            if (entityName != null) {
                if ((Entity = transpileContext.Catalog.DefineObjectCode(EntityType, entityName)) == null)
                    throw new TranspileException(GetNameToken(), "Duplicate definition of object.");

                Entity.TranspileInit(this, new DataModel.DocumentSpan(sourceFile.Filename, this));
            }
        }
        public                  void                                TranspileOptions(Transpile.Context context)
        {
            n_Options?.TranspileNode(context);
            n_GrantList?.TranspileNode(context);
        }
        public                  void                                TranspileStatement(Transpile.Context context, bool query=false)
        {
            n_Statement.TranspileNode(query ? new Transpile.ContextStatementQuery(context) : context);
        }

                                Statement                           IParseContext.StatementParent          => null;
                                bool                                IParseContext.StatementCanParse(Core.ParserReader reader)
        {
            return reader.Transpiler.StatementParsers.CanParse(reader, this);
        }
                                Statement                           IParseContext.StatementParse(Core.ParserReader reader)
        {
            return reader.Transpiler.StatementParsers.Parse(reader, this);
        }

        public      override    void                                EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_GrantList != null) {
                var emitWriter = new Core.EmitWriterSourceMap(emitContext, sourceFile.Filename, Children.FirstNoWhithspaceToken.Beginning.Lineno);

                n_GrantList.EmitGrant("OBJECT", EntityName, emitWriter);
                emitContext.Database.ExecuteStatement(emitWriter.GetSql(), emitWriter.SourceMap, emitContext.AddEmitError);
            }
        }

        private                 void                                _externalObjectReferences_walk(Core.IAstNode node, HashSet<DataModel.EntityName> rtn)
        {
            if (node is IReferencedEntity refnode) {
                var entityName = refnode.getReferencedEntity(this);
                if (entityName != null) {
                    if (!rtn.Contains(entityName))
                        rtn.Add(entityName);
                }
            }

            if (node.Children != null) {
                foreach(var child in node.Children)
                    _externalObjectReferences_walk(child, rtn);
            }
        }
    }
}
