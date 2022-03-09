using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_ServiceEntityName: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol            n_Schema;
        public      readonly    Core.TokenWithSymbol            n_ServiceName;
        public      readonly    DataModel.EntityName            n_ServiceEntitiyName;
        public      readonly    Core.TokenWithSymbol            n_Name;

        public                  DeclarationService              DeclarationService          { get; private set; }

        public                                                  Node_ServiceEntityName(Core.ParserReader reader)
        {
            n_ServiceName = ParseName(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Dot) != null) {
                n_Schema      = n_ServiceName;
                n_ServiceName = ParseName(reader);
                n_ServiceEntitiyName = new DataModel.EntityName(n_Schema.ValueString, n_ServiceName.ValueString);
            }
            else {
                var schema = reader.Options.Schema;
                if (schema == null)
                    throw new ParseException(n_Name, "Schema not defined.");
                n_ServiceEntitiyName = new DataModel.EntityName(schema, n_ServiceName.ValueString);
            }

            ParseToken(reader, Core.TokenID.DoubleColon);
            n_Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Name, Core.TokenID.QuotedName, Core.TokenID.String);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            DeclarationService = null;

            Validate.Schema(context, n_Schema);

            if (!context.Transpiler.ServiceDeclarations.TryGetValue(n_ServiceEntitiyName, out var service))
                throw new TranspileException(n_ServiceName, "Unknown service " + n_ServiceEntitiyName + ".");

            n_ServiceName.SetSymbolUsage(service, DataModel.SymbolUsageFlags.Reference);
            DeclarationService = service;
        }
    }
}
