using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms176104.aspx
    //  Data_SelectItem::=  *
    //                   | { source_name }.*
    //                   | column_alias = expression
    //                   | expression AS column_alias
    public abstract class Query_Select_Column: Core.AstParseNode
    {
        public      abstract        void                AddColumnToList(Transpile.Context context, List<DataModel.Column> columns);
    }
}
