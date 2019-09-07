using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-US/library/ms175976.aspx
    //      BEGIN TRY
    //           { sql_statement | statement_block }
    //      END TRY
    //      BEGIN CATCH
    //           [ { sql_statement | statement_block } ]
    //      END CATCH
    [StatementParser(Core.TokenID.BEGIN, prio:2)]
    public class Statement_TRY_CATCH: Statement_BEGINEND_TRYCATCH
    {
        public      readonly    StatementBlock                      n_TryStatements;
        public      readonly    StatementBlock                      n_CatchStatements;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.BEGIN && reader.NextPeek().isToken(Core.TokenID.TRY);
        }
        public                                                      Statement_TRY_CATCH(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, Core.TokenID.TRY);
            n_TryStatements = AddChild(new StatementBlock());

            if (!n_TryStatements.Parse(reader, parseContext, (r) => (r.CurrentToken.isToken(Core.TokenID.END) && reader.NextPeek().isToken(Core.TokenID.TRY)) )) {
                reader.AddError(new Exception("Missing END TRY."));
                return;
            }

            ParseToken(reader, Core.TokenID.END);
            ParseToken(reader, Core.TokenID.TRY);

            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, Core.TokenID.CATCH);
            n_CatchStatements = AddChild(new StatementBlock());

            if (!n_CatchStatements.Parse(reader, parseContext, (r) => (r.CurrentToken.isToken(Core.TokenID.END) && reader.NextPeek().isToken(Core.TokenID.CATCH)) )) {
                reader.AddError(new Exception("Missing END CATCH."));
                return;
            }

            ParseToken(reader, Core.TokenID.END);
            ParseToken(reader, Core.TokenID.CATCH);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.ScopeIndentityType = null;
            n_TryStatements?.TranspileNode(context);
            context.ScopeIndentityType = null;
            n_CatchStatements?.TranspileNode(context);
            context.ScopeIndentityType = null;
        }
    }
}
