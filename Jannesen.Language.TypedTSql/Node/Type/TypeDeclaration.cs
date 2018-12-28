using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class TypeDeclaration: Core.AstParseNode
    {
        public      abstract    DataModel.SymbolType            EntityType      { get; }
        public      abstract    DataModel.EntityType            Entity          { get; }
        public      abstract    void                            TranspileInit(Declaration_TYPE declaration, GlobalCatalog catalog, SourceFile sourceFile);
        public      abstract    void                            Transpiled();
        public      abstract    void                            Emit(Core.EmitWriter emitWriter, Declaration_TYPE statement);

        public      virtual     void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
        }
        public      virtual     bool                            EmitInstallInto(EmitContext emitContext, int step)
        {
            return true;
        }
    }

    public abstract class TypeDeclarationWithGrant: TypeDeclaration
    {
        public                  Node_ObjectGrantList            n_GrantList;

        protected               void                            ParseGrant(Core.ParserReader reader)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.GRANT))
                n_GrantList = AddChild(new Node_ObjectGrantList(reader, EntityType));
        }
        public                  void                            TranspileGrant(Transpile.Context context)
        {
            n_GrantList?.TranspileNode(context);
        }

        public      override    void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            if (n_GrantList != null) {
                var emitWriter = new Core.EmitWriterSourceMap(emitContext, sourceFile.Filename, Children.FirstNoWhithspaceToken.Beginning.Lineno);

                n_GrantList.EmitGrant("TYPE", Entity.EntityName, emitWriter);
                emitContext.Database.ExecuteStatement(emitWriter.GetSql(), emitWriter.SourceMap, emitContext.AddEmitError);
            }
        }
    }
}
