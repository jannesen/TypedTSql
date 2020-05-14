using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms190295.aspx
    [StatementParser(Core.TokenID.COMMIT)]
    public class Statement_COMMIT: Statement
    {
        public                                                      Statement_COMMIT(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.COMMIT);
            ParseOptionalToken(reader, Core.TokenID.TRAN, Core.TokenID.TRANSACTION);
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
        }
    }
}
