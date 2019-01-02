using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-US/library/ms187956.aspx
    //      CREATE VIEW [ schema_name . ] view_name [ (column [ ,...n ] ) ]
    //      [ WITH { ENCRYPTION | SCHEMABINDING  | VIEW_METADATA } [ ,...n ] ]
    //      AS select_statement
    //      [ WITH CHECK OPTION ]
    [DeclarationParser(Core.TokenID.VIEW)]
    public class Declaration_VIEW: DeclarationObjectCode
    {
        public      override    DataModel.SymbolType            EntityType
        {
            get {
                return DataModel.SymbolType.View;
            }
        }
        public      override    DataModel.EntityName            EntityName
        {
            get {
                return n_Name.n_EntitiyName;
            }
        }

        public      override    bool                            callableFromCode                { get { return true; } }

        public      readonly    Node_EntityNameDefine           n_Name;

        public                                                  Declaration_VIEW(Core.ParserReader reader, IParseContext parseContext)
        {
            AddLeading(reader);
            AddChild(new Node.Node_CustomNode("CREATE "));
            ParseToken(reader, Core.TokenID.VIEW);
            n_Name = AddChild(new Node_EntityNameDefine(reader));

            ParseWith(reader, DataModel.SymbolType.View);
            ParseGrant(reader, DataModel.SymbolType.View);

            ParseToken(reader, Core.TokenID.AS);

            ParseStatementQuery(reader, Query_SelectContext.StatementView);

            if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                ParseToken(reader, Core.TokenID.CHECK);
                ParseToken(reader, Core.TokenID.OPTION);
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (!_declarationTranspiled) {
                n_Name.TranspileNode(context);
                TranspileOptions(context);
                _declarationTranspiled = true;
            }

            TranspileStatement(context, query:true);

            Entity.Transpiled(returns: new DataModel.SqlTypeTable(Entity, ((Query_Select)n_Statement).Resultset?.GetUniqueNamedList(), null));
            n_Name.n_Name.SetSymbol(Entity);

            Transpiled = true;
        }
        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(n_Name.n_EntitiyName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('V'))");
            stringWriter.Write("    DROP VIEW ");
                stringWriter.WriteLine(n_Name.n_EntitiyName.Fullname);
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "view " + n_Name.n_EntitiyName.Name;
        }
    }
}
