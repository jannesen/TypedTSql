using System;
using System.Collections.Generic;
using System.IO;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class DeclarationService: DeclarationEntity, DataModel.ISymbol
    {
        public      override    DataModel.SymbolType                EntityType                                      { get { return DataModel.SymbolType.Service; } }
        public      override    DataModel.EntityName                EntityName                                      { get { return n_Name.n_EntitiyName;         } }
        public      override    bool                                callableFromCode                                { get { return false;                        } }

                                DataModel.SymbolType                DataModel.ISymbol.Type                          { get { return DataModel.SymbolType.Service; } }
                                string                              DataModel.ISymbol.Name                          { get { return EntityName.Name;              } }
                                string                              DataModel.ISymbol.FullName                   { get { return SqlStatic.QuoteName(EntityName.Name); } }
                                object                              DataModel.ISymbol.Declaration                   { get { return _declaration; } }
                                DataModel.ISymbol                   DataModel.ISymbol.ParentSymbol                  { get { return null; } }
                                DataModel.ISymbol                   DataModel.ISymbol.SymbolNameReference           { get { return null; } }

        public      readonly    Node_EntityNameDefine               n_Name;

        private                 object                              _declaration;

        protected                                                   DeclarationService(Core.ParserReader reader)
        {
            AddLeading(reader);
            Core.TokenWithSymbol.SetKeyword(ParseToken(reader));
            n_Name = AddChild(new Node_EntityNameDefine(reader));
            n_Name.n_Name.SetSymbolUsage(this, DataModel.SymbolUsageFlags.Declaration);
            _declaration = new DataModel.DocumentSpan(reader.SourceFile.Filename, this);
        }

        public      abstract    bool                                IsMember(DeclarationObjectCode method);

        public      override    void                                TranspileInit(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile)
        {
        }
        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Name.TranspileNode(context);
        }

        public      override    bool                                EmitCode(EmitContext emitContext, SourceFile sourceFile)
        {
            return true;
        }
        public      abstract    void                                EmitServiceFiles(EmitContext emitContext, Node.DeclarationServiceMethod[] methods, bool rebuild);

        public      override    Core.IAstNode                       GetNameToken()
        {
            return n_Name;
        }
    }

    public class DeclarationServiceList: Library.ListHash<DeclarationService, DataModel.EntityName>
    {
        public                                          DeclarationServiceList(int capacity): base(capacity)
        {
        }

        protected   override    DataModel.EntityName    ItemKey(DeclarationService item)
        {
            return item.EntityName;
        }
    }
}
