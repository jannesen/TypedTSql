using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188929.aspx
    [StatementParser(Core.TokenID.BEGIN, prio:2)]
    public class Statement_BEGIN_TRANSACTION: Statement
    {
        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.BEGIN && reader.NextPeek().isToken(Core.TokenID.TRAN, Core.TokenID.TRANSACTION);
        }
        public                                                      Statement_BEGIN_TRANSACTION(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, Core.TokenID.TRAN, Core.TokenID.TRANSACTION);
            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.ScopeIndentityType = null;
        }
    }
}
