using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms174366.aspx
    [StatementParser(Core.TokenID.CONTINUE)]
    public class Statement_CONTINUE: Statement
    {
        public                                                      Statement_CONTINUE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.CONTINUE);
            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
        }
    }
}
