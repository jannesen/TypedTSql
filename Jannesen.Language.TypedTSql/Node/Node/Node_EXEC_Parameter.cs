using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_EXEC_Parameter: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol    n_Name;
        public      readonly    IExprNode               n_Expression;
        public      readonly    bool                    n_Output;
        public      readonly    bool                    n_Default;

        public      static      bool                    CanParse(Core.ParserReader reader)
        {
             return reader.CurrentToken.isToken(Core.TokenID.LocalName, Core.TokenID.DEFAULT) || Expr.CanParse(reader);
        }

        public                                          Node_EXEC_Parameter(Core.ParserReader reader, bool nameMandatory)
        {
            if (nameMandatory || (reader.CurrentToken.isToken(Core.TokenID.LocalName) && reader.NextPeek().isToken(Core.TokenID.Equal))) {
                n_Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
                ParseToken(reader, Core.TokenID.Equal);
            }

            if (ParseOptionalToken(reader, Core.TokenID.DEFAULT) != null) {
                n_Default = true;
            }
            else {
                n_Expression = ParseSimpleExpression(reader);

                if (ParseOptionalToken(reader, "OUTPUT") != null)
                    n_Output = true;
            }
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            n_Expression?.TranspileNode(context);

            if (n_Expression != null && n_Expression.isValid()) {
                try {
                    if (n_Output) {
                        var variable = n_Expression.GetVariable(context);
                        if (variable != null) {
                            if (!variable.isReadonly)
                                variable.setAssigned();
                            else
                                context.AddError(n_Expression, "Not allowed to assign a readonly variable.");
                        }
                    }
                }
                catch(Exception err) {
                    context.AddError(n_Expression, err);
                }

            }

            if (n_Name != null) {
                n_Name.SetSymbol(new DataModel.Parameter(n_Name.Text, n_Expression.SqlType, n_Name, n_Expression.isNullable() ? DataModel.VariableFlags.Nullable : DataModel.VariableFlags.None, null));
            }
        }
    }
}
