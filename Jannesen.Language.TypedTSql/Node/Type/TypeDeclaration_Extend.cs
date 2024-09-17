using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class TypeDeclaration_Extend: TypeDeclaration
    {
        public      override    DataModel.SymbolType            EntityType      => DataModel.SymbolType.TypeExtend;
        public      override    DataModel.EntityType            Entity          => _entity;
        public      override    bool                            HasEmitCode     => false;

        public      readonly    Node_EntityNameReference        n_ParentType;

        private                 DataModel.EntityTypeExtend      _entity;

        public                                                  TypeDeclaration_Extend(Core.ParserReader reader)
        {
            ParseToken(reader, "EXTEND");

            n_ParentType = new Node_EntityNameReference(reader, EntityReferenceType.UserDataType, DataModel.SymbolUsageFlags.Reference);
        }

        public      override    void                            TranspileInit(Transpile.TranspileContext transpileContext, Declaration_TYPE declaration, SourceFile sourceFile)
        {
            if ((_entity = transpileContext.Catalog.DefineTypeExtend(declaration.EntityName)) == null)
                throw new TranspileException(declaration.n_Name, "Duplicate definition of type.");

            _entity.TranspileInit(new DataModel.DocumentSpan(sourceFile.Filename, declaration));
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_ParentType.TranspileNode(context);

        }
        public      override    void                            Transpiled()
        {
            _entity.Transpiled((DataModel.EntityType)n_ParentType.Entity);
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter, Declaration_TYPE type)
        {
        }
    }
}
