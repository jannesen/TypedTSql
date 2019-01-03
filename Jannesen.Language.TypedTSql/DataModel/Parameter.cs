using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class Parameter: Variable
    {
        public  override        SymbolType              Type                { get { return SymbolType.Parameter; } }
        public  override        object                  Declaration         { get { return _declaration;         } }
        public  override        VariableFlags           Flags               { get { return _flags;               } }
        public                  object                  DefaultValue        { get { return _defaultValue;        } }

        protected               object                  _declaration;
        protected               VariableFlags           _flags;
        public                  object                  _defaultValue;

        public                                          Parameter(string name, DataModel.ISqlType sqlType, object declaration, VariableFlags flags, object defaultValue): base()
        {
            Name          = name;
            SqlType       = sqlType;
            _declaration  = declaration;
            _flags        = flags;
            _defaultValue = defaultValue;
        }

        internal                                        Parameter(GlobalCatalog catalog, string database, SqlDataReader dataReader)
        {
            Name            = dataReader.GetString   ( 1);
            SqlType         = dataReader.GetBoolean  (12) ? new DataModel.SqlTypeCursorRef()
                                                          : catalog.GetSqlType(database, dataReader, 2);
            _flags          = (dataReader.GetBoolean (11) ? VariableFlags.Output          : VariableFlags.None) |
                              (dataReader.GetBoolean (13) ? VariableFlags.HasDefaultValue : VariableFlags.None) |
                              (dataReader.GetBoolean (14) ? VariableFlags.XmlDocument     : VariableFlags.None) |
                              (dataReader.GetBoolean (15) ? VariableFlags.Readonly        : VariableFlags.None);

            if ((_flags & VariableFlags.HasDefaultValue) != 0)
                _defaultValue = dataReader.GetSqlValue(16);
        }

        public  override        void                    setUsed()
        {
            _flags |= VariableFlags.Used;
        }
        public  override        void                    setAssigned()
        {
            if ((_flags & VariableFlags.Readonly) != 0)
                throw new InvalidOperationException("Can't assign value const parameter variable.");

            _flags |= VariableFlags.Assigned;
        }

        internal    static  string                      SqlStatement = "SELECT p.[parameter_id]," +         //  0
                                                                              "p.[name]," +                 //  1
                                                                              "[user_type_schema] = CASE WHEN t.[user_type_id]<>t.[system_type_id] THEN SCHEMA_NAME(t.[schema_id]) END," +        //  2
                                                                              "[user_type_name] = CASE WHEN t.[user_type_id]<>t.[system_type_id] THEN t.[name] END," +                            //  3
                                                                              "t.[is_table_type]," +        //  4
                                                                              "[assemblyname] = CASE WHEN t.[system_type_id] = 240 THEN (SELECT a.[name] FROM sys.type_assembly_usages u INNER JOIN sys.assemblies a ON a.[assembly_id]=u.[assembly_id] WHERE u.[user_type_id]=t.[user_type_id]) END," + //  5
                                                                              "[classname] = CASE WHEN t.[system_type_id] = 240 THEN (SELECT z.[assembly_class] from sys.assembly_types z WHERE z.[user_type_id] = t.[user_type_id]) END," + //  6
                                                                              "p.[system_type_id]," +       //  7
                                                                              "p.[max_length]," +           //  8
                                                                              "p.[precision]," +            //  9
                                                                              "p.[scale]," +                // 10
                                                                              "p.[is_output]," +            // 11
                                                                              "p.[is_cursor_ref]," +        // 12
                                                                              "p.[has_default_value]," +    // 13
                                                                              "p.[is_xml_document]," +      // 14
                                                                              "p.[is_readonly]," +          // 15
                                                                              "p.[default_value]" +         // 16
                                                                        " FROM sys.all_parameters p" +
                                                                             " INNER JOIN sys.types t ON t.[user_type_id]=p.[user_type_id]" +
                                                                       " WHERE p.[object_id]=@object_id" +
                                                                    " ORDER BY p.[parameter_id]";
    }

    public class ParameterList: Library.ListHashName<Parameter>
    {
        public                                      ParameterList(int capacity): base(capacity)
        {
        }
        public                                      ParameterList(IList<Parameter> list): base(list)
        {
        }

        protected   override    string              ItemKey(Parameter item)
        {
            return item.Name;
        }
    }
}
