using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms181299.aspx
    [StatementParser(Core.TokenID.ROLLBACK)]
    public class Statement_ROLLBACK: Statement
    {
        public                                                      Statement_ROLLBACK(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.ROLLBACK);
            ParseOptionalToken(reader, Core.TokenID.TRAN, Core.TokenID.TRANSACTION);
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.ScopeIndentityType = null;
        }
    }
}
