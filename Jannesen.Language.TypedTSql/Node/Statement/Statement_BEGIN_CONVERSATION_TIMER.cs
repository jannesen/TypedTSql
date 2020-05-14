using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/begin-conversation-timer-transact-sql
    [StatementParser(Core.TokenID.BEGIN, prio:3)]
    public class Statement_BEGIN_CONVERSATION_TIMER: Statement
    {
        public      readonly    IExprNode                           n_ConversationHandle;
        public      readonly    IExprNode                           n_Timeout;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            var peek = reader.Peek(3);
            return peek[0].ID == Core.TokenID.BEGIN && peek[1].isToken("CONVERSATION") && peek[2].isToken("TIMER");
        }
        public                                                      Statement_BEGIN_CONVERSATION_TIMER(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, "CONVERSATION");
            ParseToken(reader, "TIMER");
            ParseToken(reader, Core.TokenID.LrBracket);
            n_ConversationHandle  = ParseSimpleExpression(reader);
            ParseToken(reader, Core.TokenID.RrBracket);
            ParseToken(reader, "TIMEOUT");
            ParseToken(reader, Core.TokenID.Equal);
            n_Timeout  = ParseSimpleExpression(reader);
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_ConversationHandle.TranspileNode(context);
            n_Timeout.TranspileNode(context);

            Logic.Validate.ValueUniqueIdentifier(n_ConversationHandle);

            Logic.Validate.ValueType(n_Timeout, (t) => {
                if (t != DataModel.SystemType.Int) {
                    return "Expect int.";
                }
                return null;
            });
        }
    }
}
