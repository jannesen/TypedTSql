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
    [StatementParser(Core.TokenID.EXEC,    prio:3)]
    [StatementParser(Core.TokenID.EXECUTE, prio:3)]
    public class Statement_EXECUTE_expression: Statement
    {
        public      readonly    Core.IAstNode[]                     n_ExecuteExpression;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken(Core.TokenID.EXEC, Core.TokenID.EXECUTE) && reader.NextPeek().isToken(Core.TokenID.LrBracket);
        }
        public                                                      Statement_EXECUTE_expression(Core.ParserReader reader, IParseContext parseContext): this(reader, parseContext, true)
        {
        }
        public                                                      Statement_EXECUTE_expression(Core.ParserReader reader, IParseContext parseContext, bool statement=false)
        {
            ParseToken(reader, Core.TokenID.EXEC, Core.TokenID.EXECUTE);
            ParseToken(reader, Core.TokenID.LrBracket);

            var executeExpression = new List<Core.IAstNode>();

            do {
                switch(reader.CurrentToken.validateToken(Core.TokenID.LocalName, Core.TokenID.String)) {
                case Core.TokenID.LocalName:
                    executeExpression.Add(AddChild(new Expr_Variable(reader)));
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

            if (statement) { 
                ParseStatementEnd(reader, parseContext);
            }
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            foreach(var e in n_ExecuteExpression) {
                if (e is IExprNode exprNode)
                    exprNode.TranspileNode(context);
            }
        }
    }
}
