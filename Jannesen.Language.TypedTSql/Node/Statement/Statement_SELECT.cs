using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms189499.aspx
    //  Statement_SELECT ::= <query_expression>
    //                       [ ORDER BY { order_by_expression | column_position [ ASC | DESC ] } [ ,...n ] ]
    //                       [ FOR forClause>]
    //                       [ OPTION ( <query_hint> [ ,...n ] ) ]
    [StatementParser(Core.TokenID.SELECT)]
    public class Statement_SELECT: Statement
    {
        public      readonly    Query_Select                        n_Select;
        public      readonly    Node_QueryOptions                   n_QueryOptions;

        public                                                      Statement_SELECT(Core.ParserReader reader, IParseContext parseContext)
        {
            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.StatementSelect));

            if (reader.CurrentToken.isToken(Core.TokenID.OPTION)) {
                n_QueryOptions = AddChild(new Node_QueryOptions(reader));
            }

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            var contextStatement = new Transpile.ContextStatementQuery(context);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Select.TranspileNode(contextStatement);
        }
    }
}
