using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/nl-nl/library/ms178642.aspx
    //  Statement_WHILE ::=
    //      WHILE Boolean_expression
    //           { sql_statement | statement_block | BREAK | CONTINUE }
    [StatementParser(Core.TokenID.WHILE)]
    public class Statement_WHILE: Statement
    {
        public      readonly    IExprNode                           n_Test;
        public      readonly    Statement                           n_WhileStatement;

        public                                                      Statement_WHILE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.WHILE);
            n_Test = ParseExpression(reader);
            n_WhileStatement = AddChild(parseContext.StatementParse(reader));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Test.TranspileNode(context);
                Validate.BooleanExpression(n_Test);
            }
            catch(Exception err) {
                context.AddError(n_Test, err);
            }

            try {
                context.ScopeIndentityType = null;
                n_WhileStatement.TranspileNode(context);
            }
            catch(Exception err) {
                context.AddError(n_WhileStatement, err);
            }
        }
    }
}
