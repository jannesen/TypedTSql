using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177673.aspx
    public class Query_Select_GroupBy: Core.AstParseNode
    {
        public      readonly    IExprNode[]                     n_Items;

        public                                                  Query_Select_GroupBy(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.GROUP);
            ParseToken(reader, Core.TokenID.BY);

            var items = new List<IExprNode>();

            do {
                items.Add(ParseExpression(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Items = items.ToArray();
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Items.TranspileNodes(context);
        }
    }
}
