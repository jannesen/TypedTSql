using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_EXEC_Parameter: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol    n_Name;
        public      readonly    IExprNode               n_Expression;
        public      readonly    ISetVariable            n_Var;
        public      readonly    bool                    n_Output;
        public      readonly    bool                    n_Default;

        public      static      bool                    CanParse(Core.ParserReader reader)
        {
            return (reader.CurrentToken.isToken(Core.TokenID.LocalName, Core.TokenID.DEFAULT) || Expr.CanParse(reader)) ||
                   (reader.CurrentToken.isToken("VAR") && reader.NextPeek().isToken(Core.TokenID.LocalName));
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
                if (reader.CurrentToken.isToken("VAR")) {
                    n_Var = ParseSetVariable(reader);
                }
                else { 
                    n_Expression = ParseSimpleExpression(reader);
                }

                if (ParseOptionalToken(reader, "OUTPUT") != null)
                    n_Output = true;
            }
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            n_Expression?.TranspileNode(context);
        }
        public                  void                    TranspileParameter(Transpile.Context context, DataModel.Variable callingParameter)
        {
            try {
                if (n_Var != null) {
                    if (!n_Output) {
                        context.AddError(this, "var without output.");
                    }

                    if (callingParameter != null) {
                        context.VarVariableSet(n_Var.TokenName, callingParameter.SqlType);
                    }
                    else {
                        context.AddError(n_Name, "Can't determin type of parameter.");
                    }
                }
                else {
                    if (n_Output) {
                        var variable = context.VariableGet(n_Expression.GetVariableToken());
                        if (variable != null) {
                            if (!variable.isReadonly) { 
                                variable.setAssigned();
                            }
                            else { 
                                context.AddError(n_Expression, "Not allowed to assign a readonly variable.");
                            }
                        }
                    }
                }
                
                if (n_Expression != null && callingParameter != null) {
                    try {
                        Validate.Assign(context, this, callingParameter, this.n_Expression, output:this.n_Output);
                    }
                    catch(Exception err) {
                        context.AddError(this, err);
                    }
                }

                if (n_Name != null) {
                    if (callingParameter != null) { 
                        n_Name.SetSymbol(callingParameter);

                        if (n_Name.Text != callingParameter.Name) {
                            context.AddError(n_Name, "Case missmatch, expect '" + callingParameter.Name + "'.");
                        }
                    }
                    else {
                        TokenWithSymbol.SetNoSymbol(n_Name);
                    }
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
