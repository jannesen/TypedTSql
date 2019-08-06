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
            var     statement = new Statement_BEGIN_END(reader, this);

            if (standardSettings)
                _insertStandardSettings(statement.n_Statements);

            n_Statement = AddChild(statement);
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

        public      override    void                                TranspileInit(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile)
        {
            Transpiled             = false;
            _declarationTranspiled = false;

            var entityName = EntityName;
            if (entityName != null) {
                if ((Entity = catalog.DefineObjectCode(EntityType, entityName)) == null)
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
            else {
                if (node.Children != null) {
                    foreach(var child in node.Children)
                        _externalObjectReferences_walk(child, rtn);
                }
            }
        }

        private     static      void                                _insertStandardSettings(StatementBlock  statements)
        {
            bool        addLine                        = true;
            bool        setNOCOUNT                     = true;
            bool        setCONCAT_NULL_YIELDS_NULL     = true;
            bool        setXACT_ABORT                  = true;
            bool        setANSI_NULLS                  = true;
            bool        setANSI_PADDING                = true;
            bool        setANSI_WARNINGS               = true;
            bool        setARITHABORT                  = true;
            bool        setNUMERIC_ROUNDABORT          = true;
            bool        setTRANSACTION_ISOLATION_LEVEL = true;

            if (statements.Children != null) {
                foreach(var child in statements.Children) {
                    if (child is Core.AstParseNode) {
                        if (child is Statement_SET_option) {
                            addLine = false;

                            foreach(string option in ((Statement_SET_option)child).n_Options) {
                                switch(option) {
                                case "ANSI_NULLS":                  setANSI_NULLS                  = false; break;
                                case "ANSI_PADDING":                setANSI_PADDING                = false; break;
                                case "ANSI_WARNINGS":               setANSI_WARNINGS               = false; break;
                                case "ARITHABORT":                  setARITHABORT                  = false; break;
                                case "CONCAT_NULL_YIELDS_NULL":     setCONCAT_NULL_YIELDS_NULL     = false; break;
                                case "NOCOUNT":                     setNOCOUNT                     = false; break;
                                case "NUMERIC_ROUNDABORT":          setNUMERIC_ROUNDABORT          = false; break;
                                case "TRANSACTION":                 setTRANSACTION_ISOLATION_LEVEL = false; break;
                                case "XACT_ABORT":                  setXACT_ABORT                  = false; break;

                                case "ANSI_NULL_DFLT_ON":
                                    if (string.Compare(((Core.Token)((Statement_SET_option)child).n_Value).Text, "ON", StringComparison.InvariantCultureIgnoreCase) == 0) {
                                        setARITHABORT    = false;
                                        setXACT_ABORT    = false;
                                        setANSI_NULLS    = false;
                                        setANSI_PADDING  = false;
                                        setANSI_WARNINGS = false;
                                    }
                                    break;
                                }
                            }
                        }
                        else
                            break;
                    }
                    else
                    if (child is Core.Token) {
                        if (!((Core.Token)child).isWhitespaceOrComment)
                            break;
                    }
                }
            }

            List<Node_CustomNode>   children = new List<Node_CustomNode>();

            string  setOn = "";
            if (setNOCOUNT)                     setOn += ",NOCOUNT";
            if (setANSI_NULLS)                  setOn += ",ANSI_NULLS";
            if (setANSI_PADDING)                setOn += ",ANSI_PADDING";
            if (setANSI_WARNINGS)               setOn += ",ANSI_WARNINGS";
            if (setARITHABORT)                  setOn += ",ARITHABORT";
            if (setCONCAT_NULL_YIELDS_NULL)     setOn += ",CONCAT_NULL_YIELDS_NULL";
            if (setXACT_ABORT)                  setOn += ",XACT_ABORT";

            if (setOn.Length > 0) {
                children.Add(new Node.Node_CustomNode("    SET " + setOn.Substring(1) + " ON;\r\n"));
            }

            if (setNUMERIC_ROUNDABORT)          children.Add(new Node.Node_CustomNode("    SET NUMERIC_ROUNDABORT OFF;\r\n"));
            if (setTRANSACTION_ISOLATION_LEVEL) children.Add(new Node.Node_CustomNode("    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;\r\n"));
            if (addLine)                        children.Add(new Node.Node_CustomNode("\r\n"));

            statements.InsertRangeChild(0, children);
        }
    }
}
