using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public interface ITable: ISymbol
    {
        IColumnList             Columns             { get; }
        IndexList               Indexes             { get; }
    }
}
