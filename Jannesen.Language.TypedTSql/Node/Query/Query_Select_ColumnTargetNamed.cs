using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Query_Select_ColumnTargetNamed: Query_Select_Column
    {
        public      readonly        Core.TokenWithSymbol    n_ColumnName;
        public      readonly        IExprNode               n_Expression;

        public                                              Query_Select_ColumnTargetNamed(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(2);

            if (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Equal) {
                n_ColumnName = ParseName(reader);
                ParseToken(reader, Core.TokenID.Equal);
                n_Expression = ParseExpression(reader);
            }
            else {
                n_Expression = ParseExpression(reader);

                if (ParseOptionalToken(reader, Core.TokenID.AS) != null) {
                    n_ColumnName = ParseName(reader);
                }
            }
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            n_Expression.TranspileNode(context);

            if (n_ColumnName != null) {
                var targetColumn = context.Target.Columns.FindColumn(n_ColumnName.ValueString, out var ambigous);
                if (targetColumn != null) {
                    if (ambigous) {
                        context.AddError(n_ColumnName, "Target column [" + n_ColumnName.ValueString + "] is ambigous.");
                    }
                    else {
                        context.CaseWarning(n_ColumnName, targetColumn.Name);
                    }

                    n_ColumnName.SetSymbol(targetColumn);

                    try {
                        Validate.Assign(context, targetColumn, n_Expression);
                    }
                    catch(Exception err) {
                        if (!(err is TranspileException))
                            err = new ErrorException("Assignment target column [" + targetColumn.Name + "] failed.", err);

                        context.AddError(n_ColumnName, err);
                    }
                }
                else {
                    context.AddError(n_ColumnName, "Unknown target column [" + n_ColumnName.ValueString + "].");
                }
            }
            else {
                context.AddError(this, "Target-column name missing");
            }
        }

        public      override        void                    AddColumnToList(Transpile.Context context, List<DataModel.Column> columns)
        {
            if (n_ColumnName != null) {
                columns.Add(new DataModel.ColumnExpr(n_ColumnName,
                                                     n_Expression,
                                                     declaration: n_ColumnName));
            }
            else {
                columns.Add(new DataModel.ColumnExpr(n_Expression));
            }
        }
    }
}
