using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms187926.aspx
    //      CREATE { PROC | PROCEDURE } Objectname
    //          [ { @parameter [ type_schema_name. ] data_type } [ VARYING ] [ = default ] [ OUT | OUTPUT | [READONLY] ] [ ,...n ]
    //      [ WITH <procedure_option> [ ,...n ] ]
    //      AS
    //      { [ BEGIN ] sql_statement [ ...n ] [ END ] }
    [DeclarationParser(Core.TokenID.PROCEDURE)]
    public class Declaration_PROCEDURE: DeclarationObjectCode
    {
        public      override    DataModel.SymbolType            EntityType
        {
            get {
                return _procedureType;
            }
        }
        public      override    DataModel.EntityName            EntityName
        {
            get {
                return n_Name.n_EntitiyName;
            }
        }
        public      override    bool                            callableFromCode            { get { return true; } }

        public      readonly    Node_EntityNameDefine           n_Name;
        public      readonly    Node_External                   n_Node_External;

        public      override    ObjectReturnOption              ReturnOption                { get { return ObjectReturnOption.Optional;  } }
        public      override    DataModel.ISqlType              ReturnType                  { get { return DataModel.SqlTypeNative.Int;  } }

        private                 DataModel.SymbolType            _procedureType;

        public                                                  Declaration_PROCEDURE(Core.ParserReader reader, IParseContext parseContext)
        {
            AddLeading(reader);
            AddChild(new Node.Node_CustomNode("CREATE "));
            ParseToken(reader, Core.TokenID.PROCEDURE);
            n_Name = AddChild(new Node_EntityNameDefine(reader));
            ParseParameters(reader, Node_SqlParameter.InterfaceType.Procedure);
            ParseWith(reader, DataModel.SymbolType.StoredProcedure);
            ParseGrant(reader, DataModel.SymbolType.StoredProcedure);

            ParseOptionalAS(reader);

            switch(reader.CurrentToken.validateToken(Core.TokenID.BEGIN, Core.TokenID.EXTERNAL)) {
            case Core.TokenID.BEGIN:
                _procedureType = DataModel.SymbolType.StoredProcedure;
                ParseStatementBlock(reader, true);
                break;

            case Core.TokenID.EXTERNAL:
                _procedureType = DataModel.SymbolType.StoredProcedure_clr;
                n_Node_External = AddChild(new Node_External(reader));
                break;
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Name.TranspileNode(context);
                n_Parameters?.TranspileNode(context);
                TranspileOptions(context);

                Entity.Transpiled(parameters: n_Parameters?.t_Parameters);
                n_Name.n_Name.SetSymbolUsage(Entity, DataModel.SymbolUsageFlags.Declaration);
                _declarationTranspiled = true;
            }

            if (n_Node_External != null)
                n_Node_External.TranspileNode(context);
            else
                TranspileStatement(context);

            Transpiled = true;
        }

        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(n_Name.n_EntitiyName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('P','PC'))");
            stringWriter.Write("    DROP PROCEDURE ");
                stringWriter.WriteLine(n_Name.n_EntitiyName.Fullname);
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "procedure " + n_Name.n_EntitiyName.Name;
        }
    }
}
