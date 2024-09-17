using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnExpression: Query_Select_Column
    {
        public      readonly        Query_SelectContext     n_SelectContext;
        public      readonly        Core.TokenWithSymbol    n_ColumnName;
        public      readonly        Core.Token              n_Assign;
        public      readonly        IExprNode               n_Expression;

        public                      DataModel.Column        ResultColumn            { get; private set; }

        public                                              Query_Select_ColumnExpression(Core.ParserReader reader, Query_SelectContext selectContext)
        {
            n_SelectContext = selectContext;

            Core.Token[]        peek = reader.Peek(2);

            if (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Equal) {
                n_ColumnName = ParseName(reader);
                n_Assign = ParseToken(reader, Core.TokenID.Equal);
                n_Expression = ParseExpression(reader);
            }
            else {
                n_Expression = ParseExpression(reader);

                if ((n_Assign = ParseOptionalToken(reader, Core.TokenID.AS)) != null) {
                    n_ColumnName = ParseName(reader);
                }
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            ResultColumn = null;

            n_Expression.TranspileNode(context);

            if (n_Expression.ValueFlags.isBooleanExpression())
                throw new TranspileException(n_Expression, "Expression is a boolean expression.");

            var target = context.Target;
            if (target != null) {
                if (n_ColumnName != null) {
                    ResultColumn = target.GetColumnForAssign(n_ColumnName.ValueString,
                                                             n_Expression.SqlType,
                                                             n_Expression.CollationName,
                                                             n_Expression.ValueFlags,
                                                             n_ColumnName,
                                                             null,
                                                             out var declated);

                    if (ResultColumn == null) {
                        context.AddError(n_ColumnName, "Unknown target column [" + n_ColumnName.ValueString + "].");
                    }
                    n_ColumnName.SetSymbolUsage(ResultColumn, declated ? DataModel.SymbolUsageFlags.Write | DataModel.SymbolUsageFlags.Declaration : DataModel.SymbolUsageFlags.Write);
                }
                else if (n_Expression is Expr_ColumnUserFunction exprColumn && exprColumn.ReferencedColumn != null){
                    ResultColumn = target.GetColumnForAssign(n_Expression.ReferencedColumn.Name,
                                                             n_Expression.SqlType,
                                                             n_Expression.CollationName,
                                                             n_Expression.ValueFlags,
                                                             null,
                                                             exprColumn.ReferencedColumn.Symbol,
                                                             out var declated);

                    if (ResultColumn == null) {
                        context.AddError(n_Expression, "Unknown target column [" + n_Expression.ReferencedColumn.Name + "].");
                    }

                    exprColumn.SetColumnSymbol(new DataModel.SymbolSourceTarget(new DataModel.SymbolUsage(exprColumn.ReferencedColumn.Symbol, DataModel.SymbolUsageFlags.Read),
                                                                                new DataModel.SymbolUsage(ResultColumn.Symbol,                declated ? DataModel.SymbolUsageFlags.Write | DataModel.SymbolUsageFlags.Declaration : DataModel.SymbolUsageFlags.Write)));
                }
                else {
                    context.AddError(this, "Target-column name missing");
                }

                if (ResultColumn != null) {
                    try {
                        Logic.Validate.Assign(context, ResultColumn, n_Expression);
                    }
                    catch(Exception err) {
                        if (!(err is TranspileException))
                            err = new ErrorException("Assignment target column [" + ResultColumn.Name + "] failed.", err);

                        context.AddError(this, err);
                    }
                }
            }
            else {
                if (n_ColumnName != null) {
                    ResultColumn = new DataModel.ColumnExpr(n_ColumnName,
                                                            n_Expression,
                                                            declaration: n_ColumnName);
                    n_ColumnName.SetSymbolUsage(ResultColumn, DataModel.SymbolUsageFlags.Write);
                }
                else {
                    if ((ResultColumn = n_Expression.ReferencedColumn) == null) {
                        ResultColumn = new DataModel.ColumnExpr(n_Expression);
                    }
                }
            }
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (ResultColumn != null) {
                columns.Add(ResultColumn);
            }
        }

        public      override        void                    Emit(Core.EmitWriter emitWriter)
        {
            if (n_Assign != null) {
                if (n_SelectContext == Query_SelectContext.StatementReceive) {
                    if (n_Assign.isToken(Core.TokenID.Equal)) { 
                        var expressionEmitArray = new Core.EmitWriterArray(emitWriter.EmitContext);
                        n_Expression.Emit(expressionEmitArray);
                        var expressionEmitArrayEnd = expressionEmitArray.IndexEndWhitespace();

                        foreach(var c in Children) {
                            if (object.Equals(c, n_ColumnName)) {
                                expressionEmitArray.EmitNodes(emitWriter, 0, expressionEmitArrayEnd);
                                continue;
                            }

                            if (object.Equals(c, n_Assign)) {
                                emitWriter.WriteText(" AS ");
                                continue;
                            }

                            if (object.Equals(c, n_Expression)) {
                                n_ColumnName.Emit(emitWriter);
                                expressionEmitArray.EmitNodes(emitWriter, expressionEmitArrayEnd);
                                continue;
                            }

                            c.Emit(emitWriter);
                        }

                        return ;
                    }
                }

                if (n_SelectContext == Query_SelectContext.StatementInsertTargetNamed) { 
                    int     i = 0;

                    if (n_Assign.isToken(Core.TokenID.Equal)) {
                        foreach(var c in Children) {
                            switch(i) {
                            case 0: if (object.Equals(c, n_ColumnName))    i = 1;   break;
                            case 1: if (object.Equals(c, n_Expression))    i = 2;   break;
                            }

                            if (i != 1) {
                                c.Emit(emitWriter);
                            }
                        }
                        return ;
                    }

                    if (n_Assign.isToken(Core.TokenID.AS)) {
                        foreach(var c in Children) {
                            switch(i) {
                            case 0: if (c is Core.Token token && token.isToken(Core.TokenID.AS))   i = 1;  break;
                            case 1: if (object.Equals(c, n_ColumnName))                            i = 2;  continue;
                            }

                            if (i != 1) {
                                c.Emit(emitWriter);
                            }
                        }
                        return ;
                    }
                }
            }
            base.Emit(emitWriter);
        }
    }
}
