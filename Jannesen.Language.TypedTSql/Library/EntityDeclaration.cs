using System;
using System.IO;
using System.Text;

namespace Jannesen.Language.TypedTSql.Library
{
    public class EntityDeclaration
    {
        public                  SourceFile                      SourceFile      { get; private set; }
        public                  Node.Node_ParseOptions          Options         { get; private set; }
        public                  DataModel.SymbolType            EntityType      { get; private set; }
        public                  DataModel.EntityName            EntityName      { get; private set; }
        public                  Node.DeclarationEntity          Declaration     { get; private set; }

        public                                                  EntityDeclaration(SourceFile sourceFile, Node.Node_ParseOptions options, Node.DeclarationEntity declaration)
        {
            this.SourceFile  = sourceFile;
            this.Options     = options;
            this.EntityType  = declaration.EntityType;
            this.EntityName  = declaration.EntityName;
            this.Declaration = declaration;
        }

        public                  void                            Transpile(Transpiler transpiler, GlobalCatalog catalog, bool reportNeedTranspile,  ref bool transpiled, ref bool needtranspile)
        {
            if (!Declaration.Transpiled) {
                var context = new Transpile.ContextRoot(transpiler, SourceFile, catalog, Options, reportNeedTranspile, Declaration);

                try {
                    Declaration.TranspileNode(context);
                    transpiled = true;
                }
                catch(NeedsTranspileException) {
                    needtranspile = true;
                }
                catch(Exception err) {
                    context.AddError(Declaration, err);
                }
            }
        }
        public                  void                            EmitDrop(StringWriter stringWriter)
        {
            Declaration.EmitDrop(stringWriter);
        }
        public                  bool                            EmitInstallInto(EmitContext emitContext, int step)
        {
            return Declaration.EmitInstallInto(emitContext, step);
        }
        public                  bool                            EmitCode(EmitContext emitContext)
        {
            return Declaration.EmitCode(emitContext, SourceFile);
        }
        public                  void                            EmitGrant(EmitContext emitContext)
        {
            Declaration.EmitGrant(emitContext, SourceFile);
        }
    }
}
