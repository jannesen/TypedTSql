using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class StatementBlock: Core.AstParseNode
    {
        public delegate bool EndTest(Core.ParserReader reader);

        public                  bool                            n_CodeBlock;
        public                  IReadOnlyCollection<Statement>  Statements
        {
            get {
                return _statements;
            }
        }

        private                 List<Statement>                 _statements;
        public                  DataModel.VariableList          _variableList;

        public                                                  StatementBlock(bool codeBlock)
        {
            n_CodeBlock = codeBlock;
        }
        public                  bool                            Parse(Core.ParserReader reader, IParseContext parseContext, EndTest endTest)
        {
            _statements = new List<Statement>();

            for (;;) {
                reader.ReadBlanklines(this);

                if (endTest(reader))
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
            var contextStatementBlock = n_CodeBlock ? new Transpile.ContextCodeBlock(context) : new Transpile.ContextBlock(context);

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
                        if (_variableList != null) { 
                            foreach(var v in _variableList) {
                                if (v.isVarDeclare) {
                                    emitWriter.WriteIndent(indent);
                                    emitWriter.WriteText("DECLARE ");
                                    emitWriter.WriteText(v.SqlName ?? v.Name);
                                    emitWriter.WriteText(" ");

                                    var sqlType = v.SqlType;

                                    if (sqlType is DataModel.SqlTypeTable typeTable) {
                                        int pos = emitWriter.Linepos;
                                        emitWriter.WriteText("TABLE");
                                        emitWriter.WriteNewLine(pos);
                                        emitWriter.WriteText("(");

                                        var fnext = false;
                                        foreach (var c in typeTable.Columns) {
                                            if (fnext) {
                                                emitWriter.WriteText(",");
                                            }
                                            emitWriter.WriteNewLine(pos + 4);
                                            emitWriter.WriteText(Library.SqlStatic.QuoteName(c.Name));
                                            emitWriter.WriteText(" ");
                                            emitWriter.WriteText(c.SqlType.ToSql());

                                            emitWriter.WriteText(c.isNullable ? " NULL" : " NOT NULL");
                                            fnext = true;
                                        }

                                        emitWriter.WriteNewLine(pos);
                                        emitWriter.WriteText(")");
                                    }
                                    else {
                                        emitWriter.WriteText(v.SqlType.ToSql());
                                    }

                                    emitWriter.WriteText(";");
                                    emitWriter.WriteNewLine(-1);
                                }
                            }
                        }
                        f = false;
                    }

                    node.Emit(emitWriter);
                }
            }
        }
    }
}
