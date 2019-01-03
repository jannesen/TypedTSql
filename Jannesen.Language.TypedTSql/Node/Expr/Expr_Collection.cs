using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_Collection: Core.AstParseNode
    {
        private                 IExprNode[]         _zeroArch = new IExprNode[0];

        public      readonly    IExprNode[]         n_Expressions;

        // ( { expression } [,...n] )
        public                                      Expr_Collection(Core.ParserReader reader, bool atleastone)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            if (atleastone || !reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                var expressions = new List<IExprNode>();

                do {
                    expressions.Add(ParseExpression(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                n_Expressions = expressions.ToArray();
            }
            else
                n_Expressions = _zeroArch;

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                TranspileNode(Transpile.Context context)
        {
            n_Expressions?.TranspileNodes(context);
        }
    }
}
