using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnResponse: Query_Select_Column
    {
        public      readonly        Query_SelectContext     n_SelectContext;
        public      readonly        TokenWithSymbol         n_FieldName;
        public      readonly        IExprNode               n_Expression;
        public      readonly        Node_AS                 n_As;

        public                      DataModel.Column        ResultColumn        { get; private set; }

        public                                              Query_Select_ColumnResponse(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            n_SelectContext = selectContext;

            Core.Token[]        peek = reader.Peek(2);

            if (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Equal) {
                n_FieldName = ParseName(reader);
                ParseToken(reader, Core.TokenID.Equal);
            }

            if (selectContext == Query_SelectContext.ExpressionResponseObject ||
                selectContext == Query_SelectContext.ExpressionResponseValue)
            {
                n_Expression = ParseExpression(reader, ParseExprContext.ServiceReturns);

                if (reader.CurrentToken.isToken(Core.TokenID.AS))
                    n_As = AddChild(new Node_AS(reader));
            }
            else
                n_Expression = ParseExpression(reader);
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            ResultColumn = null;

            switch(n_SelectContext) {
            case Query_SelectContext.ExpressionResponseObject:
                if (n_FieldName == null)
                    context.AddError(this, "Field name missing");
                break;

            case Query_SelectContext.ExpressionResponseValue:
                if (n_FieldName != null)
                    context.AddError(n_FieldName, "Field name not possible.");
                break;
            }

            n_Expression.TranspileNode(context);
            n_As?.TranspileNode(context);

            if (n_FieldName != null) {
                ResultColumn = new DataModel.ColumnExpr(n_FieldName,
                                                        n_Expression,
                                                        declaration: n_FieldName);
                n_FieldName.SetSymbolUsage(ResultColumn, DataModel.SymbolUsageFlags.Write);
            }
            else {
                ResultColumn = new DataModel.ColumnExpr(n_Expression);
            }

        }
        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (ResultColumn != null) { 
                columns.Add(ResultColumn);
            }
        }

        public      override        void                    Emit(EmitWriter emitWriter)
        {
            int     i = 0;

            // Emit pre spaces and comment.
            while (Children[i].isWhitespaceOrComment)
                Children[i++].Emit(emitWriter);

            if (n_SelectContext == Query_SelectContext.ExpressionResponseValue) {
                // Skip everthing until expression
                while (Children[i] != n_Expression)
                    ++i;

                if (n_Expression is Expr_ServiceComplexType complexExpr) {
                    complexExpr.EmitResponseNode(emitWriter, "object");
                }
                else {
                    emitWriter.WriteText(" [value] =");
                    n_Expression.Emit(emitWriter);
                }
                ++i;
            }
            else
            if (n_Expression is IExprResponseNode exprResponseNode) {
                // Skip ever thing until expression
                while (Children[i] != n_Expression)
                    ++i;

                exprResponseNode.EmitResponseNode(emitWriter, n_FieldName.ValueString);
                ++i;
            }
            else {
                while (Children[i] != n_Expression)
                    Children[i++].Emit(emitWriter);

                if (n_Expression.SqlType is DataModel.EntityTypeExternal) {
                    emitWriter.WriteText(" CONVERT(VARBINARY(MAX),");
                    n_Expression.Emit(emitWriter);
                    emitWriter.WriteText(")");
                }
                else
                    n_Expression.Emit(emitWriter);

                ++i;
            }

            // Emit rest
            {
                while (i < Children.Count) {
                    if (Children[i] is Core.Token token && (token.isWhitespaceOrComment || token.ID == Core.TokenID.Semicolon))
                        token.Emit(emitWriter);

                    ++i;
                }
            }
        }
    }
}
