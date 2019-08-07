using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnAssign: Query_Select_Column
    {
        public      readonly        Token.TokenLocalName    n_VariableName;
        public      readonly        IExprNode               n_Expression;

        public      static          bool                    CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.LocalName) && reader.NextPeek().isToken(Core.TokenID.Equal);
        }

        public                                              Query_Select_ColumnAssign(Core.ParserReader reader)
        {
            n_VariableName = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
            ParseToken(reader, Core.TokenID.Equal);
            n_Expression = ParseExpression(reader);
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
        }
        public      override        void                    TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
            context.VariableSet(n_VariableName, n_Expression);
        }
    }
}
