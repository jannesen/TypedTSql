using System;
using System.Text;
using Jannesen.Language.TypedTSql.Logic;
using LTTSQL = Jannesen.Language.TypedTSql;

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

        public                  LTTSQL.DataModel.EntityName     BuildEntityName(string[] methods)
        {
            return BuildEntityName(n_ServiceEntitiyName, n_Name.ValueString, methods)
                        ?? throw new TranspileException(n_Name, "Invalid character in name");
        }
        public      static      LTTSQL.DataModel.EntityName     BuildEntityName(DataModel.EntityName serviceEntity, string entityName, string[] methods)
        {
            StringBuilder   rtn = new StringBuilder();

            rtn.Append(serviceEntity.Name);
            rtn.Append('/');

            if (entityName.IndexOf('{') >= 0)  {
                int             n = 0;

                for (int i = 0 ; i < entityName.Length ; ++i) {
                    char c = entityName[i];

                    switch(c) {
                    case '{':
                        if (n == 0)
                            rtn.Append("{X}");

                        ++n;
                        break;

                    case '}':
                        if (n > 0)
                            --n;
                        break;

                    default:
                        if (c < ' ') {
                            return null;
                        }

                        if (n == 0) {
                            if (!(('A' <= c && c <= 'Z') ||
                                    ('a' <= c && c <= 'z') ||
                                    ('0' <= c && c <= '9') ||
                                    (c == '-' || c == '_' || c == '~' || c == ':' || c == '/' || c == '.'))) {
                                return null;
                            }

                            rtn.Append(c);
                        }
                        break;
                    }
                }
            }
            else {
                rtn.Append(entityName);
            }

            foreach(var method in methods) {
                rtn.Append(':');
                rtn.Append(method.ToUpperInvariant());
            }

            return new LTTSQL.DataModel.EntityName(serviceEntity.Schema, rtn.ToString());
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
