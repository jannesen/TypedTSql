using System;
using System.Collections.Generic;
using System.IO;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class DeclarationServiceMethod: DeclarationObjectCode
    {
        public      override    DataModel.SymbolType            EntityType                  { get { return DataModel.SymbolType.ServiceMethod;  } }
        public      override    bool                            callableFromCode            { get { return true;                                } }
        public      override    ObjectReturnOption              ReturnOption                { get { return ObjectReturnOption.Optional;         } }
        public      override    DataModel.ISqlType              ReturnType                  { get { return DataModel.SqlTypeNative.Int;         } }

        public      abstract    DataModel.EntityName            ServiceName                 { get; }

        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(EntityName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('P'))");
            stringWriter.Write("    DROP PROCEDURE ");
                stringWriter.WriteLine(EntityName.Fullname);
        }
    }
}
