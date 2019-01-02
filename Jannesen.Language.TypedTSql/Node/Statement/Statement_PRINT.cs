using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms176047.aspx
    // Statement_PRINT  ::=
    //      PPRINT msg_str | @local_variable | string_expr
    [StatementParser(Core.TokenID.PRINT)]
    public class Statement_PRINT: Statement
    {
        public      readonly    IExprNode                           n_Expression;

        public                                                      Statement_PRINT(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.PRINT);
            n_Expression = ParseExpression(reader);

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
        }
    }
}
