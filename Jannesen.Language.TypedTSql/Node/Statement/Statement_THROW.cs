using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-US/library/ee677615.aspx
    [StatementParser(Core.TokenID.THROW)]
    public class Statement_THROW: Statement
    {
        public      readonly    Core.IAstNode                       n_ErrorNumber;
        public      readonly    Core.IAstNode                       n_Message;
        public      readonly    Core.IAstNode                       n_State;

        public                                                      Statement_THROW(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.THROW);

            if (reader.CurrentToken.isToken(Core.TokenID.Number) || reader.NextPeek().isToken(Core.TokenID.Comma)) { 
                n_ErrorNumber = _parseArgument(reader, Core.TokenID.Number, Core.TokenID.LocalName);
                ParseToken(reader, Core.TokenID.Comma);
                n_Message = _parseArgument(reader, Core.TokenID.String, Core.TokenID.LocalName);
                ParseToken(reader, Core.TokenID.Comma);
                n_State = _parseArgument(reader, Core.TokenID.Number, Core.TokenID.LocalName);
            }
            else {
                AddLeading(reader);
                n_Message = _parseArgument(reader, Core.TokenID.String, Core.TokenID.LocalName);
            }

            ParseStatementEnd(reader, parseContext);
        }

        private                 Core.IAstNode                       _parseArgument(Core.ParserReader reader, params Core.TokenID[] ids)
        {
            reader.CurrentToken.validateToken(ids);

            if (reader.CurrentToken.isToken(Core.TokenID.LocalName))
                return AddChild(new Expr_Variable(reader));

            return ParseToken(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            if (n_ErrorNumber != null) {
                if (n_ErrorNumber is Token.Number)
                    context.ValidateInteger((Token.Number)n_ErrorNumber, 50000, int.MaxValue);
                else
                if (n_ErrorNumber is Core.AstParseNode)
                    ((Core.AstParseNode)n_ErrorNumber).TranspileNode(context);
            }

            if (n_Message is Core.AstParseNode) {
                ((Core.AstParseNode)n_Message).TranspileNode(context);
            }

            if (n_State != null) {
                if (n_State is Token.Number)
                    context.ValidateInteger((Token.Number)n_State, 0, 255);
                else
                if (n_State is Core.AstParseNode)
                    ((Core.AstParseNode)n_State).TranspileNode(context);
            }
        }
        public      override    void                                Emit(EmitWriter emitWriter)
        {
            if (n_ErrorNumber == null) {
                foreach(var child in Children) {
                    if (object.ReferenceEquals(child, n_Message)) {
                        emitWriter.WriteText("50000,");
                        child.Emit(emitWriter);
                        emitWriter.WriteText(",0");
                    }
                    else {
                        child.Emit(emitWriter);
                    }
                }
            }
            else {
                base.Emit(emitWriter);
            }
        }
    }
}
