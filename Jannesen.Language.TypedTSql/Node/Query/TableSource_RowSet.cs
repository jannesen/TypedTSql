using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class TableSource_RowSet: Core.AstParseNode
    {
        public      abstract    Core.TokenWithSymbol            n_Alias                         { get; }
        public      virtual     DataModel.JoinType              n_JoinType                      { get { return DataModel.JoinType.NONE;     } }
        public      abstract    DataModel.IColumnList           ColumnList                      { get; }
        public      abstract    DataModel.RowSet                t_RowSet                        { get; }
        public      virtual     DataModel.ISymbol               t_Source                        { get { return null;                        } }
        public      virtual     bool                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            return false;
        }

        internal    abstract    void                            TranspileRowSet(Transpile.Context context, bool nullable);
    }
}
