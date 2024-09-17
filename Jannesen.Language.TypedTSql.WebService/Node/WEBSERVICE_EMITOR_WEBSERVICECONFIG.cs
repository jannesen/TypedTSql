using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class WEBSERVICE_EMITOR_WEBSERVICECONFIG: WEBSERVICE_EMITOR
    {
        public      readonly    string                          n_File;
        public      readonly    string                          n_Database;

        public                                                  WEBSERVICE_EMITOR_WEBSERVICECONFIG(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext)
        {
            ParseToken(reader, "WEBSERVICECONFIG");
            ParseToken(reader, Core.TokenID.LrBracket);

            while(!reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                switch(reader.CurrentToken.Text.ToUpper()) {
                case "FILE":
                    ParseToken(reader, "FILE");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_File = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "DATABASE":
                    ParseToken(reader, "DATABASE");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_Database = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                default:
                    throw new ParseException(reader.CurrentToken, "Except FILE,DATABASE got " + reader.CurrentToken.Text.ToString() + ".");
                }
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            if (n_File == null) {
                context.AddError(this, "Missing FILE.");
            }

            if (n_Database == null) {
                context.AddError(this, "Missing DATABASE.");
            }
        }
        internal    override    Emit.FileEmitor                 ConstructEmitor(string basedirectory)
        {
            return new Emit.WebServiceConfigEmitor(this, basedirectory); 
        }
    }
}
