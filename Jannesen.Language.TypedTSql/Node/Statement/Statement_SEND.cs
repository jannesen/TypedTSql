using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://docs.microsoft.com/en-us/sql/t-sql/statements/send-transact-sql
    [StatementParser(Core.TokenID.Name, prio:3)]
    public class Statement_SEND: Statement
    {
        public      readonly    IExprNode[]                         n_ConversationHandles;
        public      readonly    Core.Token                          n_MessageTypeName;
        public      readonly    IExprNode                           n_MessageBodyExpression;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("SEND");
        }
        public                                                      Statement_SEND(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, "SEND");
            ParseToken(reader, Core.TokenID.ON);
            ParseToken(reader, "CONVERSATION");

            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                var handlers = new List<IExprNode>();

                do {
                    handlers.Add(ParseSimpleExpression(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);

                n_ConversationHandles = handlers.ToArray();
            }
            else {
                n_ConversationHandles = new IExprNode[] { ParseSimpleExpression(reader) };
            }

            if (ParseOptionalToken(reader, "MESSAGE") != null) {
                ParseToken(reader, Core.TokenID.TYPE);
                n_MessageTypeName = ParseToken(reader, Core.TokenID.QuotedName);
            }

            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                n_MessageBodyExpression   = ParseExpression(reader);
                ParseToken(reader, Core.TokenID.RrBracket);
            }

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_ConversationHandles.TranspileNodes(context);
            n_MessageBodyExpression.TranspileNode(context);

            Core.TokenWithSymbol.SetNoSymbol(n_MessageTypeName);

            foreach(var h in n_ConversationHandles) { 
                Logic.Validate.ValueUniqueIdentifier(h);
            }
        }
    }
}
