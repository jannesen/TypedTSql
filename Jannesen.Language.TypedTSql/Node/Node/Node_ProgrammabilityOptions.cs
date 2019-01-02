using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //      WITH
    //          { ENCRYPTION
    //          | SCHEMABINDING
    //          | RETURNS NULL ON NULL INPUT
    //          | CALLED ON NULL INPUT
    //          | EXECUTE AS class } [,...n]
    //
    public class Node_ProgrammabilityOptions: Core.AstParseNode
    {
        public enum Option
        {
            ENCRYPTION                      = 0x0001,
            RECOMPILE                       = 0x0002,
            SCHEMABINDING                   = 0x0004,
            VIEW_METADATA                   = 0x0008,
            RETURNS_NULL_ON_NULL_INPUT      = 0x0010,
            CALLED_NULL_ON_NULL_INPUT       = 0x0020,
            EXECUTE_AS_CALLER               = 0x0100,
            EXECUTE_AS_SELF                 = 0x0200,
            EXECUTE_AS_OWNER                = 0x0400,
            EXECUTE_AS_PRINCIPEL            = 0x0800
        }

        public      readonly        Option                  m_Options;
        public      readonly        Token.String            m_Principel;

        public                                              Node_ProgrammabilityOptions(Core.ParserReader reader, DataModel.SymbolType type)
        {
            ParseToken(reader, Core.TokenID.WITH);

            do {
                var token  = reader.CurrentToken;
                var option = ParseEnum<Option>(reader, _parseEnum);

                switch(option) {
                case Option.EXECUTE_AS_PRINCIPEL:
                    m_Principel = (Token.String)ParseToken(reader, Core.TokenID.String);
                    break;
                }

                if ((option & ~_allowedOption(type)) != 0)
                    reader.AddError(new ParseException(token, "Option to possible."));

                m_Options |= option;
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
            if (m_Principel != null) {
                var principal = context.Catalog.GetPrincipal(m_Principel.ValueString);
                if (principal == null) {
                    context.AddError(m_Principel, "Unknown principal '" + m_Principel.ValueString + "'.");
                    return;
                }

                m_Principel.SetSymbol(principal);
                context.CaseWarning(m_Principel, principal.Name);
            }
        }

        private     static          Option                  _allowedOption(DataModel.SymbolType type)
        {
            switch(type) {
            case DataModel.SymbolType.View:
                return Option.ENCRYPTION | Option.SCHEMABINDING | Option.VIEW_METADATA;

            case DataModel.SymbolType.Function:
            case DataModel.SymbolType.FunctionScalar:
            case DataModel.SymbolType.FunctionScalar_clr:
            case DataModel.SymbolType.FunctionInlineTable:
            case DataModel.SymbolType.FunctionMultistatementTable:
            case DataModel.SymbolType.FunctionMultistatementTable_clr:
            case DataModel.SymbolType.FunctionAggregateFunction_clr:
                return Option.ENCRYPTION | Option.SCHEMABINDING | Option.RETURNS_NULL_ON_NULL_INPUT | Option.CALLED_NULL_ON_NULL_INPUT | Option.EXECUTE_AS_CALLER | Option.EXECUTE_AS_OWNER |  Option.EXECUTE_AS_SELF | Option.EXECUTE_AS_PRINCIPEL;

            case DataModel.SymbolType.StoredProcedure:
            case DataModel.SymbolType.StoredProcedure_clr:
            case DataModel.SymbolType.StoredProcedure_extended:
            case DataModel.SymbolType.ServiceMethod:
                return Option.ENCRYPTION | Option.RECOMPILE | Option.EXECUTE_AS_CALLER | Option.EXECUTE_AS_OWNER |  Option.EXECUTE_AS_SELF | Option.EXECUTE_AS_PRINCIPEL;

            case DataModel.SymbolType.Trigger:
                return Option.ENCRYPTION | Option.EXECUTE_AS_CALLER | Option.EXECUTE_AS_OWNER | Option.EXECUTE_AS_SELF | Option.EXECUTE_AS_PRINCIPEL;

            default:
                return 0;
            }
        }

        private static  Core.ParseEnum<Option>              _parseEnum = new Core.ParseEnum<Option>(
                                                                "Code option",
                                                                new Core.ParseEnum<Option>.Seq(Option.ENCRYPTION,                       "ENCRYPTION"),
                                                                new Core.ParseEnum<Option>.Seq(Option.RECOMPILE,                        "RECOMPILE"),
                                                                new Core.ParseEnum<Option>.Seq(Option.SCHEMABINDING,                    "SCHEMABINDING"),
                                                                new Core.ParseEnum<Option>.Seq(Option.VIEW_METADATA,                    "VIEW_METADATA"),
                                                                new Core.ParseEnum<Option>.Seq(Option.RETURNS_NULL_ON_NULL_INPUT,       "RETURNS", "NULL", "ON", "NULL", "INPUT"),
                                                                new Core.ParseEnum<Option>.Seq(Option.CALLED_NULL_ON_NULL_INPUT,        "CALLED", "NULL", "ON", "NULL", "INPUT"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_CALLER,                Core.TokenID.EXEC,    "AS", "CALLER"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_CALLER,                Core.TokenID.EXECUTE, "AS", "CALLER"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_SELF,                  Core.TokenID.EXEC,    "AS", "SELF"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_SELF,                  Core.TokenID.EXECUTE, "AS", "SELF"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_OWNER,                 Core.TokenID.EXEC,    "AS", "OWNER"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_OWNER,                 Core.TokenID.EXECUTE, "AS", "OWNER"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_PRINCIPEL,             Core.TokenID.EXEC,    "AS"),
                                                                new Core.ParseEnum<Option>.Seq(Option.EXECUTE_AS_PRINCIPEL,             Core.TokenID.EXECUTE, "AS")
                                                            );
    }
}
