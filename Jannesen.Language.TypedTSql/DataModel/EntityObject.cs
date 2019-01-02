using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public abstract class EntityObject: Entity
    {
        protected                                           EntityObject(SymbolType type, DataModel.EntityName name, EntityFlags flags): base(type, name, flags)
        {
            if ((flags & EntityFlags.SourceDatabase) != 0) {
                if (this.Type != SymbolType.Trigger && this.Type != SymbolType.Trigger_clr)
                    this.EntityFlags |= EntityFlags.PartialLoaded;
            }
        }

        internal    static      EntityObject                ReadFromDatabase(string database, SqlDataReader dataReader)
        {
            var entityType = Library.SqlStatic.ParseObjectType(dataReader.GetString(2));
            var entityName = new DataModel.EntityName(database, dataReader.GetString(0), dataReader.GetString(1));

            switch(entityType) {
            case SymbolType.TableInternal:
            case SymbolType.TableSystem:
            case SymbolType.TableUser:
                return new EntityObjectTable(entityType, entityName, EntityFlags.SourceDatabase);

            default:
                return new EntityObjectCode(entityType, entityName, EntityFlags.SourceDatabase);
            }
        }

        public      static      string                      SqlStatementCatalog = "SELECT [schema]=SCHEMA_NAME([schema_id]),[name],[type]" +
                                                                                   " FROM sys.objects" +
                                                                                  " WHERE [type] in ('AF','FN','FS','FT','IF','P ','PC','TA','TF','TR','U ','V ','X ')";
        public      static      string                      SqlStatementByName(EntityName name)
        {
            return "EXEC " + (name.Database!=null ? name.Database+".":"")+ "sys.sp_executesql " +
                                    Library.SqlStatic.QuoteNString("DECLARE @object_id INT=OBJECT_ID(@objectname)\n" +
                                                                   "SELECT [schema]=SCHEMA_NAME([schema_id]),[name],[type] FROM sys.all_objects WHERE [object_id]=@object_id") +
                                ",\n N'@objectname nvarchar(1024)', @objectname=" + Library.SqlStatic.QuoteString(name.SchemaName);
        }
    }
}
