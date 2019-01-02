using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum ParseExprContext
    {
        Normal          = 0,
        ServiceReturns  = 1
    }
    public enum ExprType
    {
        NeedsTranspile,
        Const,
        Variable,
        Complex
    }

    public delegate bool ValidatorConstValue(IExprNode expr);

    public abstract class Expr: Core.AstParseNode, IExprNode
    {
        public      delegate    bool                TestCallback(Core.TokenID id);
        public      delegate    IExprNode           ParseCallback(Core.ParserReader reader);

        public      abstract    DataModel.ValueFlags    ValueFlags          { get; }
        public      abstract    DataModel.ISqlType      SqlType             { get; }
        public      virtual     string                  CollationName       { get { return null;                } }
        public      virtual     ExprType                ExpressionType      { get { return ExprType.Complex;    } }
        public      virtual     bool                    NoBracketsNeeded    { get { return false;               } }

        public      static      bool                    CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.Number, Core.TokenID.String, Core.TokenID.BinaryValue, Core.TokenID.NULL, Core.TokenID.LocalName, Core.TokenID.Name, Core.TokenID.QuotedName, Core.TokenID.LrBracket, Core.TokenID.Plus, Core.TokenID.Minus, Core.TokenID.BitNot, Core.TokenID.NOT, Core.TokenID.CASE) ||
                   BuildIn.Catalog.ScalarFunctions.Contains(reader.CurrentToken.Text);
        }
        public      static      IExprNode               Parse(Core.ParserReader reader, ParseExprContext context)
        {
            if (context == ParseExprContext.ServiceReturns) {
                if (Expr_ResponseNode.CanParse(reader))         return new Expr_ResponseNode(reader);
                if (Expr_ServiceComplexType.CanParse(reader))   return new Expr_ServiceComplexType(reader);
            }

            return Expr_Operator_AndOr.Parse(reader, _precedence1_And, (r) => r == Core.TokenID.OR);
        }

        public      virtual     bool                    ValidateConst(DataModel.ISqlType sqlType)
        {
            if (this.isNullOrConstant()) {
                var constValue = this.ConstValue();

                if (constValue == null)
                    return true;

                if (!(constValue is Exception)) {
                    Validate.ConstValue(constValue, sqlType, this);
                    return true;
                }
            }

            return false;
        }
        public      virtual     object                  ConstValue()
        {
            return new TranspileException(this, "Can't calculate constant value.");
        }
        public      virtual     DataModel.Variable      GetVariable(Transpile.Context context)
        {
            throw new InvalidOperationException("Not a variable expression");
        }
        private     static      IExprNode               _precedence1_And(Core.ParserReader reader)
        {
            return Expr_Operator_AndOr.Parse(reader, _precedence2_NOT, (r) => r == Core.TokenID.AND);
        }
        private     static      IExprNode               _precedence2_NOT(Core.ParserReader reader)
        {
            if (Expr_Operator_NOT.CanParse(reader))     return new Expr_Operator_NOT(reader, _precedence3_Compare);

            return _precedence3_Compare(reader);
        }
        private     static      IExprNode               _precedence3_Compare(Core.ParserReader reader)
        {
            var expr = _precedence4_ValueCollate(reader);

            if (Expr_Operator_Compare.CanParse(reader))  return new Expr_Operator_Compare(reader, expr, _precedence4_ValueCollate);
            if (Expr_Operator_NULL.CanParse(reader))     return new Expr_Operator_NULL(reader, expr);
            if (Expr_Operator_BETWEEN.CanParse(reader))  return new Expr_Operator_BETWEEN(reader, expr, _precedence4_ValueCollate);
            if (Expr_Operator_LIKE.CanParse(reader))     return new Expr_Operator_LIKE(reader, expr, _precedence4_ValueCollate);
            if (Expr_Operator_IN.CanParse(reader))       return new Expr_Operator_IN(reader, expr);

            return expr;
        }
        private     static      IExprNode               _precedence4_ValueCollate(Core.ParserReader reader)
        {
            var expr = _precedence5_AddSub(reader);

            if (Expr_Operator_Collate.CanParse(reader))
                expr = new Expr_Operator_Collate(reader, expr);

            return expr;
        }
        private     static      IExprNode               _precedence5_AddSub(Core.ParserReader reader)
        {
            return Expr_Operator_Calculation.Parse(reader, _precedence6_MulDiv, (r) => (r == Core.TokenID.Plus ||
                                                                                        r == Core.TokenID.Minus ||
                                                                                        r == Core.TokenID.BitAnd ||
                                                                                        r == Core.TokenID.BitOr ||
                                                                                        r == Core.TokenID.BitXor));
        }
        private     static      IExprNode               _precedence6_MulDiv(Core.ParserReader reader)
        {
            return Expr_Operator_Calculation.Parse(reader, _precedence7_Value, (r) => (r == Core.TokenID.Star ||
                                                                                       r == Core.TokenID.Divide ||
                                                                                       r == Core.TokenID.Module));
        }
        private     static      IExprNode               _precedence7_Value(Core.ParserReader reader)
        {
            var expr = _precedence8_Primative(reader);

            while (reader.CurrentToken.isToken(Core.TokenID.Dot))
                expr = new Expr_ObjectMethodProperty(reader, expr);

            return expr;
        }
        private     static      IExprNode               _precedence8_Primative(Core.ParserReader reader)
        {
            switch (reader.CurrentToken.ID) {
            // Constant
            case Core.TokenID.Number:
            case Core.TokenID.String:
            case Core.TokenID.BinaryValue:
            case Core.TokenID.NULL:
                return new Expr_Constant(reader);

            // Variable
            case Core.TokenID.LocalName:
                return new Expr_PrimativeValue(reader);

            case Core.TokenID.LrBracket:
                if (Expr_Subquery.CanParse(reader))
                    return new Expr_Subquery(reader);

                return new Expr_SubExpr(reader);

            // Constant, OperatorUnary
            case Core.TokenID.Plus:
            case Core.TokenID.Minus:
                if (Expr_Constant.CanParse(reader))
                    return new Expr_Constant(reader);

                return new Expr_Operator_Unary(reader, _precedence8_Primative);

            // OperatorUnary
            case Core.TokenID.BitNot:
                return new Expr_Operator_Unary(reader, _precedence8_Primative);

            // CaseExpression
            case Core.TokenID.CASE:
                return new Expr_CASE(reader);

            // Columnname, FunctionCall
            case Core.TokenID.Name:
            case Core.TokenID.QuotedName:
                if (reader.NextPeek().ID == Core.TokenID.LrBracket) {
                    if (BuildIn.Catalog.ScalarFunctions.TryGetValue(reader.CurrentToken.Text, out Internal.BuildinFunctionDeclaration bfd))
                        return (Expr)bfd.Parse(reader);

                    return new Expr_PrimativeValue(reader);
                }

                if (Expr_TypeStatic.CanParse(reader)) return new Expr_TypeStatic(reader);

                return new Expr_PrimativeValue(reader);

            default:
                {
                    if (BuildIn.Catalog.ScalarFunctions.TryGetValue(reader.CurrentToken.Text, out Internal.BuildinFunctionDeclaration bfd))
                        return (Expr)bfd.Parse(reader);
                }

                throw new ParseException(reader.CurrentToken, reader.CurrentToken.ToString() + " unexpected.");
            }
        }

        public      virtual     void                    EmitSimple(Core.EmitWriter emitWriter)
        {
            Emit(emitWriter);
        }
    }

    public abstract class ExprBoolean: Expr
    {
        public      override    DataModel.ValueFlags    ValueFlags          { get { return DataModel.ValueFlags.Computed | DataModel.ValueFlags.BooleanExpression; } }
        public      override    DataModel.ISqlType      SqlType             { get { throw new TranspileException(this, "Boolean expression"); } }
    }

    public abstract class ExprCalculation: Expr
    {
    }

    public abstract class ExprBooleanBuildIn: ExprBoolean
    {
        public  readonly        Core.TokenWithSymbol    Name;

        public  override        bool                    NoBracketsNeeded    { get { return true;   } }

        protected                                       ExprBooleanBuildIn(DataModel.ISymbol symbol, Core.ParserReader reader)
        {
            Name = (Core.TokenWithSymbol)ParseToken(reader);
            Name.SetSymbol(symbol);
        }
    }

    public abstract class ExprCalculationBuildIn: ExprCalculation
    {
        public  readonly        Core.TokenWithSymbol    Name;

        public  override        bool                    NoBracketsNeeded    { get { return true;   } }

        protected                                       ExprCalculationBuildIn(DataModel.ISymbol symbol, Core.ParserReader reader)
        {
            Name = (Core.TokenWithSymbol)ParseToken(reader);
            Name.SetSymbol(symbol);
        }
    }
}
