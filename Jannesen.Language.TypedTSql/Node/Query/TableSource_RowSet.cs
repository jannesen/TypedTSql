using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class TableSource_RowSet: Core.AstParseNode
    {
        public enum JoinType
        {
            NONE        = 0,
            INNER,
            LEFT_OUTER,
            RIGHT_OUTER,
            FULL_OUTER,
            CROSS_JOIN,
            CROSS_APPLY,
            OUTER_APPLY
        }

        public      abstract    Core.TokenWithSymbol            n_Alias                         { get; }
        public      virtual     JoinType                        n_JoinType                      { get { return JoinType.NONE;   } }
        public      virtual     IExprNode                       n_On                            { get { return null;            } }
        public      abstract    DataModel.IColumnList           ColumnList                      { get; }
        public      abstract    DataModel.RowSet                t_RowSet                        { get; }
        public      virtual     DataModel.ISymbol               t_Source                        { get { return null;            } }
        public      virtual     bool                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            return false;
        }
    }
}
