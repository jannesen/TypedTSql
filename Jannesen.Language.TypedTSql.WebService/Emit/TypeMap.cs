using System;
using System.Collections.Generic;
using LTTSQL = Jannesen.Language.TypedTSql;

namespace Jannesen.Language.TypedTSql.WebService.Emit
{
    public class FromExpression
    {
        public  readonly    string      From;
        public  readonly    string      Expression;

        public                          FromExpression(string s)
        {
            s = s.Trim();
            var i = s.IndexOf(':');
            if (i < 0)
                throw new FormatException("Invalid format");

            From       = s.Substring(0, i);
            Expression = s.Substring(i+1);
        }
    }

    public class TypeMapDictionary: Dictionary<LTTSQL.DataModel.ISqlType, FromExpression>
    {
    }
}
