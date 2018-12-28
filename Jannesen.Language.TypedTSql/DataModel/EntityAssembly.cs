using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityAssembly: Entity
    {
        internal                                        EntityAssembly(SymbolType type, DataModel.EntityName name, EntityFlags flags): base(type, name, flags)
        {
        }
        internal                                        EntityAssembly(SqlDataReader dataReader): base(SymbolType.Assembly, new DataModel.EntityName(null, dataReader.GetString(0)), EntityFlags.SourceDatabase)
        {
        }

        internal                void                    Update(EntityAssembly newAssembly)
        {
            throw new InvalidOperationException("Not allowed to update a locked assembly.");
        }

        internal    static      string                  SqlStatementCatalog = "SELECT [name]" +
                                                                           " FROM sys.assemblies";
    }
}
