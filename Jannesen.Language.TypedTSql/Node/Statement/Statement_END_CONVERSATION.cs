using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/end-conversation-transact-sql
    [StatementParser(Core.TokenID.END, prio:2)]
    public class Statement_END_CONVERSATION: Statement
    {
        public      readonly    IExprNode                           n_conversation_handle;
        public      readonly    IExprNode                           n_failure_code;
        public      readonly    IExprNode                           n_failure_text;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.END && reader.NextPeek().isToken("CONVERSATION");
        }
        public                                                      Statement_END_CONVERSATION(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.END);
            ParseToken(reader, "CONVERSATION");
            n_conversation_handle  = ParseSimpleExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                if (ParseOptionalToken(reader, "ERROR") != null) {
                    ParseToken(reader, Core.TokenID.Equal);
                    n_failure_code  = ParseSimpleExpression(reader);
                    ParseToken(reader, "DESCRIPTION");
                    ParseToken(reader, Core.TokenID.Equal);
                    n_failure_text  = ParseSimpleExpression(reader);
                }
                else if (ParseOptionalToken(reader, "CLEANUP") != null) {
                }
                else {
                    throw new ParseException(reader.CurrentToken, "Expect ERROR or CLEANUP");
                }
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_conversation_handle.TranspileNode(context);
            n_failure_code?.TranspileNode(context);
            n_failure_text?.TranspileNode(context);

            Logic.Validate.ValueUniqueIdentifier(n_conversation_handle);

            Logic.Validate.ValueType(n_failure_code, (t) => {
                if (t != DataModel.SystemType.Int) {
                    return "Expect int.";
                }
                return null;
            });
            Logic.Validate.ValueType(n_failure_text, (t) => {
                if (t != DataModel.SystemType.NVarChar || t != DataModel.SystemType.NChar) {
                    return "Expect nvarchar.";
                }
                return null;
            });
        }
    }
}
