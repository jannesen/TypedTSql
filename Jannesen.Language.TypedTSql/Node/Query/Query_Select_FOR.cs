using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms173812.aspx
    public class Query_Select_FOR: Core.AstParseNode
    {
        public enum ForMode
        {
            XML_AUTO            = 1,
            XML_EXPLICIT,
            XML_PATH,
            XML_RAW,
        }

        [Flags]
        public enum ForOptions
        {
            None                = 0,
            TYPE                = 0x0001,
            ROOT                = 0x0002,
            ELEMENTS_XSINIL     = 0x0004,
            ELEMENTS_ABSENT     = 0x0008,
            ELEMENTS            = 0x0010,
            BINARY_BASE64       = 0x0020,
            XMLDATA             = 0x0040,
            XMLSCHEMA           = 0x0080
        }

        public      readonly    ForMode     Mode;
        public      readonly    ForOptions  Options;
        public      readonly    string      ElementName;
        public      readonly    string      Root;
        public      readonly    string      XmlSchema;

        public                              Query_Select_FOR(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.FOR);

            Mode = ParseEnum<ForMode>(reader, _parseEnumMode);

            ForOptions allowed = ForOptions.None;

            switch(Mode) {
            case ForMode.XML_AUTO:
                allowed = ForOptions.TYPE | ForOptions.ROOT | ForOptions.ELEMENTS_XSINIL | ForOptions.ELEMENTS_ABSENT | ForOptions.ELEMENTS | ForOptions.BINARY_BASE64 | ForOptions.XMLDATA | ForOptions.XMLSCHEMA;
                break;
            case ForMode.XML_EXPLICIT:
                allowed = ForOptions.TYPE | ForOptions.ROOT                                                                                 | ForOptions.BINARY_BASE64 | ForOptions.XMLDATA;
                break;
            case ForMode.XML_PATH:
                ElementName = _parseOptionName(reader);
                allowed = ForOptions.TYPE | ForOptions.ROOT | ForOptions.ELEMENTS_XSINIL | ForOptions.ELEMENTS_ABSENT | ForOptions.ELEMENTS | ForOptions.BINARY_BASE64 | ForOptions.XMLDATA | ForOptions.XMLSCHEMA;
                break;
            case ForMode.XML_RAW:
                ElementName = _parseOptionName(reader);
                allowed = ForOptions.TYPE | ForOptions.ROOT | ForOptions.ELEMENTS_XSINIL | ForOptions.ELEMENTS_ABSENT | ForOptions.ELEMENTS | ForOptions.BINARY_BASE64;
                break;
            }

            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                var option = ParseEnum<ForOptions>(reader, _parseEnumOptions);

                if ((option & ~allowed) != 0)
                    reader.AddError(new Exception("Option " + option + " not available with " + Mode + "."));

                if ((Options & option) != 0)
                    reader.AddError(new Exception("Option already defined."));

                Options |= Options;

                switch(option) {
                case ForOptions.ROOT:
                    Root = _parseOptionName(reader);
                    break;

                case ForOptions.XMLSCHEMA:
                    XmlSchema = _parseOptionName(reader);
                    break;
                }
            }
        }

        public      override    void        TranspileNode(Transpile.Context context)
        {
        }

        private     string                  _parseOptionName(Core.ParserReader reader)
        {
            string      rtn = null;

            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                rtn = ParseToken(reader, Core.TokenID.String).ValueString;
                ParseToken(reader, Core.TokenID.RrBracket);
            }

            return rtn;
        }

        private static  Core.ParseEnum<ForMode>             _parseEnumMode = new Core.ParseEnum<ForMode>(
                                                                "FOR Clause Mode",
                                                                new Core.ParseEnum<ForMode>.Seq(ForMode.XML_AUTO,           "XML", "AUTO"               ),
                                                                new Core.ParseEnum<ForMode>.Seq(ForMode.XML_EXPLICIT,       "XML", "EXPLICIT"           ),
                                                                new Core.ParseEnum<ForMode>.Seq(ForMode.XML_PATH,           "XML", "PATH"               ),
                                                                new Core.ParseEnum<ForMode>.Seq(ForMode.XML_RAW,            "XML", "RAW"                )
                                                            );
        private static  Core.ParseEnum<ForOptions>          _parseEnumOptions = new Core.ParseEnum<ForOptions>(
                                                                "FOR Clause Option",
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.TYPE,             "TYPE"                  ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.ROOT,             "ROOT"                  ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.ELEMENTS_XSINIL,  "ELEMENTS", "XSINIL"    ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.ELEMENTS_ABSENT,  "ELEMENTS", "ABSENT"    ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.ELEMENTS,         "ELEMENTS"              ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.BINARY_BASE64 ,   "BINARY",   "BASE64"    ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.XMLDATA,          "XMLDATA"               ),
                                                                new Core.ParseEnum<ForOptions>.Seq(ForOptions.XMLSCHEMA,        "XMLSCHEMA"             )
                                                            );
    }
}
