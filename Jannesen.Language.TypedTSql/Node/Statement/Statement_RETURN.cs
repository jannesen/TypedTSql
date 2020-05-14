using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms174998.aspx
    //  RETURN  [ Expression ]
    [StatementParser(Core.TokenID.RETURN)]
    public class Statement_RETURN: Statement
    {
        public      readonly    IExprNode                           n_Expression;

        public                                                      Statement_RETURN(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.RETURN);

            if (Expr.CanParse(reader))
                n_Expression = ParseExpression(reader);

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Expression?.TranspileNode(context);

            var declarationObject = context.GetDeclarationObjectCode();

            if (n_Expression != null) {
                if (declarationObject.ReturnOption == ObjectReturnOption.Nothing)
                    throw new Exception("Object returns nothing.");

                Validate.Assign(context, declarationObject.ReturnType, n_Expression);
            }
            else {
                if (declarationObject.ReturnOption == ObjectReturnOption.Required)
                    throw new Exception("Return value expexted.");
            }
        }
    }
}
