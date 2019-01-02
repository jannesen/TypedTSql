using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188385.aspx
    public class Query_Select_OrderBy: Core.AstParseNode
    {
        public      readonly    Query_Select_OrderByItem[]      n_Items;
        public      readonly    IExprNode                       n_Offset;
        public      readonly    IExprNode                       n_Rows;

        public                                                  Query_Select_OrderBy(Core.ParserReader reader)
        {
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

            if (ParseOptionalToken(reader, "OFFSET") != null) {
                n_Offset = ParseExpression(reader);
                ParseToken(reader, "ROW", "ROWS");

                if (ParseOptionalToken(reader, Core.TokenID.FETCH) != null) {
                    ParseToken(reader, "FIRST", "NEXT");
                    n_Rows = ParseExpression(reader);
                    ParseToken(reader, "ROW", "ROWS");
                    ParseToken(reader, "ONLY");
                }
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Items.TranspileNodes(context);
            n_Offset?.TranspileNode(context);
            n_Rows?.TranspileNode(context);
        }
    }
}
