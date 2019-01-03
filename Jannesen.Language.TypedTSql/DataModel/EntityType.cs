using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public abstract class EntityType: Entity, ISqlType
    {
        public      abstract    SqlTypeFlags            TypeFlags       { get; }
        public      virtual     SqlTypeNative           NativeType      { get { throw new InvalidOperationException(this.GetType().Name + ": has no nativetype.");      } }
        public      virtual     object                  DefaultValue    { get { return null;                                                                            } }
        public      virtual     InterfaceList           Interfaces      { get { throw new InvalidOperationException(this.GetType().Name + ": has no interfaces.");      } }
        public      virtual     ValueRecordList         Values          { get { throw new InvalidOperationException(this.GetType().Name + ": has no values.");          } }
        public      virtual     IColumnList             Columns         { get { throw new InvalidOperationException(this.GetType().Name + ": has no columns.");         } }
        public      virtual     IndexList               Indexes         { get { throw new InvalidOperationException(this.GetType().Name + ": has no indexes.");         } }
        public      virtual     JsonSchema              JsonSchema      { get { throw new InvalidOperationException(this.GetType().Name + ": has no json-schema.");     } }
        public      virtual     Entity                  Entity          { get { return this;                                                                            } }
        public      virtual     string                  ToSql()         { return EntityName.Fullname;                                                                     }

        protected                                       EntityType(SymbolType type, DataModel.EntityName name, EntityFlags flags): base(type, name, flags)
        {
        }

        internal    static      EntityType              NewEntityType(GlobalCatalog catalog, SqlDataReader dataReader, int coloffset)
        {
            var entityName = new DataModel.EntityName(dataReader.GetString(coloffset + 0), dataReader.GetString(coloffset + 1));

            if (dataReader.GetBoolean(coloffset + 2))
                return new EntityTypeTable(catalog, entityName, dataReader, coloffset);

            if (!dataReader.IsDBNull(coloffset + 3))
                return new EntityTypeExternal(catalog, entityName, dataReader, coloffset);

            return new EntityTypeUser(catalog, entityName, dataReader, coloffset);
        }

        internal    static      string                  SqlStatementCatalog = "SELECT [schema] = schema_name([schema_id])," +   //  0
                                                                                     "[name]," +                                //  1
                                                                                     "[is_table_type]," +                       //  2
                                                                                     "[assemblyname]  = case when t.[system_type_id] = 240 then (SELECT a.[name] FROM sys.type_assembly_usages u INNER JOIN sys.assemblies a ON a.[assembly_id]=u.[assembly_id] WHERE u.[user_type_id]=t.[user_type_id]) end," + //  3
                                                                                     "[classname]     = case when t.[system_type_id] = 240 then (SELECT z.[assembly_class] from sys.assembly_types z where z.[user_type_id] = t.[user_type_id]) end," + //  4
                                                                                     "[system_type_id]," +                      //  5
                                                                                     "[max_length]," +                          //  6
                                                                                     "[precision]," +                           //  7
                                                                                     "[scale]" +                                //  8
                                                                               " FROM sys.types t WHERE t.[system_type_id]<>t.[user_type_id]";
    }
}
