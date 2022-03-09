using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnVariableAssign: Query_Select_Column
    {
        public      readonly        Node_AssignVariable     n_Variable;
        public      readonly        Core.Token              n_Assign;
        public      readonly        IExprNode               n_Expression;

        public      override        bool                    isVariableAssign            => true;

        public      static          bool                    CanParse(Core.ParserReader reader)
        {
            var readahead = reader.Peek(3);
            var off = readahead[0].isToken("VAR", "LET") ? 1 : 0;
            return readahead[off + 0].isToken(Core.TokenID.LocalName) && readahead[off + 1].isToken(Core.TokenID.Equal);
        }

        public                                              Query_Select_ColumnVariableAssign(Core.ParserReader reader)
        {
            n_Variable = ParseVarVariable(reader);
            n_Assign = ParseToken(reader, Core.TokenID.Equal, Core.TokenID.PlusAssign, Core.TokenID.MinusAssign, Core.TokenID.MultAssign, Core.TokenID.DivAssign, Core.TokenID.ModAssign, Core.TokenID.AndAssign, Core.TokenID.XorAssign, Core.TokenID.OrAssign);
            n_Expression = ParseExpression(reader);
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
            n_Variable.TranspileAssign(context, n_Expression, !n_Assign.isToken(Core.TokenID.Equal));
        }
        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
        }
    }
}
