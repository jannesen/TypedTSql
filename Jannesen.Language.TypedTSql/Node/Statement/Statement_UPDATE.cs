using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177523.aspx
    //      UPDATE { { { table_alias | <object> } [ WITH ( <Table_Hint_Limited> [ ...n ] ) ] } | @table_variable }
    //         SET { column_name { = | += | -= | *= | /= | %= | &= | ^= | |= } { expression | DEFAULT } } [ ,...n ]
    //      [ FROM{ <table_source> } ]
    //      [ WHERE { <search_condition>
    //            | { CURRENT OF { [ GLOBAL ] cursor_name } | cursor_variable_name } }
    //      [ OPTION ( <query_hint> [ ,...n ] ) ]
    [StatementParser(Core.TokenID.UPDATE)]
    public class Statement_UPDATE: Statement_DELETE_UPDATE
    {
        public class Update_SET: Core.AstParseNode
        {
            public      readonly        Core.TokenWithSymbol        n_Column;
            public      readonly        IExprNode                   n_Expression;
            public                      DataModel.Column            Column              { get; private set; }

            public                                                  Update_SET(Core.ParserReader reader)
            {
                n_Column = ParseName(reader);

                ParseToken(reader, Core.TokenID.Equal, Core.TokenID.PlusAssign, Core.TokenID.MinusAssign, Core.TokenID.MultAssign, Core.TokenID.DivAssign, Core.TokenID.ModAssign, Core.TokenID.AndAssign, Core.TokenID.XorAssign, Core.TokenID.OrAssign);

                if (reader.CurrentToken.isToken(Core.TokenID.DEFAULT))
                    ParseToken(reader, Core.TokenID.DEFAULT);
                else
                    n_Expression = ParseExpression(reader);
            }

            public      override        void                        TranspileNode(Transpile.Context context)
            {
                Column = null;

                n_Expression?.TranspileNode(context);

                if (context.Target != null) {
                    Column = context.Target.Columns.FindColumn(n_Column.ValueString, out bool ambiguous);

                    if (Column != null) {
                        n_Column.SetSymbol(Column);
                        Column.SetUsed();

                        if (ambiguous)
                            context.AddError(n_Column, "Column [" + n_Column.ValueString + "] is ambiguous.");
                        else
                        if ((Column.ValueFlags & (DataModel.ValueFlags.NULL      |
                                                  DataModel.ValueFlags.Const     |
                                                  DataModel.ValueFlags.Variable  |
                                                  DataModel.ValueFlags.Computed  |
                                                  DataModel.ValueFlags.Identity)) != 0)
                            context.AddError(n_Column, "Can't update [" + n_Column.ValueString + "] is .");
                        else
                            context.CaseWarning(n_Column, Column.Name);

                        if (n_Expression != null) {
                            try {
                                Validate.Assign(context, Column, n_Expression);
                            }
                            catch(Exception err) {
                                context.AddError(n_Column, err);
                            }
                        }
                    }
                    else
                        context.AddError(n_Column, "Unknown column '" + n_Column.ValueString + "' in '" + context.Target.Source.Name + "'.");
                }
                context.ScopeIndentityType = null;
            }
        }

        public      readonly    Update_SET[]                        n_Set;

        public                                                      Statement_UPDATE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.UPDATE);

            ParseTarget(reader);

            ParseToken(reader, Core.TokenID.SET);

            {
                var set = new List<Update_SET>();

                do {
                    set.Add(AddChild(new Update_SET(reader)));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                n_Set = set.ToArray();
            }

            ParseFromWhereOption(reader);

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            var contextStatement = new Transpile.ContextStatementQuery(context);

            TranspileOptions(contextStatement);

            var contextRowSet    = new Transpile.ContextRowSets(contextStatement);

            TranspileFromWhereExpression(contextStatement, contextRowSet);

            n_Set.TranspileNodes(contextRowSet);
        }
    }
}
