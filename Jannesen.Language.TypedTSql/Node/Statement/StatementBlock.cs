using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class StatementBlock: Core.AstParseNode
    {
        public delegate bool EndTest(Core.ParserReader reader);

        public                  IReadOnlyCollection<Statement>  Statements
        {
            get {
                return _statements;
            }
        }

        private                 List<Statement>                 _statements;
        public                  DataModel.VariableList          _variableList;

        public                                                  StatementBlock()
        {
        }
        public                  bool                            Parse(Core.ParserReader reader, IParseContext parseContext, EndTest endTest)
        {
            _statements = new List<Statement>();

            for (;;) {
                reader.ReadBlanklines(this);

                if (endTest(reader) && !parseContext.StatementCanParse(reader))
                    return true;

                if (reader.CurrentToken.ID == Core.TokenID.EOF || reader.Transpiler.DeclarationParsers.CanParse(reader, null))
                    return false;

                var     savedPosition = reader.Position;

                try {
                    _statements.Add(AddChild(parseContext.StatementParse(reader)));
                }
                catch(Exception err) {
                    reader.AddError(err);

                    var errNode = new Core.AstParseErrorNode(reader, savedPosition);

                    while (!(reader.CurrentToken.ID == Core.TokenID.EOF || endTest(reader) || parseContext.StatementCanParse(reader) || reader.Transpiler.DeclarationParsers.CanParse(reader, null)))
                        reader.ReadToken(errNode);

                    AddChild(errNode);
                }
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _variableList = null;
            var contextStatementBlock = new Transpile.ContextBlock(context);

            foreach(var statement in _statements)
                statement.TranspileStatement(contextStatementBlock);

            contextStatementBlock.EndBlock();
            _variableList = contextStatementBlock.VariableList;
        }
        public                  void                            Emit(Core.EmitWriter emitWriter, int indent)
        {
            bool    f = true;

            if (this.Children != null) {
                foreach(var node in this.Children) {
                    if (f && !(node is Node.Statement_DECLARE || node is Node.Statement_SET_option)) {
                        Node_VarVariable.EmitVarVariable(emitWriter, _variableList, indent);
                        f = false;
                    }

                    node.Emit(emitWriter);
                }
            }
        }
    }
}
