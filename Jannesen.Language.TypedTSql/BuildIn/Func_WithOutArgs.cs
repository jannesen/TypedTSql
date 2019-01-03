using System;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn
{
    // Expression_Function:
    //      : Objectname '(' Expression ( ',' Expression )* ')'
    public abstract class Func_WithOutArgs: ExprCalculationBuildIn
    {
        public      override    DataModel.ValueFlags                ValueFlags          { get { return DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;  } }

        internal                                                    Func_WithOutArgs(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader, bool brackets=true): base(declaration, reader)
        {
            if (brackets) {
                ParseToken(reader, Core.TokenID.LrBracket);
                ParseToken(reader, Core.TokenID.RrBracket);
            }
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
        }
    }
}
