using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class APPROX_PERCENTILE_CONT: _APPROX_PERCENTILE
    {
        internal                                                    APPROX_PERCENTILE_CONT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
