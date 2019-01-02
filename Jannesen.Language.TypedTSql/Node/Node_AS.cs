using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_AS: Core.AstParseNode
    {
        public      readonly    Token.DataIsland                    n_AsType;
        public                  object                              AsType                  { get; private set; }

        public                                                      Node_AS(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.AS);
            n_AsType = (Token.DataIsland)ParseToken(reader, Core.TokenID.DataIsland);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            AsType = null;

            try {
                AsType = context.DeclarationEntity.DeclarationService.TranspilseNodeAS(this);
            }
            catch(Exception err) {
                context.AddError(n_AsType, err);
            }
        }
        public      override    void                                Emit(Core.EmitWriter emitWriter)
        {
            EmitCommentNewine(emitWriter);
        }
    }
}
