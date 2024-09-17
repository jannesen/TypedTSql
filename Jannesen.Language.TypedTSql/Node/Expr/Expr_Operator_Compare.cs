using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms177682.aspx
    // https://msdn.microsoft.com/en-us/library/ms179859.aspx
    // Expression_OperatorCompare
    //      : Expression_OperatorAddSub ('<' | '<=' | '=' | '<>' | '>=' | '>' | IS [NOT] DISTINCT FROM) Expression_OperatorAddSub
    public class Expr_Operator_Compare: ExprBoolean
    {
        public      readonly    IExprNode                       n_Expr1;
        public      readonly    CompareOperator                 n_Operator;
        public      readonly    IExprNode                       n_Expr2;

        private                 Core.IAstNode                   _translateToken;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            switch(reader.CurrentToken.ID) {
            case Core.TokenID.Equal:
            case Core.TokenID.NotEqual:
            case Core.TokenID.Less:
            case Core.TokenID.Greater:
            case Core.TokenID.LessEqual:
            case Core.TokenID.GreaterEqual:
                return true;

            case Core.TokenID.DistinctEqual:
            case Core.TokenID.DistinctNotEqual:
                return true;

            case Core.TokenID.IS: {
                var peek = reader.Peek(4);
                return (peek[1].isToken("DISTINCT") && peek[2].isToken("FROM"))
                    || (peek[1].isToken("NOT")      && peek[2].isToken("DISTINCT") && peek[3].isToken("FROM"));
                }

            default:
                return false;
            }
        }
        public                                                  Expr_Operator_Compare(Core.ParserReader reader, IExprNode expr1, ParseCallback parser)
        {
            n_Expr1    = AddChild(expr1);

            switch(reader.CurrentToken.ID) {
            case Core.TokenID.Equal:
            case Core.TokenID.NotEqual:
            case Core.TokenID.Less:
            case Core.TokenID.Greater:
            case Core.TokenID.LessEqual:
            case Core.TokenID.GreaterEqual: {
                    switch(ParseToken(reader).ID) {
                    case Core.TokenID.Equal:            n_Operator = CompareOperator.Equal;             break;
                    case Core.TokenID.NotEqual:         n_Operator = CompareOperator.NotEqual;          break;
                    case Core.TokenID.Less:             n_Operator = CompareOperator.Less;              break;
                    case Core.TokenID.Greater:          n_Operator = CompareOperator.Greater;           break;
                    case Core.TokenID.LessEqual:        n_Operator = CompareOperator.LessEqual;         break;
                    case Core.TokenID.GreaterEqual:     n_Operator = CompareOperator.GreaterEqual;      break;
                    }
                }
                break;

            case Core.TokenID.DistinctEqual:
            case Core.TokenID.DistinctNotEqual: {
                    var token = ParseToken(reader);
                    _translateToken = token;
                    switch(token.ID) {
                    case Core.TokenID.DistinctEqual:    n_Operator = CompareOperator.DistinctEqual;             break;
                    case Core.TokenID.DistinctNotEqual: n_Operator = CompareOperator.DistinctNotEqual;          break;
                    }
                }
                break;

            case Core.TokenID.IS:
                ParseToken(reader);
                n_Operator = ParseOptionalToken(reader, "NOT") != null ? CompareOperator.DistinctEqual : CompareOperator.DistinctNotEqual;
                ParseToken(reader, "DISTINCT");
                ParseToken(reader, "FROM");
                break;
            }

            n_Expr2    = AddChild(parser(reader));
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr1.TranspileNode(context);
                n_Expr2.TranspileNode(context);
                TypeHelpers.OperationCompare(context, n_Operator, n_Expr1, n_Expr2);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }

        public  override        void                            Emit(EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (node == _translateToken) {
                    switch(n_Operator) {
                    case CompareOperator.DistinctEqual:         emitWriter.WriteText(" IS NOT DISTINCT FROM ");     break;
                    case CompareOperator.DistinctNotEqual:      emitWriter.WriteText(" IS DISTINCT FROM ");         break;
                    }
                }
                else {
                    node.Emit(emitWriter);
                }
            }
        }
    }
}
