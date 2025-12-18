using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188385.aspx
    public class Node_OVER: Core.AstParseNode
    {
        public      readonly    Expr_with_COLLATE[]             n_PartitionItems;
        public      readonly    Expr_with_COLLATE[]             n_OrderByItems;
        
        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("OVER") && reader.NextPeek().isToken(Core.TokenID.GROUP);
        }
        public                                                  Node_OVER(Core.ParserReader reader)
        {
            ParseToken(reader, "OVER");
            ParseToken(reader, Core.TokenID.LrBracket);

            if (ParseOptionalToken(reader, "PARTITION") != null) {
                ParseToken(reader, Core.TokenID.BY);
                n_PartitionItems = ParseItems(reader, (r) => new Expr_with_COLLATE(r));
            }

            ParseToken(reader, Core.TokenID.ORDER);
            ParseToken(reader, Core.TokenID.BY);
            n_OrderByItems = ParseItems(reader, (r) => new Expr_with_COLLATE(r));
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_PartitionItems?.TranspileNodes(context);
            n_OrderByItems?.TranspileNodes(context);
        }
    }
}
