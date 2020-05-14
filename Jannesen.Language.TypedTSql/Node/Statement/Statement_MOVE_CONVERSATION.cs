using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/move-conversation-transact-sql
    [StatementParser(Core.TokenID.Name, prio:3)]
    public class Statement_MOVE_CONVERSATION: Statement
    {
        public      readonly    IExprNode                           n_ConversationHndle;
        public      readonly    IExprNode                           n_ConversationGroupId;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("MOVE") && reader.NextPeek().isToken("CONVERSATION");
        }
        public                                                      Statement_MOVE_CONVERSATION(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, "MOVE");
            ParseToken(reader, "CONVERSATION");
            n_ConversationHndle  = ParseSimpleExpression(reader);
            ParseToken(reader, "TO");
            n_ConversationGroupId  = ParseSimpleExpression(reader);
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_ConversationHndle.TranspileNode(context);
            n_ConversationGroupId.TranspileNode(context);

            Logic.Validate.ValueUniqueIdentifier(n_ConversationHndle);
            Logic.Validate.ValueUniqueIdentifier(n_ConversationGroupId);
        }
    }
}
