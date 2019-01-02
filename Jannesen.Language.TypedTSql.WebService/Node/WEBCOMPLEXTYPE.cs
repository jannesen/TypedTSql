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
        public      override    LTTSQL.DataModel.SymbolType             EntityType                  { get { return LTTSQL.DataModel.SymbolType.ServiceComplexType;       } }
        public      override    LTTSQL.DataModel.EntityName             EntityName                  { get { return n_Declaration.n_EntityName;                           } }
        public      override    bool                                    callableFromCode            { get { return true; } }
        public      override    string                                  ComplexTypeName             { get { return n_Declaration.n_ServiceTypeName.n_Name.ValueString;   } }
        public      override    LTTSQL.Node.DeclarationService          DeclarationService          { get { return n_Declaration.n_ServiceTypeName.DeclarationService;   } }

        public      readonly    Declaration                             n_Declaration;
        public      readonly    LTTSQL.Node.Node_AS                     n_As;

        public                                                          WEBCOMPLEXTYPE(Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext): base(reader)
        {
            AddLeading(reader);
            n_Declaration = AddChild(new Declaration(reader));
            n_As          = AddChild(new LTTSQL.Node.Node_AS(reader));
            ParseParameters(reader, LTTSQL.Node.Node_SqlParameter.InterfaceType.Function);
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

                TranspileStatement(context, query:true);

                Entity.Transpiled(parameters: n_Parameters.t_Parameters, returns: ResponseNode.SqlType);
                n_Declaration.n_ServiceTypeName.n_Name.SetSymbol(Entity);

                _declarationTranspiled = true;
            }
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
    }
}
