using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-US/library/ms181271.aspx
    //https://msdn.microsoft.com/en-us/library/ms174366.aspx
    [StatementParser(Core.TokenID.BREAK)]
    [StatementParser(Core.TokenID.CONTINUE)]
    public class Statement_BREAK_CONTINUE: Statement
    {
        public      readonly    Core.Token                          n_Cmd;
        public                  ILoopStatement                      n_LoopStatement     { get; private set; }

        public                                                      Statement_BREAK_CONTINUE(Core.ParserReader reader, IParseContext parseContext)
        {
            n_Cmd = ParseToken(reader, Core.TokenID.BREAK, Core.TokenID.CONTINUE);
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_LoopStatement = _findLoop();
            if (n_LoopStatement != null) {
                n_LoopStatement.UseGotoLabel(n_Cmd);
            } else {
                context.AddError(n_Cmd, "Only allowed in loop statement.");
            }
        }
        public      override    void                                Emit(EmitWriter emitWriter)
        {
            foreach (var c in Children) {
                if (object.ReferenceEquals(c, n_Cmd)) {
                    var label = n_LoopStatement.GetGotoLabel(n_Cmd);
                    if (label != null) {
                        emitWriter.WriteText("GOTO ", label);
                        continue;
                    }
                }

                c.Emit(emitWriter);
            }
        }

        private                 ILoopStatement                      _findLoop()
        {
            for (var p = ParentNode ; p != null ; p = p.ParentNode) {
                if (p is ILoopStatement s) {
                    return s;
                }
            }

            return null;
        }
    }
}
