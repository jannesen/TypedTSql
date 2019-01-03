using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://msdn.microsoft.com/en-us/library/ms175997.aspx
    public class COUNT: ExprCalculationBuildIn
    {
        public      readonly    IExprNode                   n_Expression;
        public      override    DataModel.ValueFlags        ValueFlags          { get { return DataModel.ValueFlags.Aggregaat|DataModel.ValueFlags.Function; } }
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.Int;                                  } }

        internal                                            COUNT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            if (ParseOptionalToken(reader, Core.TokenID.Star) != null)
            { }
            else {
                ParseOptionalToken(reader, Core.TokenID.DISTINCT);
                n_Expression = ParseExpression(reader);
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression?.TranspileNode(context);

                if (n_Expression != null)
                    Validate.Value(n_Expression);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
