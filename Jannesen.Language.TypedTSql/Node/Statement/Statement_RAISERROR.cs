using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms178592.aspx
    //      RAISERROR ( { msg_id | msg_str | @local_variable }
    //          { ,severity ,state }
    //          [ ,argument [ ,...n ] ] )
    //          [ WITH { "log" | "nowait" | "seterror"} [ ,...n ] ]
    [StatementParser(Core.TokenID.RAISERROR)]
    public class Statement_RAISERROR: Statement
    {
        public      readonly    IExprNode                           n_Message;
        public      readonly    IExprNode                           n_Severity;
        public      readonly    IExprNode                           n_State;

        public      readonly    Core.IAstNode[]                     n_Arguments;

        public                                                      Statement_RAISERROR(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.RAISERROR);
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Message  = ParseSimpleExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Severity = ParseSimpleExpression(reader, constValue:true);
            ParseToken(reader, Core.TokenID.Comma);
            n_State    = ParseSimpleExpression(reader, constValue:true);

            if (reader.CurrentToken.isToken(Core.TokenID.Comma)) {
                var arguments = new List<Core.IAstNode>();

                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                    arguments.Add(_parseArgument(reader, Core.TokenID.Number, Core.TokenID.String, Core.TokenID.LocalName));
                }

                n_Arguments = arguments.ToArray();
            }

            ParseToken(reader, Core.TokenID.RrBracket);

            if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                do {
                    ParseToken(reader, "LOG", "NOWAIT", "SETERROR");
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
            }

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Message.TranspileNode(context);
            n_Severity.TranspileNode(context);
            n_State.TranspileNode(context);

            switch(n_Message.SqlType.NativeType.SystemType) {
            case DataModel.SystemType.Int:
            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
                break;

            default:
                context.AddError(n_Message, "Expect int or string.");
                break;
            }

            context.ValidateInteger(n_Severity, 0,  25);
            context.ValidateInteger(n_State, -1, 255);

            if (n_Arguments != null) {
                foreach (var arg in n_Arguments) {
                    if (arg is Core.AstParseNode parseNode)
                        parseNode.TranspileNode(context);
                }
            }
        }

        private                 Core.IAstNode                       _parseArgument(Core.ParserReader reader, params Core.TokenID[] ids)
        {
            reader.CurrentToken.validateToken(ids);

            if (reader.CurrentToken.isToken(Core.TokenID.LocalName))
                return AddChild(new Expr_PrimativeValue(reader, localVariable:true));

            return ParseToken(reader);
        }
    }
}
