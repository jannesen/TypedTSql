using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-US/library/ms190487.aspx
    // Statement_BEGIN_END ::=
    //  BEGIN { sql_statement } [...n] END
    [StatementParser(Core.TokenID.BEGIN)]
    public class Statement_BEGIN_END: Statement
    {
        public      readonly    StatementBlock                      n_Statements;

        public                                                      Statement_BEGIN_END(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);

            n_Statements = AddChild(new StatementBlock());

            if (!n_Statements.Parse(reader, parseContext, (r) => (r.CurrentToken.isToken(Core.TokenID.END)) )) {
                reader.AddError(new Exception("Missing END."));
                return;
            }

            ParseToken(reader, Core.TokenID.END);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Statements.TranspileNode(context);
        }
    }
}
