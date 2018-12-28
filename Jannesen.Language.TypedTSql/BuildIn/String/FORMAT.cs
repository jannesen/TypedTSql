using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/format-transact-sql
    public class FORMAT: Func_Scalar_TODO
    {
        internal                                            FORMAT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
