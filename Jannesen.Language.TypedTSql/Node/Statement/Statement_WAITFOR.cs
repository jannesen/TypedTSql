using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms187331.aspx
    [StatementParser(Core.TokenID.WAITFOR)]
    public class Statement_WAITFOR: Statement, IParseContext
    {
        public      readonly    Statement                           n_ChildStatement;
        public      readonly    IExprNode                           n_Timeout;

        public                                                      Statement_WAITFOR(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.WAITFOR);

            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                if (Statement_GET_CONVERSATION_GROUP.CanParse(reader, this)) {
                    n_ChildStatement = AddChild(new Statement_GET_CONVERSATION_GROUP(reader, this));
                }
                if (Statement_RECEIVE.CanParse(reader, this)) {
                    n_ChildStatement = AddChild(new Statement_RECEIVE(reader, this));
                }
                else {
                    throw new ParseException(reader.CurrentToken, "Expect 'GET CONVERSATION GROUP' or 'RECEIVE'.");
                }

                ParseToken(reader, Core.TokenID.RrBracket);

                if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                    ParseToken(reader, "TIMEOUT");
                    n_Timeout = ParseSimpleExpression(reader);
                }
            }
            else {
                ParseToken(reader, "TIME", "DELAY");
                ParseToken(reader, Core.TokenID.String);
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_ChildStatement?.TranspileNode(context);
            n_Timeout?.TranspileNode(context);

            Logic.Validate.ValueInt(n_Timeout);
        }

                                Statement                           IParseContext.StatementParent                   => this;

                                bool                                IParseContext.StatementCanParse(Core.ParserReader reader)
        {
            return false;
        }
                                Statement                           IParseContext.StatementParse(Core.ParserReader reader)
        {
            throw new InvalidOperationException("Statement_WAITFOR IParseContext.StatementParse");
        }
    }
}
