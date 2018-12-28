using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms182717.aspx
    //  Statement_IF ::=
    //      IF Boolean_expression
    //           { sql_statement | statement_block }
    //      [ ELSE { sql_statement | statement_block }  ]
    [StatementParser(Core.TokenID.IF)]
    public class Statement_IF: Statement
    {
        public      readonly    IExprNode                           n_Test;
        public      readonly    Statement                           n_TrueStatement;
        public      readonly    Statement                           n_FalseStatement;

        public                                                      Statement_IF(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.IF);
            n_Test = ParseExpression(reader);
            n_TrueStatement = AddChild(parseContext.StatementParse(reader));

            if (ParseOptionalToken(reader, Core.TokenID.ELSE) != null) {
                n_FalseStatement = AddChild(parseContext.StatementParse(reader));
            }
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

            var   sitBegin    = context.ScopeIndentityType;
            var   sitEndTrue  = sitBegin;
            var   sitEndFalse = sitBegin;

            try {
                n_TrueStatement.TranspileNode(context);
                sitEndTrue = context.ScopeIndentityType;
            }
            catch(Exception err) {
                context.AddError(n_TrueStatement, err);
            }

            try {
                context.ScopeIndentityType = sitBegin;
                n_FalseStatement?.TranspileNode(context);
                sitEndFalse = context.ScopeIndentityType;
            }
            catch(Exception err) {
                context.AddError(n_FalseStatement, err);
            }

            context.ScopeIndentityType = sitEndTrue !=null && object.ReferenceEquals(sitEndTrue, sitEndFalse) ? sitEndTrue : null;
        }
    }
}
