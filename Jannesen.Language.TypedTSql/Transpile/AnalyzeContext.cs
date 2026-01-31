using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class AnalyzeContext
    {
        public  readonly        Transpiler                          Transpiler;
        public  readonly        GlobalCatalog                       Catalog;
        public  readonly        SourceFile                          SourceFile;

        public                                                      AnalyzeContext(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile)
        {
            Transpiler = transpiler;
            Catalog    = catalog;
            SourceFile = sourceFile;
        }

        public                  void                                AddError(Core.IAstNode node, Exception err)
        {
            if (err is TranspileException)
                AddError(((TranspileException)err).Node, TypedTSqlTranspileError.ErrorToString(err), ((TranspileException)err).QuickFix);
            else
                AddError(node, TypedTSqlTranspileError.ErrorToString(err), null);
        }
        public                  void                                AddError(Core.IAstNode node, string error, QuickFix quickFix=null)
        {
            SourceFile.AddTranspileMessage(new TypedTSqlTranspileError(SourceFile, node, error, quickFix));
        }
    }
}
