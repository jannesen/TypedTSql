using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ColumnDS: Column
    {
        public      override    string                  Name                    { get { return _name;                } }
        public      override    object                  Declaration             { get { return _declaration;         } }
        public      override    ISymbol                 Parent                  { get { return _parent;              } }
        public      override    ISqlType                SqlType                 { get { return _sqlType;             } }
        public      override    string                  CollationName           { get { return _collationName;       } }
        public      override    ValueFlags              ValueFlags              { get { return _flags;               } }

        private                 ISymbol                 _parent;
        private                 string                  _name;
        private                 object                  _declaration;
        private                 ISqlType                _sqlType;
        private                 string                  _collationName;
        private                 ValueFlags              _flags;

        public                                          ColumnDS(string name, DataModel.ISqlType sqlType, object declaration=null, string collationName=null, ValueFlags flags=ValueFlags.None)
        {
            if (name == null)
                throw new ArgumentNullException("name is null");

            if (sqlType == null) {
                if ((flags & ValueFlags.Error) == 0)
                    throw new ArgumentNullException("sqlType is null");

                sqlType = new SqlTypeAny();
            }

            _name                = name;
            _sqlType             = sqlType;
            _declaration         = declaration;
            _collationName       = collationName;
            _flags               = (flags & (ValueFlags.Flags | ValueFlags.ColumnFlags)) | ValueFlags.Column;
        }
        internal                                        ColumnDS(GlobalCatalog catalog, string database, SqlDataReader dataReader)
        {
            _name          = dataReader.GetString  ( 1);
            _sqlType       = catalog.GetSqlType(database, dataReader, 2);
            _collationName = dataReader.IsDBNull(11) ? null : dataReader.GetString (11);
            _flags         = (dataReader.GetBoolean (12)   ? ValueFlags.Nullable     : ValueFlags.None) |
                             (dataReader.GetBoolean (13)   ? ValueFlags.AnsiPadded   : ValueFlags.None) |
                             (dataReader.GetBoolean (14)   ? ValueFlags.Rowguidcol   : ValueFlags.None) |
                             (dataReader.GetBoolean (15)   ? ValueFlags.Identity     : ValueFlags.None) |
                             (dataReader.GetBoolean (16)   ? ValueFlags.Computed     : ValueFlags.None) |
                             (dataReader.GetBoolean (17)   ? ValueFlags.Filestream   : ValueFlags.None) |
                             (dataReader.GetBoolean (18)   ? ValueFlags.XmlDocument  : ValueFlags.None) |
                             (dataReader.GetBoolean (19)   ? ValueFlags.Sparse       : ValueFlags.None) |
                             (dataReader.GetBoolean (20)   ? ValueFlags.ColumnSet    : ValueFlags.None) |
                             (dataReader.GetInt32(21) != 0 ? ValueFlags.HasDefault   : ValueFlags.None) |
                             ValueFlags.Column;

            if (_collationName == catalog.DefaultCollation)
                _collationName = "database_default";
        }

        internal                void                    SetParent(DataModel.ISymbol parent)
        {
            if (this._parent != null) {
                if (this._parent != parent) {
                    if (this._parent.Type == SymbolType.TableUser    ||
                        this._parent.Type == SymbolType.TableSystem  ||
                        this._parent.Type == SymbolType.TableInternal)
                        return;

                    System.Diagnostics.Debugger.Break();
                }
            }
            this._parent = parent;
        }

        internal    static      string                  SqlStatement = "SELECT c.[column_id]," +            //  0
                                                                              "c.[name]," +                 //  1
                                                                              "[user_type_schema] = CASE WHEN t.[user_type_id]<>t.[system_type_id] THEN SCHEMA_NAME(t.[schema_id]) END," +        //  2
                                                                              "[user_type_name] = CASE WHEN t.[user_type_id]<>t.[system_type_id] THEN t.[name] END," +                            //  3
                                                                              "t.[is_table_type]," +        //  4
                                                                              "[assemblyname] = CASE WHEn t.[system_type_id] = 240 THEN (SELECT a.[name] FROM sys.type_assembly_usages u INNER JOIN sys.assemblies a ON a.[assembly_id]=u.[assembly_id] WHERE u.[user_type_id]=t.[user_type_id]) end," + //  5
                                                                              "[classname] = CASE WHEN t.[system_type_id] = 240 THEN (SELECT z.[assembly_class] from sys.assembly_types z WHERE z.[user_type_id] = t.[user_type_id]) end," + //  6
                                                                              "c.[system_type_id]," +       //  7
                                                                              "c.[max_length]," +           //  8
                                                                              "c.[precision]," +            //  9
                                                                              "c.[scale]," +                // 10
                                                                              "c.[collation_name]," +       // 11
                                                                              "c.[is_nullable]," +          // 12
                                                                              "c.[is_ansi_padded]," +       // 13
                                                                              "c.[is_rowguidcol]," +        // 14
                                                                              "c.[is_identity]," +          // 15
                                                                              "c.[is_computed]," +          // 16
                                                                              "c.[is_filestream]," +        // 17
                                                                              "c.[is_xml_document]," +      // 18
                                                                              "c.[is_sparse]," +            // 19
                                                                              "c.[is_column_set]," +        // 20
                                                                              "[has_default] = CASE WHEN c.[default_object_id] <> 0 THEN 1 ELSE 0 END" + // 21
                                                                        " FROM sys.all_columns c" +
                                                                               " INNER JOIN sys.types t on t.[user_type_id]=c.[user_type_id]" +
                                                                       " WHERE c.[object_id]=@object_id"+
                                                                    " ORDER BY c.[column_id]";
    }
}
