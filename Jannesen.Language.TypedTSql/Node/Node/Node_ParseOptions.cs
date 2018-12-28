using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_ParseOptions: Core.AstParseNode
    {
        public  readonly        Core.TokenWithSymbol    n_Schema;

        public                  string                  Schema      { get ; private set; }

        public                                          Node_ParseOptions(Core.ParserReader reader)
        {
            Core.Token  token;

            while ((token = ParseOptionalToken(reader, Core.TokenID.SCHEMA)) != null) {
                switch(token.ID)
                {
                case Core.TokenID.SCHEMA:
                    Schema = (n_Schema = ParseName(reader)).ValueString;
                    break;
                }
            }
        }

        internal                void                    TranspileInit(SourceFile sourceFile, GlobalCatalog catalog)
        {
            if (n_Schema != null) {
                var name   = n_Schema.ValueString;
                var schema = catalog.GetSchema(name);
                if (schema == null)
                    throw new TranspileException(n_Schema, "Unknown schema '" + name + "'.");

                n_Schema.SetSymbol(schema);
            }
        }
        public      override    void                    TranspileNode(Transpile.Context context)
        {
            throw new InvalidOperationException();
        }
    }
}
