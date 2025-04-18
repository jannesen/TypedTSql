using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    [StatementParser(Core.TokenID.FOR, prio:3)]
    public class Statement_FOR_SELECT: Statement, ILoopStatement
    {
        public      readonly    bool                                m_Static;
        public      readonly    Query_Select                        n_Select;
        public      readonly    Node_QueryOptions                   n_QueryOptions;
        public      readonly    Node_IntoVariables                  n_IntoVariables;
        public      readonly    Statement                           n_Statement;

        private                 string                              _forName;
        private                 DataModel.VariableList              _variableList;
        private                 bool                                _udtToNative;
        private                 bool                                _breakused;
                         
        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return (reader.CurrentToken.isToken(Core.TokenID.FOR) && reader.NextPeek().isToken(Core.TokenID.STATIC, Core.TokenID.SELECT));
        }
        public                                                      Statement_FOR_SELECT(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.FOR);
            m_Static = ParseOptionalToken(reader, Core.TokenID.STATIC) != null;
            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.StatementDeclareCursor));

            if (reader.CurrentToken.isToken(Core.TokenID.OPTION))
                n_QueryOptions = AddChild(new Node_QueryOptions(reader));

            ParseToken(reader, Core.TokenID.INTO);
            n_IntoVariables = AddChild(new Node_IntoVariables(reader));

            n_Statement = AddChild(parseContext.StatementParse(reader));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _variableList = null;
            _breakused    = false;

            var contextBlock     = new Transpile.ContextBlock(context);
            var contextStatement = new Transpile.ContextStatementQuery(contextBlock);

            if (n_QueryOptions != null) {
                n_QueryOptions.TranspileNode(contextStatement);
                contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
            }

            n_Select.TranspileNode(contextStatement);

            var selectColumn = n_Select.Resultset;
            if (selectColumn != null) {
                if (n_IntoVariables.n_VariableNames.Length < selectColumn.Count) {
                    contextBlock.AddError(this, "Missing variable, expect " + selectColumn.Count + ".");
                    return;
                }
                else if (n_IntoVariables.n_VariableNames.Length > selectColumn.Count) {
                    contextBlock.AddError(this, "To many variable, expect " + selectColumn.Count + ".");
                    return;
                }
            }

            for (int i = 0 ; i < n_IntoVariables.n_VariableNames.Length ; ++i)
                n_IntoVariables.n_VariableNames[i].TranspileAssign(contextBlock, selectColumn[i]);

            n_Statement.TranspileNode(contextStatement);

            _forName      = "__FOR" + (++contextBlock.RootContext.ForNr).ToString(System.Globalization.CultureInfo.InvariantCulture) + "_";
            _variableList = contextBlock.VariableList;
            _udtToNative  = context.DeclarationEntity.NeedUDTToNative;
        }
        public      override    void                                Emit(EmitWriter emitWriter)
        {
            var namecursor = _forName + "CURSOR";

            foreach(var c in Children) {
                if (!c.isWhitespaceOrComment) {
                    break;
                }
                c.Emit(emitWriter);
            }

            int indent = emitWriter.Linepos;
            emitWriter.WriteText("BEGIN");
            Node_AssignVariable.EmitVarVariable(emitWriter, _variableList, _udtToNative, indent + 4);
            emitWriter.WriteNewLine(indent + 4, "DECLARE ", namecursor, " CURSOR LOCAL", (m_Static ? " STATIC FORWARD_ONLY READ_ONLY" : " FAST_FORWARD"), "  FOR");
            emitWriter.WriteNewLine(indent + 8);  n_Select.Emit(emitWriter); n_QueryOptions?.Emit(emitWriter);
            emitWriter.WriteNewLine(indent + 4, "OPEN ", namecursor);
            emitWriter.WriteNewLine(1, _forName, "NEXT:");
            emitWriter.WriteNewLine(indent + 4, "FETCH ", namecursor, " INTO "); n_IntoVariables.Emit(emitWriter);
            emitWriter.WriteNewLine(indent + 4, "IF @@FETCH_STATUS=0");
            emitWriter.WriteNewLine(indent + 4, "BEGIN");
            emitWriter.WriteNewLine(-1);

            if (n_Statement is Statement_BEGIN_END beginend) {
                beginend.EmitWithoutBeginEnd(emitWriter, indent + 8);
            }
            else { 
                n_Statement.Emit(emitWriter);
            }

            emitWriter.WriteNewLine(indent + 8, "GOTO ", _forName, "NEXT");
            emitWriter.WriteNewLine(indent + 4, "END");

            if (_breakused) {
                emitWriter.WriteNewLine(1, _forName, "BREAK:");
            }

            emitWriter.WriteNewLine(indent + 4, "DEALLOCATE ", namecursor);
            emitWriter.WriteNewLine(indent, "END");
            emitWriter.WriteNewLine(-1);
        }

        public                  void                                UseGotoLabel(Core.Token token)
        {
            _breakused = token.isToken(Core.TokenID.BREAK);
        }
        public                  string                              GetGotoLabel(Core.Token token)
        {
            return _forName + (token.isToken(Core.TokenID.CONTINUE) ? "NEXT" : "BREAK");
        }
    }
}
