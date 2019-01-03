using System;
using System.IO;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class Declaration: Core.AstParseNode
    {
        public      abstract    void                        TranspileInit(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile);

        public      abstract    Core.IAstNode               GetNameToken();
        public      abstract    string                      CollapsedName();
    }
}
