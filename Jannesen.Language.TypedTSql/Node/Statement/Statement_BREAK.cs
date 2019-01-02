using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-US/library/ms181271.aspx
    [StatementParser(Core.TokenID.BREAK)]
    public class Statement_BREAK: Statement
    {
        public                                                      Statement_BREAK(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BREAK);
            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
        }
    }
}
