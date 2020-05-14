using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-US/library/ms189835.aspx
    //      DELETE [FROM] { { { table_alias | <object> } [ WITH ( <Table_Hint_Limited> [ ...n ] ) ] } | @table_variable }
    //      [ FROM{ <table_source> } ]
    //      [ WHERE { <search_condition>
    //            | { CURRENT OF { [ GLOBAL ] cursor_name } | cursor_variable_name } }
    //      [ OPTION ( <query_hint> [ ,...n ] ) ]
    [StatementParser(Core.TokenID.DELETE)]
    public class Statement_DELETE: Statement_DELETE_UPDATE
    {
        public                                                      Statement_DELETE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DELETE);
            ParseOptionalToken(reader, Core.TokenID.FROM);

            ParseTarget(reader);
            ParseFromWhereOption(reader);

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            var contextStatement = new Transpile.ContextStatementQuery(context);

            TranspileOptions(contextStatement);

            var contextRowSet    = new Transpile.ContextRowSets(contextStatement, true);

            TranspileFromWhereExpression(contextStatement, contextRowSet);
        }
    }
}
