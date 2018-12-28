using System;
using System.IO;
using System.Text;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class DeclarationEntity: Declaration
    {
        public      abstract    DataModel.SymbolType        EntityType          { get; }
        public      abstract    DataModel.EntityName        EntityName          { get; }
        public      abstract    bool                        callableFromCode    { get; }
        public      virtual     DeclarationService          DeclarationService  { get { throw new InvalidOperationException(EntityName + " is not a service");      } }

        public                  bool                        Transpiled      { get; protected set; }

        protected               bool                        _declarationTranspiled;

        public      virtual     DataModel.EntityName[]      ObjectReferences()
        {
            return null;
        }

        public      virtual     void                        EmitDrop(StringWriter stringWriter)
        {
        }
        public      virtual     bool                        EmitInstallInto(EmitContext emitContext, int step)
        {
            return true;
        }
        public      virtual     bool                        EmitCode(EmitContext emitContext, SourceFile sourceFile)
        {
            emitContext.Database.Print("# create " + EntityType.ToString().ToLower().PadRight(30, ' ') + " " + EntityName.ToString());
            var emitWriter = new Core.EmitWriterSourceMap(emitContext, sourceFile.Filename, Children.FirstNoWhithspaceToken.Beginning.Lineno);

            Emit(emitWriter);

            return emitContext.Database.ExecuteStatement(emitWriter.GetSql(), emitWriter.SourceMap, emitContext.AddEmitError) == 0;
        }
        public      virtual     void                        EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
        }
    }
}
