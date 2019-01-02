using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class IS_EQUAL: ExprBooleanBuildIn
    {
        public      readonly    IExprNode                   n_Expr1;
        public      readonly    IExprNode                   n_Expr2;
        public      readonly    Core.Token                  n_Collate;

        internal                                            IS_EQUAL(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Expr1 = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Expr2 = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null)
                n_Collate = ParseToken(reader, Core.TokenID.Name);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Expr1.TranspileNode(context);
                n_Expr2.TranspileNode(context);

                TypeHelpers.OperationCompare(context, null, n_Expr1, n_Expr2);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }

        public      override    void                        Emit(Core.EmitWriter emitWriter)
        {
            EmitCustom(emitWriter, _customEmit);
        }
        private                 void                        _customEmit(Core.EmitWriter emitWriter)
        {
            var trimWriter = new Core.EmitWriterTrim(emitWriter, true);

            emitWriter.WriteText("(");
                n_Expr1.Emit(trimWriter);   trimWriter.WriteText("=");  n_Expr2.Emit(trimWriter);

                if (n_Collate != null) {
                    emitWriter.WriteText(" COLLATE ");
                    n_Collate.Emit(emitWriter);
                }

                if (n_Expr1.isNullable() && n_Expr2.isNullable()) {
                    trimWriter.WriteText(" OR ("); n_Expr1.Emit(trimWriter); emitWriter.WriteText(" IS NULL AND "); n_Expr2.Emit(trimWriter); emitWriter.WriteText(" IS NULL)");
                }

            emitWriter.WriteText(")");
        }
    }
}
