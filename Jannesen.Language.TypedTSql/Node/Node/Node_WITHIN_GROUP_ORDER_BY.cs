using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188385.aspx
    public class Node_WITHIN_GROUP_ORDER_BY: Core.AstParseNode
    {
        public      readonly    Query_Select_OrderByItem[]      n_Items;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("WITHIN") && reader.NextPeek().isToken(Core.TokenID.GROUP);
        }
        public                                                  Node_WITHIN_GROUP_ORDER_BY(Core.ParserReader reader)
        {
            ParseToken(reader, "WITHIN");
            ParseToken(reader, Core.TokenID.GROUP);
            ParseToken(reader, Core.TokenID.LrBracket);
            ParseToken(reader, Core.TokenID.ORDER);
            ParseToken(reader, Core.TokenID.BY);

            {
                var items = new List<Query_Select_OrderByItem>();

                do {
                    items.Add(AddChild(new Query_Select_OrderByItem(reader)));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                n_Items = items.ToArray();
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Items.TranspileNodes(context);
        }
    }
}
