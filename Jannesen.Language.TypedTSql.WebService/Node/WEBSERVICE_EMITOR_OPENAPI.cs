using System;
using LTTSQL = Jannesen.Language.TypedTSql;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class WEBSERVICE_EMITOR_OPENAPI: WEBSERVICE_EMITOR
    {
        public enum OptimizeComponent
        {
            None    = 0x00,
            Type    = 0x01,
            Object  = 0x02,
            Logical = 0x04
        }

        public      readonly    string                          n_File;
        public      readonly    string                          n_Title;
        public      readonly    string                          n_Version;
        public      readonly    string                          n_Path;
        public      readonly    OptimizeComponent               n_Component;

        private static  Core.ParseEnum<OptimizeComponent>       _parseComponent = new Core.ParseEnum<OptimizeComponent>(
                                                                                      "Component generation option",
                                                                                      new Core.ParseEnum<OptimizeComponent>.Seq(OptimizeComponent.None,     "NONE"),
                                                                                      new Core.ParseEnum<OptimizeComponent>.Seq(OptimizeComponent.Type,     "TYPE"),
                                                                                      new Core.ParseEnum<OptimizeComponent>.Seq(OptimizeComponent.Object,   "OBJECT"),
                                                                                      new Core.ParseEnum<OptimizeComponent>.Seq(OptimizeComponent.Logical,  "LOGICAL")
                                                                                  );

        public                                                  WEBSERVICE_EMITOR_OPENAPI(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext)
        {
            ParseToken(reader, "OPENAPI");
            ParseToken(reader, Core.TokenID.LrBracket);

            while(!reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                switch(reader.CurrentToken.Text.ToUpper()) {
                case "FILE":
                    ParseToken(reader, "FILE");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_File = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "TITLE":
                    ParseToken(reader, "TITLE");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Title = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "VERSION":
                    ParseToken(reader, "VERSION");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Version = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "PATH":
                    ParseToken(reader, "PATH");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Path = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "COMPONENT":
                    ParseToken(reader, "COMPONENT");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Component = OptimizeComponent.None;
                    do {
                        n_Component |= _parseComponent.Parse(this, reader);
                    }
                    while(ParseOptionalToken(reader, Core.TokenID.Comma) != null);
                    break;

                default:
                    throw new ParseException(reader.CurrentToken, "Except FILE,TITLE,VERSION,PATH,COMPONENT got " + reader.CurrentToken.Text.ToString() + ".");
                }
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (n_File == null) {
                context.AddError(this, "Missing FILE");
            }
            if (n_Title == null) {
                context.AddError(this, "Missing TITLE");
            }
            if (n_Version == null) {
                context.AddError(this, "Missing VERSION");
            }

        }
        internal    override    Emit.FileEmitor                 ConstructEmitor(string basedirectory)
        {
            return new Emit.OpenApiEmitor(this, basedirectory); 
        }
    }
}
