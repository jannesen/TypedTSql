using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms188782.aspx
    //  Statement_DEALLOCATE ::=
    //      { { [ GLOBAL ] cursor_name } | cursor_variable_name }
    [StatementParser(Core.TokenID.DEALLOCATE)]
    public class Statement_DEALLOCATE: Statement
    {
        public      readonly    Node_CursorName                     n_Cursor;

        public                                                      Statement_DEALLOCATE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DEALLOCATE);
            n_Cursor = AddChild(new Node_CursorName(reader));
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Cursor.TranspileNode(context);
        }
    }
}
