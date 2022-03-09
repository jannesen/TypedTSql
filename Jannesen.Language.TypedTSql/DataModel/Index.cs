using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum IndexFlags
    {
        None            = 0,
        PrimaryKey      = 0x0001,
        Unique          = 0x0002,
        Clustered       = 0x0004
    }

    public class Index: ISymbol
    {
        public                  SymbolType          Type                    { get { return SymbolType.Index; } }
        public                  IndexFlags          Flags                   { get; private set; }
        public                  string              Name                    { get; private set; }
        public                  string              FullName             { get { return (ParentSymbol.FullName ?? "???") + "." + SqlStatic.QuoteName(Name); } }
        public                  IndexColumn[]       Columns                 { get; private set; }
        public                  string              Filter                  { get; private set; }
        public                  object              Declaration             { get; private set; }
        public                  ISymbol             ParentSymbol            { get; private set; }
        public                  DataModel.ISymbol   SymbolNameReference     { get { return null; } }

        public                                      Index(IndexFlags flags, string name, IndexColumn[] columns, string filter = null, object declaration = null)
        {
            Flags       = flags;
            Name        = name;
            Columns     = columns;
            Filter      = filter;
            Declaration = declaration;
        }
        internal                                    Index(SqlDataReader dataReader, IndexColumn[] columns)
        {
            Flags   = (dataReader.GetBoolean(2)    ? IndexFlags.PrimaryKey : IndexFlags.None) |
                      (dataReader.GetBoolean(3)    ? IndexFlags.Unique     : IndexFlags.None) |
                      (dataReader.GetInt32(4) != 0 ? IndexFlags.Clustered  : IndexFlags.None);
            Name    = dataReader.GetString (1);
            Filter  = !dataReader.IsDBNull(5) ? dataReader.GetString (5) : null;
            Columns = columns;
        }

        internal                void                SetParent(DataModel.ISymbol parent)
        {
            this.ParentSymbol = parent;
        }

        public      override    int                 GetHashCode()
        {
            return Name.GetHashCode();
        }
        public      override    bool                Equals(Object obj)
        {
            if (obj is Index idx2) {
                if (this.Name           == idx2.Name        &&
                    this.Flags          == idx2.Flags       &&
                    this.Filter         == idx2.Filter      &&
                    this.Columns.Length == idx2.Columns.Length &&
                    this.Declaration    == idx2.Declaration)
                {
                    for (int i = 0 ; i < this.Columns.Length ; ++i) {
                        if (!this.Columns[i].Equals(idx2.Columns[i]))
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        internal    static      string              SqlStatement = "SELECT i.[index_id]," +                                                             // 0
                                                                          "i.[name]," +                                                                 // 1
                                                                          "i.[is_primary_key]," +                                                       // 2
                                                                          "i.[is_unique]," +                                                            // 3
                                                                          "[clusted] = CASE WHEN i.[type] = 1 THEN 1 ELSE 0 END," +                     // 4
                                                                          "[filter] = CASE WHEN i.[has_filter] <> 0 THEN i.[filter_definition] END" +   // 5
                                                                    " FROM sys.indexes i" +
                                                                   " WHERE i.[object_id]=@object_id" +
                                                                     " AND i.[type]>0" +
                                                                     " AND i.[is_unique_constraint]=0" +
                                                                     " AND i.[is_hypothetical]=0" +
                                                                " ORDER BY i.[index_id]";
    }

    public class IndexList: Library.ListHashName<Index>
    {
        public                  Index               PrimaryKey
        {
            get {
                foreach(var index in this) {
                    if ((index.Flags & IndexFlags.PrimaryKey) == IndexFlags.PrimaryKey)
                        return index;
                }

                return null;
            }
        }

        public                                      IndexList(int capacity): base(capacity)
        {
        }
        public                                      IndexList(List<Index> indexes): base(indexes)
        {
        }

        public      static      IndexList           ReadFromDatabase(ColumnList columns, SqlDataReader dataReader)
        {
            // Read indexColumns and indexes
            {
                if (!dataReader.NextResult())
                    throw new ErrorException("Can't goto IndexColumns result.");

                var     indexColumns = new Dictionary<int, List<IndexColumn>>();
                var     indexes      = new List<Index>();

                while (dataReader.Read()) {
                    int                 index_id = dataReader.GetInt32(0);

                    if (!indexColumns.TryGetValue(index_id, out List<IndexColumn> ic))
                        indexColumns.Add(index_id, ic = new List<IndexColumn>());

                    ic.Add(new IndexColumn(columns, dataReader));
                }

                if (!dataReader.NextResult())
                    throw new ErrorException("Can't goto Index result.");

                while (dataReader.Read()) {
                    int                 index_id = dataReader.GetInt32(0);

                    if (!indexColumns.TryGetValue(index_id, out List<IndexColumn> ic))
                        throw new ErrorException("Index " + index_id + " has no columns.");

                    indexes.Add(new Index(dataReader, ic.ToArray()));
                }

                return (indexes.Count > 0) ? new IndexList(indexes) : null;
            }
        }

        protected   override    string              ItemKey(Index item)
        {
            return item.Name;
        }
    }
}
