using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityService: Entity
    {
        public                                          EntityService(DataModel.EntityName name): base(SymbolType.Service, name, EntityFlags.SourceDeclaration)
        {
        }

        internal    new         void                    TranspileInit(object location)
        {
            Declaration = location;
        }
    }
}
