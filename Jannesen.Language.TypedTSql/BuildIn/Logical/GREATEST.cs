using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/logical-functions-greatest-transact-sql
    public class GREATEST: GREATEST_LEAST
    {
        internal                                            GREATEST(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
