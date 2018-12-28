using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    //      GRANT Permissions TO DatabasePrincipal,...
    //
    public class Node_ObjectGrant: Core.AstParseNode
    {
        [Flags]
        public enum Permissions
        {
            SELECT              = 0x0001,
            INSERT              = 0x0002,
            UPDATE              = 0x0004,
            DELETE              = 0x0008,
            EXECUTE             = 0x0010,
            REFERENCES          = 0x0020,
            VIEW_DEFINITION     = 0x0040,
            CONTROL             = 0x0080,
            TAKE_OWNERSHIP      = 0x0100
        }

        public      readonly        Permissions                     n_Permissions;
        public      readonly        Core.TokenWithSymbol[]          n_DatabasePrincipals;

        public                                                      Node_ObjectGrant(Core.ParserReader reader, DataModel.SymbolType type)
        {
            var permissions        = new List<Core.Token>();
            var databaseprincipals = new List<Core.TokenWithSymbol>();

            ParseToken(reader, Core.TokenID.GRANT);

            do {
                n_Permissions |= ParseEnum<Permissions>(reader, _permissionsEnum);
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.TO);

            do {
                databaseprincipals.Add(ParseName(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_DatabasePrincipals = databaseprincipals.ToArray();
        }
        public      override        void                            TranspileNode(Transpile.Context context)
        {
            foreach(var dp in n_DatabasePrincipals) {
                var principal = context.Catalog.GetPrincipal(dp.ValueString);
                if (principal == null) {
                    context.AddError(dp, "Unknown principal '" + dp.ValueString + "'.");
                    continue;
                }

                dp.SetSymbol(principal);
                context.CaseWarning(dp, principal.Name);
            }
        }
        public                      void                            EmitGrant(string securable, DataModel.EntityName objectname, Core.EmitWriter emitWriter)
        {
            emitWriter.WriteText("GRANT ");

            var permissions = n_Permissions;
            var p           = (Permissions)1;

            while (permissions != 0) {
                if ((permissions & p) != 0) {
                    switch(p)
                    {
                    case Permissions.SELECT:            emitWriter.WriteText("SELECT");             break;
                    case Permissions.INSERT:            emitWriter.WriteText("INSERT");             break;
                    case Permissions.UPDATE:            emitWriter.WriteText("UPDATE");             break;
                    case Permissions.DELETE:            emitWriter.WriteText("DELETE");             break;
                    case Permissions.EXECUTE:           emitWriter.WriteText("EXECUTE");            break;
                    case Permissions.REFERENCES:        emitWriter.WriteText("REFERENCES");         break;
                    case Permissions.VIEW_DEFINITION:   emitWriter.WriteText("VIEW_DEFINITION");    break;
                    case Permissions.CONTROL:           emitWriter.WriteText("CONTROL");            break;
                    case Permissions.TAKE_OWNERSHIP:    emitWriter.WriteText("TAKE_OWNERSHIP");     break;
                    }

                    permissions &= ~p;

                    if (permissions != 0)
                        emitWriter.WriteText(",");
                }

                p = (Permissions)((int)p << 1);
            }

            emitWriter.WriteText(" ON ");
            emitWriter.WriteText(securable);
            emitWriter.WriteText("::");
            emitWriter.WriteText(objectname.Fullname);
            emitWriter.WriteText(" TO ");


            for (int i = 0 ; i < n_DatabasePrincipals.Length ; ++i) {
                if (i > 0)
                    emitWriter.WriteText(",");

                n_DatabasePrincipals[i].Emit(emitWriter);
            }

            emitWriter.WriteText(";");
        }

        private static  Core.ParseEnum<Permissions>              _permissionsEnum = new Core.ParseEnum<Permissions>(
                                                                    "Permissions",
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.SELECT,             Core.TokenID.SELECT),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.INSERT,             Core.TokenID.INSERT),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.UPDATE,             Core.TokenID.UPDATE),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.DELETE,             Core.TokenID.DELETE),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.EXECUTE,            Core.TokenID.EXECUTE),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.REFERENCES,         Core.TokenID.REFERENCES),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.VIEW_DEFINITION,    "VIEW", "DEFINITION"),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.CONTROL,            "CONTROL"),
                                                                    new Core.ParseEnum<Permissions>.Seq(Permissions.TAKE_OWNERSHIP,     "TAKE", "OWNERSHIP")
                                                                );
    }
}
