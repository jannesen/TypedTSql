using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/begin-dialog-conversation-transact-sql
    [StatementParser(Core.TokenID.BEGIN, prio:3)]
    public class Statement_BEGIN_DIALOG: Statement
    {
        public      readonly    Node_AssignVariable                 n_DialogHandle;
        public      readonly    Core.Token                          n_InitiatorServiceName;
        public      readonly    Core.Token                          n_TargetServiceName;
        public      readonly    Core.Token                          n_ServiceBrokerGuid;
        public      readonly    Core.Token                          n_ContractName;
        public      readonly    IExprNode                           n_RelatedConversationHandle;
        public      readonly    IExprNode                           n_RelatedConversationGroupId;
        public      readonly    IExprNode                           n_DialogLifetime;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.BEGIN && reader.NextPeek().isToken("DIALOG");
        }
        public                                                      Statement_BEGIN_DIALOG(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, "DIALOG");
            ParseOptionalToken(reader, "CONVERSATION");
            n_DialogHandle = ParseVarVariable(reader);

            ParseToken(reader, Core.TokenID.FROM);
            ParseToken(reader, "SERVICE");
            n_InitiatorServiceName = ParseToken(reader, Core.TokenID.QuotedName);

            ParseToken(reader, Core.TokenID.TO);
            ParseToken(reader, "SERVICE");
            n_TargetServiceName = ParseToken(reader, Core.TokenID.String);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                n_ServiceBrokerGuid = ParseToken(reader, Core.TokenID.String);
            }

            if (ParseOptionalToken(reader, Core.TokenID.ON) != null) {
                ParseToken(reader, "CONTRACT");
                n_ContractName = ParseToken(reader, Core.TokenID.QuotedName);
            }

            if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                do {
                    switch(reader.CurrentToken.Text.ToUpperInvariant()) {
                    case "RELATED_CONVERSATION":
                        ParseToken(reader);
                        ParseToken(reader, Core.TokenID.Equal);
                        n_RelatedConversationHandle = ParseSimpleExpression(reader);
                        break;
                    case "RELATED_CONVERSATION_GROUP":
                        ParseToken(reader);
                        ParseToken(reader, Core.TokenID.Equal);
                        n_RelatedConversationGroupId = ParseSimpleExpression(reader);
                        break;
                    case "LIFETIME":
                        ParseToken(reader);
                        ParseToken(reader, Core.TokenID.Equal);
                        n_DialogLifetime = ParseSimpleExpression(reader);
                        break;
                    case "ENCRYPTION":
                        ParseToken(reader);
                        ParseToken(reader, Core.TokenID.Equal);
                        ParseToken(reader, "ON", "OFF");
                        break;

                    default:
                        throw new ParseException(reader.CurrentToken, "Expect RELATED_CONVERSATION,RELATED_CONVERSATION_GROUP,LIFETIME or ENCRYPTION.");
                    }
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_DialogHandle.TranspileAssign(context, DataModel.SqlTypeNative.UniqueIdentifier);
            Core.TokenWithSymbol.SetNoSymbol(n_InitiatorServiceName);
            Core.TokenWithSymbol.SetNoSymbol(n_TargetServiceName);
            Core.TokenWithSymbol.SetNoSymbol(n_ServiceBrokerGuid);
            Core.TokenWithSymbol.SetNoSymbol(n_ContractName);
            n_RelatedConversationHandle?.TranspileNode(context);
            n_RelatedConversationGroupId?.TranspileNode(context);
            n_DialogLifetime?.TranspileNode(context);
            Logic.Validate.ValueUniqueIdentifier(n_RelatedConversationHandle);
            Logic.Validate.ValueUniqueIdentifier(n_RelatedConversationGroupId);
            Logic.Validate.ValueInt(n_DialogLifetime);
        }
    }
}
