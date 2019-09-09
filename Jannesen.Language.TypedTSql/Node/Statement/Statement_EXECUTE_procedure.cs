using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms188332.aspx
    //      { EXEC | EXECUTE }
    //          [ @return_status = ]
    //          Objectname
    //          [ [ @parameter = ] { value
    //                             | @variable [ OUTPUT ]
    //                             | [ DEFAULT ]
    //                             }
    //          ] [ ,...n ]
    //      { EXEC | EXECUTE }
    //      ( { @string_variable | [ N ]'tsql_string' } [ + ...n ] )
    //      [ AS { LOGIN | USER } = ' name ' ]
    [StatementParser(Core.TokenID.EXEC,    prio:2)]
    [StatementParser(Core.TokenID.EXECUTE, prio:2)]
   public class Statement_EXECUTE_procedure: Statement
    {
        public      readonly    Node_EntityNameReference        n_ProcedureReference;
        public      readonly    ISetVariable                    n_ProcedureReturn;
        public      readonly    Node_EXEC_Parameter[]           n_Parameters;

        public      static      bool                            CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.EXEC, Core.TokenID.EXECUTE)) {
                Core.Token[]        peek = reader.Peek(3);

                if (peek[1].isToken("VAR", "LET") && peek[2].isToken(Core.TokenID.LocalName)) {
                    return true;
                }

                if (peek[1].isToken(Core.TokenID.LocalName, Core.TokenID.Name, Core.TokenID.QuotedName)) {
                    return true;
                }
            }

            return false;
        }
        public                                                  Statement_EXECUTE_procedure(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.EXEC, Core.TokenID.EXECUTE);

            if (reader.CurrentToken.isToken(Core.TokenID.LocalName) ||
                (reader.CurrentToken.isToken("VAR", "LET") && reader.NextPeek().isToken(Core.TokenID.LocalName))) {
                n_ProcedureReturn = ParseVarVariable(reader);
                ParseToken(reader, Core.TokenID.Equal);
            }

            n_ProcedureReference = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.StoredProcedure));

            if (Node_EXEC_Parameter.CanParse(reader)) {
                var     parameters = new List<Node_EXEC_Parameter>();

                do {
                    parameters.Add(AddChild(new Node_EXEC_Parameter(reader, false)));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                n_Parameters = parameters.ToArray();
            }

            ParseStatementEnd(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_ProcedureReference.TranspileNode(context);
            n_Parameters?.TranspileNodes(context);

            if (n_ProcedureReturn != null) {
                context.VariableSetInt(n_ProcedureReturn);
            }

            if (n_ProcedureReference.Entity != null) {
                switch(n_ProcedureReference.Entity.Type) {
                case DataModel.SymbolType.StoredProcedure:
                case DataModel.SymbolType.StoredProcedure_clr:
                case DataModel.SymbolType.ServiceMethod:
                    _validateCallParameters(context, (DataModel.EntityObjectCode)n_ProcedureReference.Entity);
                    break;

                case DataModel.SymbolType.StoredProcedure_extended:
                    break;

                default:
                    context.AddError(n_ProcedureReference, "Not a stored-procedure.");
                    break;
                }
            }
        }

        private                 void                            _validateCallParameters(Transpile.Context context, DataModel.EntityObjectCode calling_procedure)
        {
            DataModel.ParameterList     calling_parameters = calling_procedure.Parameters;

            if (calling_parameters != null && calling_parameters.Count > 0) {
                bool[]      calling_parameter_used = new bool[calling_parameters.Count];

                if (n_Parameters != null) {
                    int argn = 0;

                    for (; argn < n_Parameters.Length && n_Parameters[argn].n_Name == null ; ++argn) {
                        if (argn >= calling_parameter_used.Length)
                            throw new TranspileException(this, "Tomany arguments for procedure call.");

                        calling_parameter_used[argn] = true;
                        n_Parameters[argn].TranspileParameter(context, calling_procedure.Parameters[argn]);
                    }

                    for (; argn < n_Parameters.Length ; ++argn) {
                        var n_callParameter     = n_Parameters[argn];
                        var n_callParameterName = n_callParameter.n_Name;

                        if (n_callParameterName == null)
                            throw new TranspileException(n_callParameter, "Unamed parameter after named parameter.");

                        var n_callParameterNameText = n_callParameter.n_Name.Text;

                        int calling_parameter_index = calling_parameters.IndexOf(n_callParameterNameText);
                        if (calling_parameter_index < 0)
                            throw new TranspileException(n_callParameter, "Unknown parameter '" + n_callParameterNameText + "'.");

                        if (calling_parameter_used[calling_parameter_index])
                            throw new TranspileException(n_callParameter, "Parameter '" + n_callParameterNameText + "' already used.");

                        calling_parameter_used[calling_parameter_index] = true;

                        var calling_parameter = calling_parameters[calling_parameter_index];

                        n_callParameter.TranspileParameter(context, calling_parameter);
                    }
                }

                for (int i = 0 ; i < calling_parameter_used.Length ; ++i) {
                    if (!(calling_parameter_used[i] || calling_parameters[i].hasDefaultValue))
                        throw new TranspileException(this, "Missing parameter '" + calling_parameters[i].Name + "'.");
                }
            }
            else {
                if (n_Parameters != null && n_Parameters.Length > 0)
                    throw new TranspileException(this, "The procedure has no parameters.");
            }
        }
    }
}
