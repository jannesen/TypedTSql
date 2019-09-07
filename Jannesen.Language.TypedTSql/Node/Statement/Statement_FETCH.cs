using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms180152.aspx
    // Statement_FETCH ::=
    //      FETCH [ [ NEXT | PRIOR | FIRST | LAST
    //              | ABSOLUTE { n | @nvar }
    //              | RELATIVE { n | @nvar }
    //            ]
    //            FROM
    //           ]
    //      { { [ GLOBAL ] cursor_name } | @cursor_variable_name }
    //      [ INTO @variable_name [ ,...n ] ]
    [StatementParser(Core.TokenID.FETCH)]
    public class Statement_FETCH: Statement
    {
        public      readonly    Node_CursorName                     n_Cursor;
        public      readonly    IExprNode                           n_Position;
        public      readonly    ISetVariable[]                      n_VariableNames;

        public                                                      Statement_FETCH(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.FETCH);

            Core.Token token = ParseOptionalToken(reader, "NEXT", "PRIOR", "FIRST", "LAST", "ABSOLUTE", "RELATIVE");

            if (token != null) {
                switch (token.Text.ToUpperInvariant()) {
                case "ABSOLUTE":
                case "RELATIVE":
                    n_Position = ParseExpression(reader);
                    break;
                }

                ParseOptionalToken(reader, Core.TokenID.FROM);
            }

            n_Cursor = AddChild(new Node_CursorName(reader));

            ParseToken(reader, Core.TokenID.INTO);

            var setvars = new List<ISetVariable>();

            do {
                setvars.Add(ParseSetVariable(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_VariableNames = setvars.ToArray();

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Cursor.TranspileNode(context);
            n_Position?.TranspileNode(context);

            if (n_Cursor.Cursor != null) {
                if (n_VariableNames.Length < n_Cursor.Cursor.Columns.Length) {
                    context.AddError(this, "Missing variable, expect " + n_Cursor.Cursor.Columns.Length + ".");
                    return;
                }
                if (n_VariableNames.Length > n_Cursor.Cursor.Columns.Length) {
                    context.AddError(this, "To many variable, expect " + n_Cursor.Cursor.Columns.Length + ".");
                    return;
                }

                for (int i = 0 ; i < n_VariableNames.Length ; ++i)
                    context.VariableSet(n_VariableNames[i], n_Cursor.Cursor.Columns[i]);
            }
        }
    }
}
