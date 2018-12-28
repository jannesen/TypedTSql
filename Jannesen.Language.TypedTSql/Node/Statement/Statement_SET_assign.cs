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
        public      readonly    Token.TokenLocalName                n_VariableName;
        public      readonly    IExprNode                           n_Expression;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.SET && reader.NextPeek().ID == Core.TokenID.LocalName;
        }
        public                                                      Statement_SET_assign(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.SET);
            n_VariableName = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
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
