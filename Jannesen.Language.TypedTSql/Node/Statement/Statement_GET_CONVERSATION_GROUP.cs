using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/get-conversation-group-transact-sql
    [StatementParser(Core.TokenID.Name, prio:3)]
    public class Statement_GET_CONVERSATION_GROUP: Statement
    {
        public      readonly    Node_AssignVariable                 n_ConversationGroupId;
        public      readonly    Node_EntityNameReference            n_Queue;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("GET") && reader.NextPeek().isToken("CONVERSATION");
        }
        public                                                      Statement_GET_CONVERSATION_GROUP(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, "GET");
            ParseToken(reader, "CONVERSATION");
            ParseToken(reader, "GROUP");
            n_ConversationGroupId = ParseVarVariable(reader);
            ParseToken(reader, Core.TokenID.FROM);
            n_Queue = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Queue, DataModel.SymbolUsageFlags.Select));
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_ConversationGroupId.TranspileAssign(context, DataModel.SqlTypeNative.UniqueIdentifier);
            n_Queue.TranspileNode(context);
        }
    }
}
