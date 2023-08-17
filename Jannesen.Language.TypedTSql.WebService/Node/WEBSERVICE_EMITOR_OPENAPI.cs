using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class WEBSERVICE_EMITOR_OPENAPI: WEBSERVICE_EMITOR
    {
        public      readonly    string                          n_Title;
        public      readonly    string                          n_Version;

        public                                                  WEBSERVICE_EMITOR_OPENAPI(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext)
        {
            ParseToken(reader, "OPENAPI");
            ParseToken(reader, Core.TokenID.LrBracket);

            while(!reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                switch(reader.CurrentToken.Text.ToUpper()) {
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

                default:
                    throw new ParseException(reader.CurrentToken, "Except TITLE,VERSION got " + reader.CurrentToken.Text.ToString() + ".");
                }
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
        }
        internal    override    Emit.FileEmitor                 ConstructEmitor(string basedirectory)
        {
            return new Emit.OpenApiEmitor(this, basedirectory); 
        }
    }
}
