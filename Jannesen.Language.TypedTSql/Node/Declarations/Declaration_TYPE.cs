using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms175007.aspx
    //  CREATE TYPE [ schema_name. ] type_name
    //  FROM base_type [ ( precision [ , scale ] ) ] [ NULL | NOT NULL ]
    //  | EXTERNAL NAME assembly_name [ .class_name ]
    //  | AS TABLE ( { <column_definition> | <computed_column_definition> } [ <table_constraint> ] [ ,...n ] )
    [DeclarationParser(Core.TokenID.TYPE)]
    public class Declaration_TYPE: DeclarationEntity
    {
        public      override    DataModel.SymbolType            EntityType
        {
            get {
                return n_TypeDeclaration.EntityType;
            }
        }
        public      override    DataModel.EntityName            EntityName
        {
            get {
                return n_Name.n_EntitiyName;
            }
        }

        public      override    bool                            callableFromCode                    { get { return false; } }

        public      readonly    Node_EntityNameDefine           n_Name;
        public      readonly    TypeDeclaration                 n_TypeDeclaration;

        public                  DataModel.EntityType            Entity                              { get { return n_TypeDeclaration.Entity; } }

        public                                                  Declaration_TYPE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.TYPE);

            n_Name = AddChild(new Node_EntityNameDefine(reader));

            switch(reader.CurrentToken.validateToken("FROM", "EXTERNAL", "AS"))
            {
            case "FROM":        n_TypeDeclaration = AddChild(new TypeDeclaration_User(reader));     break;
            case "EXTERNAL":    n_TypeDeclaration = AddChild(new TypeDeclaration_External(reader)); break;
            case "AS":          n_TypeDeclaration = AddChild(new TypeDeclaration_Table(reader));    break;
            }
        }

        public      override    void                            TranspileInit(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile)
        {
            Transpiled             = false;
            _declarationTranspiled = false;

            n_TypeDeclaration.TranspileInit(this, catalog, sourceFile);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Name.TranspileNode(context);
                n_TypeDeclaration.TranspileNode(context);
                _declarationTranspiled = true;
            }

            n_TypeDeclaration.Transpiled();
            n_Name.n_Name.SetSymbol(Entity);

            Transpiled = true;
        }
        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            n_TypeDeclaration.Emit(emitWriter, this);
        }
        public      override    bool                            EmitInstallInto(EmitContext emitContext, int step)
        {
            return n_TypeDeclaration.EmitInstallInto(emitContext, step);
        }
        public      override    void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            n_TypeDeclaration.EmitGrant(emitContext, sourceFile);
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "type " + n_Name.n_EntitiyName.Name;
        }
    }
}
