﻿using System;
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
        public      readonly    Node_IntoVariables                  n_IntoVariables;

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

            n_Cursor = AddChild(new Node_CursorName(reader, DataModel.SymbolUsageFlags.Read));

            ParseToken(reader, Core.TokenID.INTO);
            n_IntoVariables = AddChild(new Node_IntoVariables(reader));

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Cursor.TranspileNode(context);
            n_Position?.TranspileNode(context);

            if (n_Cursor.Cursor != null) {
                if (n_IntoVariables.n_VariableNames.Length < n_Cursor.Cursor.Columns.Length) {
                    context.AddError(this, "Missing variable, expect " + n_Cursor.Cursor.Columns.Length + ".");
                    return;
                }
                if (n_IntoVariables.n_VariableNames.Length > n_Cursor.Cursor.Columns.Length) {
                    context.AddError(this, "To many variable, expect " + n_Cursor.Cursor.Columns.Length + ".");
                    return;
                }

                for (int i = 0 ; i < n_IntoVariables.n_VariableNames.Length ; ++i)
                    n_IntoVariables.n_VariableNames[i].TranspileAssign(context, n_Cursor.Cursor.Columns[i]);
            }
        }
    }
}
