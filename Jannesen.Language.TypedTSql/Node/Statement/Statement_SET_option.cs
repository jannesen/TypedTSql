using System;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms190356.aspx
    [StatementParser(Core.TokenID.SET, prio:2)]
    public class Statement_SET_option: Statement
    {
        public      readonly    string[]                            n_Options;
        public      readonly    Node_EntityNameReference            n_Table;
        public      readonly    Core.IAstNode                       n_Value;


        internal    static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.SET && reader.NextPeek().isNameOrKeyword;
        }
        public                                                      Statement_SET_option(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.SET);

            string option = ParseToken(reader, "ANSI_DEFAULTS", "ANSI_NULLS", "ANSI_NULL_DFLT_OFF", "ANSI_NULL_DFLT_ON", "ANSI_PADDING", "ANSI_WARNINGS", "ARITHABORT", "ARITHIGNORE", "CONCAT_NULL_YIELDS_NULL", "CONTEXT_INFO", "CURSOR_CLOSE_ON_COMMIT", "DATEFIRST", "DATEFORMAT", "DEADLOCK_PRIORITY", "FIPS_FLAGGER", "FORCEPLAN", "IDENTITY_INSERT", "IMPLICIT_TRANSACTIONS", "LANGUAGE", "LOCK_TIMEOUT", "NOCOUNT", "NOEXEC", "NUMERIC_ROUNDABORT", "PARSEONLY", "QUERY_GOVERNOR_COST_LIMIT", "QUOTED_IDENTIFIER", "REMOTE_PROC_TRANSACTIONS", "ROWCOUNT", "SHOWPLAN_ALL", "SHOWPLAN_TEXT", "SHOWPLAN_XML", "STATISTICSIO", "STATISTICSPROFILE", "STATISTICSTIME", "STATISTICSXML", "TEXTSIZE", "TRANSACTION", "XACT_ABORT").Text.ToUpper();
            n_Options = new string[] { option };

            switch(option)
            {
            // https://msdn.microsoft.com/en-us/library/ms187768.aspx
            case "CONTEXT_INFO":
                n_Value = ParseSimpleExpression(reader);
                break;

            // https://msdn.microsoft.com/en-US/library/ms181598.aspx
            case "DATEFIRST":
                n_Value = ParseSimpleExpression(reader, constValue:true);
                break;

            // https://msdn.microsoft.com/en-US/library/ms189491.aspx
            case "DATEFORMAT":
                n_Value = ParseToken(reader, "MDY", "DMY", "YMD", "YDM", "MYD", "DYM");
                break;

            // https://msdn.microsoft.com/en-us/library/ms186736.aspx
            case "DEADLOCK_PRIORITY":
                switch(reader.CurrentToken.ID)
                {
                case Core.TokenID.Name:         n_Value = ParseToken(reader, new string[] { "LOW", "NORMAL", "HIGH"});  break;
                default:                        n_Value = ParseSimpleExpression(reader);                                break;
                }
                break;

            // https://msdn.microsoft.com/en-us/library/ms189781.aspx
            case "FIPS_FLAGGER":
                n_Value = ParseToken(reader, "OFF", "ENTRY", "FULL", "FULL");
                break;

            // https://msdn.microsoft.com/en-us/library/ms188059.aspx
            case "IDENTITY_INSERT":
                n_Table = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Table));
                n_Value = ParseToken(reader, "ON", "OFF");
                break;

            // https://msdn.microsoft.com/en-us/library/ms174398.aspx
            case "LANGUAGE":
                n_Value = ParseName(reader);
                break;

            // https://msdn.microsoft.com/en-US/library/ms189470.aspx
            case "LOCK_TIMEOUT":
                n_Value = ParseSimpleExpression(reader, constValue:true);
                break;

            // https://msdn.microsoft.com/en-us/library/ms176100.aspx
            case "QUERY_GOVERNOR_COST_LIMIT":
                n_Value = ParseSimpleExpression(reader, constValue:true);
                break;

            // https://msdn.microsoft.com/en-US/library/ms188774.aspx
            case "ROWCOUNT":
                throw new ParseException(reader.CurrentToken, "Don't use rowcount, use select with top instead, see https://msdn.microsoft.com/en-US/library/ms188774.aspx.");

            // https://msdn.microsoft.com/en-US/library/ms186238.aspx
            case "TEXTSIZE":
                n_Value = ParseSimpleExpression(reader, constValue:true);
                break;

            // https://msdn.microsoft.com/en-US/library/ms173763.aspx
            case "TRANSACTION":
                ParseToken(reader, "ISOLATION");
                ParseToken(reader, "LEVEL");

                switch(ParseToken(reader, "READ", "REPEATABLE", "SNAPSHOT", "SERIALIZABLE").Text.ToUpper())
                {
                case "READ":
                    ParseToken(reader, "UNCOMMITTED", "COMMITTED");
                    break;

                case "REPEATABLE":
                    ParseToken(reader, "READ");
                    break;
                }
                break;

            // Simple ON/OFF
            default:
                if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                    option = ParseToken(reader, "ANSI_DEFAULTS","ANSI_NULLS","ANSI_NULL_DFLT_OFF","ANSI_NULL_DFLT_ON","ANSI_PADDING","ANSI_WARNINGS","ARITHABORT","ARITHIGNORE","CONCAT_NULL_YIELDS_NULL","CURSOR_CLOSE_ON_COMMIT","FORCEPLAN", "IMPLICIT_TRANSACTIONS","NOCOUNT","NOEXEC","NUMERIC_ROUNDABORT", "PARSEONLY", "QUOTED_IDENTIFIER", "REMOTE_PROC_TRANSACTIONS","SHOWPLAN_ALL","SHOWPLAN_TEXT","SHOWPLAN_XML","STATISTICSIO","STATISTICSPROFILE","STATISTICSTIME","STATISTICSXML", "XACT_ABORT").Text.ToUpper();
                    Array.Resize<string>(ref n_Options, n_Options.Length + 1);
                }

                n_Value = ParseToken(reader, "ON", "OFF");
                break;
            }

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Table?.TranspileNode(context);

            if (n_Value is IExprNode valueExpr) {
                valueExpr.TranspileNode(context);

                switch(n_Options[0])
                {
                case "DEADLOCK_PRIORITY":
                    Validate.ValueInt(valueExpr);

                    if (valueExpr.ExpressionType == ExprType.Const)
                        context.ValidateInteger(valueExpr, -10, 10);
                    break;

                case "DATEFIRST":
                    Validate.ValueInt(valueExpr);
                    context.ValidateInteger((IExprNode)n_Value, 1, 7);
                    break;

                case "LOCK_TIMEOUT":
                    Validate.ValueInt(valueExpr);
                    context.ValidateInteger((IExprNode)n_Value, -1, 3600000);
                    break;

                case "TEXTSIZE":
                    Validate.ValueInt(valueExpr);
                    context.ValidateInteger((IExprNode)n_Value, 0, 2000000000);
                    break;
                }
            }
        }
    }
}
