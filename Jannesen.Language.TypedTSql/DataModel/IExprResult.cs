using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public interface IExprResult
    {
        ValueFlags                  ValueFlags          { get; }
        ISqlType                    SqlType             { get; }
        string                      CollationName       { get; }
        bool                        ValidateConst(ISqlType sqlType);
    }
}
