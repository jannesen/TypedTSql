using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum PrincipalType
    {
        DATABASE_ROLE               = 1,
        APPLICATION_ROLE,
        SQL_USER,
        CERTIFICATE_MAPPED_USER,
        ASYMMETRIC_KEY_MAPPED_USER,
        EXTERNAL_GROUPS,
        EXTERNAL_USER,
        WINDOWS_GROUP,
        WINDOWS_USER
    }

    public class DatabasePrincipal: ISymbol
    {
        public                  SymbolType              Type                    { get { return SymbolType.DatabasePrincipal; } }
        public                  string                  Name                    { get; private set; }
        public                  object                  Declaration             { get { return null; } }
        public                  DataModel.ISymbol       Parent                  { get { return null; } }
        public                  DataModel.ISymbol       SymbolNameReference     { get { return null; } }
        public                  PrincipalType           PrincipalType           { get; private set; }

        public                                          DatabasePrincipal(SqlDataReader dataReader)
        {
            Name          = dataReader.GetString(0);
            PrincipalType = _mapDatabaseTypeToPrincipalType(dataReader.GetString(1));
        }

        private     static      PrincipalType           _mapDatabaseTypeToPrincipalType(string type)
        {
            switch(type) {
            case "A":       return PrincipalType.APPLICATION_ROLE;
            case "C":       return PrincipalType.CERTIFICATE_MAPPED_USER;
            case "E":       return PrincipalType.EXTERNAL_USER;
            case "G":       return PrincipalType.WINDOWS_GROUP;
            case "K":       return PrincipalType.ASYMMETRIC_KEY_MAPPED_USER;
            case "R":       return PrincipalType.DATABASE_ROLE;
            case "S":       return PrincipalType.SQL_USER;
            case "U":       return PrincipalType.WINDOWS_USER;
            case "X":       return PrincipalType.EXTERNAL_GROUPS;
            default:        throw new GlobalCatalogException("Unknown principal-type '" + type + "'.");
            }
        }

        internal    const       string                  SqlStatementCatalog = "SELECT [name], [type]" +
                                                                               " FROM sys.database_principals";
    }

    public class DatabasePrincipalList: Library.ListHash<DatabasePrincipal, string>
    {
        public                                          DatabasePrincipalList(int capacity): base(capacity)
        {
        }

        protected   override    string                  ItemKey(DatabasePrincipal item)
        {
            return item.Name;
        }
    }
}
