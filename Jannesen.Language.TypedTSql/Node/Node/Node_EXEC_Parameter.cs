using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_EXEC_Parameter: Core.AstParseNode
    {
        public      readonly    TokenWithSymbol         n_Name;
        public      readonly    IExprNode               n_Expression;
        public      readonly    Node_AssignVariable     n_VarOutput;
        public      readonly    bool                    n_Default;

        public                  DataModel.ISqlType      SqlType
        {
            get {
                if (n_Expression != null) return n_Expression.SqlType;
                if (n_VarOutput != null)  return n_VarOutput.SqlType;
                return null;
            }
        }

        private                 DataModel.Variable      _t_callingParameter;

        public      static      bool                    CanParse(Core.ParserReader reader)
        {
            return (reader.CurrentToken.isToken(Core.TokenID.LocalName, Core.TokenID.DEFAULT) || Expr.CanParse(reader)) ||
                   (reader.CurrentToken.isToken("VAR", "LET") && reader.NextPeek().isToken(Core.TokenID.LocalName));
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
                if (reader.CurrentToken.isToken("VAR", "LET") || reader.NextPeek().isToken("OUTPUT")) {
                    n_VarOutput = ParseVarVariable(reader);
                    ParseToken(reader, "OUTPUT");
                }
                else { 
                    n_Expression = ParseSimpleExpression(reader);
                }

            }
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
        }
        public                  void                    TranspileParameter(Transpile.Context context, DataModel.Variable callingParameter)
        {
            try {
                _t_callingParameter = callingParameter;

                n_Expression?.TranspileNode(context);

                if (n_VarOutput != null) {
                    n_VarOutput.TranspileAssign(context, callingParameter?.SqlType);
                    n_VarOutput.Variable?.setUsed();
                }
                
                if (callingParameter != null) {
                    try {
                        if (n_Expression != null) {
                            Validate.Assign(context, this, callingParameter, n_Expression, output:false);
                        }
                        if (n_VarOutput != null) {
                            Validate.Assign(context, this, callingParameter, n_VarOutput, output:true);
                        }
                    }
                    catch(Exception err) {
                        context.AddError(this, err);
                    }
                }

                if (n_Name != null) {
                    if (callingParameter != null) { 
                        n_Name.SetSymbolUsage(callingParameter, DataModel.SymbolUsageFlags.Reference);

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
