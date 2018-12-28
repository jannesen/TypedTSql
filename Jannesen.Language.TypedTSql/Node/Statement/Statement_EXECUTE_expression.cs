using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188332.aspx
    //      { EXEC | EXECUTE }
    //          [ @return_status = ]
    //          Objectname
    //          [ [ @parameter = ] { value
    //                             | @variable [ OUTPUT ]
    //                             | [ DEFAULT ]
    //                             }
    //          ] [ ,...n ]
    //      { EXEC | EXECUTE }
    //      ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    //      [ AS { LOGIN | USER } = ' name ' ]
    [StatementParser(Core.TokenID.EXEC,    prio:2)]
    [StatementParser(Core.TokenID.EXECUTE, prio:2)]
    public class Statement_EXECUTE_expression: Statement
    {
        public      readonly    Core.IAstNode[]                     n_ExecuteExpression;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.EXEC, Core.TokenID.EXECUTE)) {
                Core.Token[]        peek = reader.Peek(2);

                return peek[1].isToken(Core.TokenID.LrBracket);
            }

            return false;
        }
        public                                                      Statement_EXECUTE_expression(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.EXEC, Core.TokenID.EXECUTE);
            ParseToken(reader, Core.TokenID.LrBracket);

            var executeExpression = new List<Core.IAstNode>();

            do {
                switch(reader.CurrentToken.validateToken(Core.TokenID.LocalName, Core.TokenID.String))
                {
                case Core.TokenID.LocalName:
                    executeExpression.Add(AddChild(new Expr_PrimativeValue(reader, true)));
                    break;

                case Core.TokenID.String:
                    executeExpression.Add(ParseToken(reader));
                    break;
                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Plus) != null);

            n_ExecuteExpression = executeExpression.ToArray();

            ParseToken(reader, Core.TokenID.EXEC, Core.TokenID.RrBracket);

            if (ParseOptionalToken(reader, Core.TokenID.AS) != null) {
                ParseToken(reader, "LOGIN", "USER");
                ParseToken(reader, Core.TokenID.Equal);
                ParseToken(reader, Core.TokenID.String);
            }

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            foreach(var e in n_ExecuteExpression) {
                if (e is Expr_PrimativeValue primativeValue)
                    primativeValue.TranspileNode(context);
            }
        }
    }
}
