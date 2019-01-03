using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class DatabaseSchema: ISymbol
    {
        public                  SymbolType              Type                    { get { return SymbolType.Schema; } }
        public                  string                  Name                    { get; private set; }
        public                  object                  Declaration             { get { return null; } }
        public                  DataModel.ISymbol       Parent                  { get { return null; } }
        public                  DataModel.ISymbol       SymbolNameReference     { get { return null; } }

        public                                          DatabaseSchema(SqlDataReader dataReader)
        {
            Name          = dataReader.GetString(0);
        }

        internal    static      string                  SqlStatementCatalog = "SELECT [name]" +
                                                                               " FROM sys.schemas";
    }

    public class DatabaseSchemaList: Library.ListHash<DatabaseSchema, string>
    {
        public                                          DatabaseSchemaList(int capacity): base(capacity)
        {
        }

        protected   override    string                  ItemKey(DatabaseSchema item)
        {
            return item.Name;
        }
    }
}
