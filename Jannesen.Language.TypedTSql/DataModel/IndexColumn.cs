using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class IndexColumn
    {
        public      readonly    Column              Column;
        public      readonly    bool                Descending;

        public                                      IndexColumn(Column column, bool descending)
        {
            Column     = column;
            Descending = descending;
        }
        internal                                    IndexColumn(ColumnList columnList, SqlDataReader dataReader)
        {
            Column     = columnList.GetValue(dataReader.GetString (1));
            Descending = dataReader.GetBoolean(2);
        }

        internal    static      string              SqlStatement = "SELECT k.[index_id]," +                     // 0
                                                                          "c.[name]," +                         // 1
                                                                          "k.[is_descending_key]" +             // 2
                                                                    " FROM sys.index_columns k" +
                                                                         " INNER JOIN sys.columns c ON c.[object_id]=k.[object_id]" +
                                                                                                 " AND c.[column_id]=k.[column_id]" +
                                                                   " WHERE k.[object_id]=@object_id" +
                                                                " ORDER BY k.[index_id],k.[key_ordinal]";
    }
}
