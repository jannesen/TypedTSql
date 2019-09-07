using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnAssign: Query_Select_Column
    {
        public      readonly        ISetVariable            n_VariableName;
        public      readonly        IExprNode               n_Expression;

        public      static          bool                    CanParse(Core.ParserReader reader)
        {
            var readahead = reader.Peek(3);
            var off = readahead[0].isToken("VAR") ? 1 : 0;
            return readahead[off + 0].isToken(Core.TokenID.LocalName) && readahead[off + 1].isToken(Core.TokenID.Equal);
        }

        public                                              Query_Select_ColumnAssign(Core.ParserReader reader)
        {
            n_VariableName = ParseSetVariable(reader);
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
