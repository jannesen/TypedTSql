using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms189799.aspx
    //      CREATE TRIGGER Objectname ON Objectname
    //      [ WITH { ENCRYPTION | EXECUTE AS Clause } [ ,...n ] ]
    //      { FOR | AFTER | INSTEAD OF }
    //      { [ INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ] }
    //      AS sql_statement
    [DeclarationParser(Core.TokenID.TRIGGER)]
    public class Declaration_TRIGGER: DeclarationObjectCode
    {
        public      override    DataModel.SymbolType            EntityType
        {
            get {
                return DataModel.SymbolType.Trigger;
            }
        }
        public      override    DataModel.EntityName            EntityName
        {
            get {
                return n_Name.n_EntitiyName;
            }
        }

        public      override    bool                            callableFromCode            { get { return false; } }

        public      readonly    Node_EntityNameDefine           n_Name;
        public      readonly    Node_EntityNameReference        n_Table;

        public                                                  Declaration_TRIGGER(Core.ParserReader reader, IParseContext parseContext)
        {
            AddLeading(reader);
            AddChild(new Node.Node_CustomNode("CREATE "));
            ParseToken(reader, Core.TokenID.TRIGGER);
            n_Name = AddChild(new Node_EntityNameDefine(reader));
            ParseToken(reader, Core.TokenID.ON);
            n_Table = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.TableOrView));

            ParseWith(reader, DataModel.SymbolType.Trigger);

            switch(ParseToken(reader, "FOR", "AFTER", "INSTEAD").Text.ToUpper()) {
            case "INSTEAD":
                ParseToken(reader, Core.TokenID.OF);
                break;
            }

            do {
                ParseToken(reader, Core.TokenID.INSERT, Core.TokenID.UPDATE, Core.TokenID.DELETE);
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseOptionalAS(reader);
            ParseStatementBlock(reader, true);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Name.TranspileNode(context);
                n_Table.TranspileNode(context);
                n_Options?.TranspileNode(context);
                TranspileOptions(context);

                Entity.Transpiled();
                n_Name.n_Name.SetSymbol(Entity);
                _declarationTranspiled = true;
            }

            TranspileStatement(context);

            Transpiled = true;
        }
        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(n_Name.n_EntitiyName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('TR'))");
            stringWriter.Write("    DROP TRIGGER ");
                stringWriter.WriteLine(n_Name.n_EntitiyName.Fullname);
        }

        public      override    string                          CollapsedName()
        {
            return "trigger " + n_Name.n_EntitiyName.Name;
        }
        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
    }
}
