using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms190500.aspx
    //  Statement_OPEN ::=
    //      { { [ GLOBAL ] cursor_name } | cursor_variable_name }
    [StatementParser(Core.TokenID.OPEN)]
    public class Statement_OPEN: Statement
    {
        public      readonly    Node_CursorName                     n_Cursor;

        public                                                      Statement_OPEN(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.OPEN);
            n_Cursor = AddChild(new Node_CursorName(reader));
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Cursor.TranspileNode(context);
        }
    }
}
