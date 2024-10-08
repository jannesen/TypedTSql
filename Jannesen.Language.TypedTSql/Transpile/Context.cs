﻿using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public abstract class Context
    {
        public      abstract    TranspileContext                    TranspileContext        { get; }
        public      abstract    Context                             Parent                  { get; }
        public      abstract    ContextRoot                         RootContext             { get; }
        public      abstract    ContextBlock                        BlockContext            { get; }
        public      abstract    SourceFile                          SourceFile              { get; }
        public      abstract    Node.Node_ParseOptions              Options                 { get; }
        public      abstract    bool                                ReportNeedTranspile     { get; }
        public      abstract    Node.DeclarationEntity              DeclarationEntity       { get; }
        public      virtual     Node.IDataTarget                    Target                  { get { return null;                                                                                    } }
        public      virtual     DataModel.RowSetList                RowSets                 { get { return null;                                                                                    } }
        public      virtual     DataModel.QueryOptions              QueryOptions            { get { return DataModel.QueryOptions.NONE;                                                             } }
        public      virtual     DataModel.ISqlType                  ScopeIndentityType      { get { return null;                                                                                    }
                                                                                              set { throw new InvalidOperationException("ScopeIndentityType not available.");                       } }

        public                  Transpiler                          Transpiler              => TranspileContext.Transpiler;
        public                  GlobalCatalog                       Catalog                 => TranspileContext.Catalog;

        public                  Node.DeclarationObjectCode          GetDeclarationObjectCode()
        {
            if (DeclarationEntity is Node.DeclarationObjectCode rtn) return rtn;

            throw new InvalidOperationException("Excpect type DeclarationObjectCode.");
        }
        public                  T                                   GetDeclarationObject<T>() where T: Node.DeclarationEntity
        {
            if (DeclarationEntity is T rtn) return rtn;

            throw new InvalidOperationException("Excpect type " + typeof(T).Name);
        }

        public      virtual     void                                SetQueryOptions(DataModel.QueryOptions options)
        {
            throw new InvalidOperationException("QueryOptions not available.");
        }

        public                  bool                                VariableDeclare(Core.TokenWithSymbol name, Node.VarDeclareScope scope, DataModel.VariableLocal variable)
        {
            var    blockContext = BlockContext ?? throw new InvalidOperationException("VariableDeclare without BlockContext.");

            if (scope == Node.VarDeclareScope.CodeScope) {
                for (;;) {
                    var p = blockContext.Parent?.BlockContext;
                    if (p is null) {
                        break;
                    }
                    blockContext = p;
                }
            }

            if (_variableTryGetParent(blockContext, name.Text.ToLowerInvariant(), out var dummy)) {
                AddError(name, "Variable " + name.Text + " already declared in parent block.");
                return false;
            }

            if (blockContext.VariableList == null)
                blockContext.VariableList = new DataModel.VariableList();

            if (!blockContext.VariableList.TryAdd(variable)) {
                AddError(name, "Variable " + name.Text + " already declared.");
                return false;
            }

            if (blockContext.BlockId > 0) {
                variable.setSqlName("@#" + blockContext.BlockId + "$" + variable.Name.Substring(1));
            }

            return true;
        }
        public                  DataModel.Variable                  VariableGet(Core.TokenWithSymbol name, bool allowGlobal=false)
        {
            var variable = VariableGet(name, name.Text, allowGlobal);

            if (variable != null) {
                if (!(variable is DataModel.VariableGlobal)) {
                    CaseWarning(name, variable.Name);
                }
            }

            return variable;
        }
        public                  DataModel.Variable                  VariableGet(Core.IAstNode node, string name, bool allowGlobal=false)
        {
            DataModel.Variable variable;
            string nameLower = name.ToLowerInvariant();

            if (nameLower[1] == '@') {
                if (!allowGlobal) {
                    AddError(node, "Global variable not allowed.");
                    return null;
                }

                if (BuildIn.Catalog.GlobalVariable.TryGetValue(nameLower, out variable))
                    return variable;
            }
            else {
                ContextBlock    blockContext = BlockContext;

                if ((blockContext?.VariableList != null && blockContext.VariableList.TryGetValue(nameLower, out variable)) ||
                    _variableTryGetParent(blockContext, nameLower, out variable))
                    return variable;
            }

            AddError(node, "Unknown variable '" + name + "'.");
            return null;
        }
        public                  DataModel.VariableLocal             VarVariableSet(Token.TokenLocalName name, Node.VarDeclareScope scope, DataModel.ISqlType sqlType)
        {
            var variable = new DataModel.VariableLocal(name.Text,
                                                       sqlType,
                                                       name,
                                                       DataModel.VariableFlags.Nullable | DataModel.VariableFlags.VarDeclare);
            VariableDeclare(name, scope, variable);
            variable.setAssigned();
            return variable;
        }
        public                  void                                VariableSet(Core.IAstNode name, DataModel.Variable variable, DataModel.IExprResult expr)
        {
            if (variable != null) {
                try {
                    if (!variable.isReadonly)
                        variable.setAssigned();
                    else
                        AddError(name, "Not allowed to assign a readonly variable.");

                    if (!variable.isNullable && expr.ValueFlags.isNullable())
                        AddError(name, "Not allowed to assign null to a non-nullable variable.");

                    Validate.Assign(this, name, variable, expr);
                }
                catch(Exception err) {
                    AddError(name, err);
                }
            }
        }

        public      virtual     DataModel.RowSet                    FindRowSet(string name)
        {
            return null;
        }
        public      virtual     DataModel.Column                    FindColumn(string name, out bool ambiguous)
        {
            ambiguous = false;
            return null;
        }

        public      abstract    void                                AddError(Core.IAstNode node, Exception err);
        public      abstract    void                                AddError(Core.IAstNode node, string error, QuickFix quickFix=null);
        public      abstract    void                                AddWarning(Core.IAstNode node, string warning, QuickFix quickFix=null);

        public      virtual     void                                SetTarget(Node.IDataTarget target)
        {
            throw new InvalidOperationException("SetTarget not available.");
        }


        public                  void                                CaseWarning(Core.Token token, string value)
        {
            if (token.ValueString != value) {
                string      valueString;

                if (token is Token.QuotedName)      valueString = Library.SqlStatic.QuoteName(value);
                else if (token is Token.String)     valueString = token.Text[0] == 'N' ? Library.SqlStatic.QuoteNString(value) : Library.SqlStatic.QuoteString(value);
                else                                valueString = value;

                AddWarning(token, "Case mismatch, expect " + valueString, new QuickFix(new DataModel.DocumentSpan(SourceFile.Filename, token), token.Text, valueString));
            }
        }

        public                  bool                                ValidateInteger(Core.Token token, int minValue, int maxValue)
        {
            if (token != null) {
                if (!token.isInteger()) {
                    AddError(token, "Value is not a integer.");
                    return false;
                }

                var value = token.ValueInt;

                if (minValue > value || value > maxValue) {
                    AddError(token, "Value out of range must be between " + minValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + " and " + maxValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
                    return false;
                }
            }

            return true;
        }
        public                  bool                                ValidateInteger(Node.IExprNode expr, int minValue, int maxValue)
        {
            if (expr != null) {
                int value;

                try {
                    object ovalue = expr.ConstValue();

                    if (ovalue is Exception)
                        throw (Exception)ovalue;

                    value = (int)ovalue;
                }
                catch(Exception err) {
                    AddError(expr, err);
                    return false;
                }

                if (minValue > value || value > maxValue) {
                    AddError(expr, "Value out of range must be between " + minValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + " and " + maxValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
                    return false;
                }
            }

            return true;
        }
        public                  bool                                ValidateNumber(Core.Token token, double minValue, double maxValue)
        {
            if (token != null) {
                if (!(token is Token.Number)) {
                    AddError(token, "Is not a number.");
                    return false;
                }

                var value = token.ValueFloat;

                if (minValue > value || value > maxValue) {
                    AddError(token, "Value out of range must be between " + minValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + " and " + maxValue.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
                    return false;
                }
            }

            return true;
        }

        public                  DataModel.DocumentSpan              CreateDocumentSpan(Core.IAstNode node)
        {
            return new DataModel.DocumentSpan(SourceFile.Filename, node);
        }

        private                 bool                                _variableTryGetParent(ContextBlock blockContext, string nameLower, out DataModel.Variable variable)
        {
            if (blockContext != null) {
                blockContext = blockContext.Parent.BlockContext;

                while (blockContext != null) {
                    if (blockContext.VariableList != null && blockContext.VariableList.TryGetValue(nameLower, out variable))
                        return true;

                    blockContext = blockContext.Parent.BlockContext;
                }
            }

            if (DeclarationEntity is Node.DeclarationObjectCode declarationObjectCode) {
                if (declarationObjectCode.n_Parameters != null) {
                    if (declarationObjectCode.n_Parameters.t_Parameters.TryGetValue(nameLower, out var paramter)) {
                        variable = paramter;
                        return true;
                    }
                }

                if (declarationObjectCode.EntityType == DataModel.SymbolType.FunctionMultistatementTable &&
                    declarationObjectCode is Node.Declaration_FUNCTION declarationFunction)
                {
                    var returnVariable = declarationFunction.ReturnVariable;
                    if (declarationFunction != null) {
                        if (returnVariable.Name.ToLowerInvariant() == nameLower) {
                            variable = returnVariable;
                            return true;
                        }
                    }
                }
            }

            variable = null;
            return false;
        }
    }
}
