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
}
