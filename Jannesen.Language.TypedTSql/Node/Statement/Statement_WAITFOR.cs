using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms187331.aspx
    [StatementParser(Core.TokenID.WAITFOR)]
    public class Statement_WAITFOR: Statement
    {
        public                                                      Statement_WAITFOR(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.WAITFOR);
            ParseToken(reader, "TIME", "DELAY");
            ParseToken(reader, Core.TokenID.String);

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
        }
    }
}
