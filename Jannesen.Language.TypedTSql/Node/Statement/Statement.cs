using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class Statement: Core.AstParseNode
    {
        public                  bool                        Transpiled          { get; private set; }

        public                  void                        TranspileStatement(Transpile.ContextBlock contextStatementBlock)
        {
            try {
                TranspileNode(contextStatementBlock);
            }
            catch(Exception err) {
                contextStatementBlock.AddError(this, err);
            }
        }
    }

    public abstract class Statement_BEGINEND_TRYCATCH: Statement
    {
        public  override        void                        Emit(Core.EmitWriter emitWriter)
        {
            int indent = 1;

            foreach (var c in Children) {
                if (c is Token.Keyword) {
                    indent = emitWriter.Linepos;
                }

                if (c is Node.StatementBlock sb) {
                    sb.Emit(emitWriter, indent + 4);
                }
                else {
                    c.Emit(emitWriter);
                }
            }
        }
    }
}
