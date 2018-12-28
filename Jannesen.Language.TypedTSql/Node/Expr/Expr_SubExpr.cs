using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_SubExpr: Expr
    {
        public class AsCast: Core.AstParseNode
        {
            public      readonly    Node_Datatype                   n_Type;

            public                                                  AsCast(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.AS);
                n_Type = AddChild(new Node_Datatype(reader));
            }

            public      override    void                            TranspileNode(Transpile.Context context)
            {
                n_Type.TranspileNode(context);
            }

            public      override    void                            Emit(EmitWriter emitWriter)
            {
                EmitCustom(emitWriter, (ew) => { });
            }
        }

        public      readonly    IExprNode                       n_Expr;
        public      readonly    AsCast                          n_Cast;

        public      override    ExprType                        ExpressionType      { get { return n_Expr.ExpressionType;                                    } }
        public      override    DataModel.ValueFlags            ValueFlags          { get { return n_Expr.ValueFlags;                                        } }
        public      override    DataModel.ISqlType              SqlType             { get { return n_Cast != null ? n_Cast.n_Type.SqlType : n_Expr.SqlType;  } }
        public      override    string                          CollationName       { get { return n_Expr.CollationName;                                     } }

        public                                                  Expr_SubExpr(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Expr = ParseExpression(reader);

            if (reader.CurrentToken.isToken(Core.TokenID.AS))
                n_Cast = AddChild(new AsCast(reader));

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    object                          ConstValue()
        {
            return n_Expr.ConstValue();
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Expr.TranspileNode(context);
            n_Cast?.TranspileNode(context);

            if (n_Cast != null) {
                var exprType = n_Expr.SqlType;
                var castType = n_Cast.n_Type.SqlType;

                if (exprType != null && !(exprType is DataModel.SqlTypeAny) &&
                    castType != null && !(castType is DataModel.SqlTypeAny))
                {
                    if (!(n_Expr.isConstant() && n_Expr.ValidateConst(castType.NativeType))) {
                        if (exprType.NativeType != castType.NativeType)
                            context.AddError(n_Cast, "Can't cast '" + exprType.ToString() + "' to '" + castType.ToString() + "'.");
                    }
                }
            }
        }

        public      override    void                            Emit(EmitWriter emitWriter)
        {
            if (n_Expr.NoBracketsNeeded)
                EmitSimple(emitWriter);
            else
                base.Emit(emitWriter);
        }
        public      override    void                            EmitSimple(Core.EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (c is Core.Token token) {
                    if (token.ID == TokenID.LrBracket ||
                        token.ID == TokenID.RrBracket)
                    {
                        emitWriter.WriteText(" ");
                        continue;
                    }
                }

                c.Emit(emitWriter);
            }
        }
    }
}
