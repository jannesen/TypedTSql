using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_EntityNameDefine: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol        n_Schema;
        public      readonly    Core.TokenWithSymbol        n_Name;
        public      readonly    DataModel.EntityName        n_EntitiyName;

        public                                              Node_EntityNameDefine(Core.ParserReader reader)
        {
            n_Name = ParseName(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Dot) != null) {
                n_Schema = n_Name;
                n_Name = ParseName(reader);
                n_EntitiyName = new DataModel.EntityName(n_Schema.ValueString, n_Name.ValueString);
            }
            else {
                var schema = reader.Options.Schema;
                if (schema == null)
                    throw new ParseException(n_Name, "Schema not defined.");

                InsertBefore(n_Name, new Node_CustomNode(Library.SqlStatic.QuoteNameIfNeeded(schema) + "."));
                n_EntitiyName = new DataModel.EntityName(schema, n_Name.ValueString);
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            Validate.Schema(context, n_Schema);
        }
    }
}
