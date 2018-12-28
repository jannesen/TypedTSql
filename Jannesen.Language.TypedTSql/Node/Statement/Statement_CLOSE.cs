using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms175035.aspx
    //  Statement_CLOSE ::=
    //      { { [ GLOBAL ] cursor_name } | cursor_variable_name }
    [StatementParser(Core.TokenID.CLOSE)]
    public class Statement_CLOSE: Statement
    {
        public      readonly    Node_CursorName                     n_Cursor;

        public                                                      Statement_CLOSE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.CLOSE);
            n_Cursor = AddChild(new Node_CursorName(reader));
            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Cursor.TranspileNode(context);
        }
    }
}
