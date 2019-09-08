using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms189484.aspx
    // Statement_SET_assign ::=
    //      SET @local_variable {+= | -= | *= | /= | %= | &= | ^= | |= } expression
    [StatementParser(Core.TokenID.SET, prio:2)]
    public class Statement_SET_assign: Statement
    {
        public      readonly    ISetVariable                        n_VariableName;
        public      readonly    IExprNode                           n_Expression;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            var readahead = reader.Peek(3);
            var off = readahead[1].isToken("VAR", "LET") ? 2 : 1;
            return readahead[0].isToken(Core.TokenID.SET) && readahead[off].isToken(Core.TokenID.LocalName);
        }
        public                                                      Statement_SET_assign(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.SET);
            n_VariableName = ParseVarVariable(reader);
            ParseToken(reader, Core.TokenID.Equal, Core.TokenID.PlusAssign, Core.TokenID.MinusAssign, Core.TokenID.MultAssign, Core.TokenID.DivAssign, Core.TokenID.ModAssign, Core.TokenID.AndAssign, Core.TokenID.XorAssign, Core.TokenID.OrAssign);
            n_Expression = ParseExpression(reader);

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);
            context.VariableSet(n_VariableName, n_Expression);
        }
    }
}
