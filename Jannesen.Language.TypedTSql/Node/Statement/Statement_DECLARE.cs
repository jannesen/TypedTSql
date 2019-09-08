using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188927.aspx
    // Statement_DECLARE ::= DECLARE declare_local (',' declare_local)* ';'?
    //                     | DECLARE CURSOR cursor_definition ';'?
    //                     | DECLARE LOCAL_ID table_type_definition ';'?
    [StatementParser(Core.TokenID.DECLARE, prio:2)]
    public class Statement_DECLARE: Statement
    {
        public class VariableTypeValue: Core.AstParseNode
        {
            public      readonly    Token.TokenLocalName        n_Name;
            public      readonly    Node_Datatype               n_Type;
            public      readonly    IExprNode                   n_Expression;
            public                  DataModel.VariableLocal     Variable            { get; private set; }

            public                                              VariableTypeValue(Core.ParserReader reader)
            {
                n_Name = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
                n_Type = AddChild(new Node_Datatype(reader));

                if (ParseOptionalToken(reader, Core.TokenID.Equal) != null) {
                    n_Expression = ParseExpression(reader);
                }
            }

            public      override    void                        TranspileNode(Transpile.Context context)
            {
                Variable = null;

                n_Type.TranspileNode(context);

                if (n_Type.SqlType != null) {
                    Variable = new DataModel.VariableLocal(n_Name.Text,
                                                           n_Type.SqlType,
                                                           n_Name,
                                                           DataModel.VariableFlags.Nullable);
                    context.VariableDeclare(n_Name, VarDeclareScope.BlockScope, Variable);
                }

                if (n_Expression != null) {
                    n_Expression.TranspileNode(context);
                    context.VariableSet(n_Name, Variable, n_Expression);
                }
            }
        }

        public      readonly    VariableTypeValue[]             n_VariableTypeValues;

        public      static      bool                            CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.DECLARE && reader.NextPeek().isToken(Core.TokenID.LocalName);
        }
        public                                                  Statement_DECLARE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DECLARE);

            var vtvlist = new List<VariableTypeValue>();

            do {
                vtvlist.Add(AddChild(new VariableTypeValue(reader)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_VariableTypeValues = vtvlist.ToArray();
            ParseStatementEnd(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_VariableTypeValues.TranspileNodes(context);
        }
    }
}
