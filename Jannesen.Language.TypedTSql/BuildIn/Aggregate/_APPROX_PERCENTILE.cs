using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public abstract class _APPROX_PERCENTILE: ExprCalculationBuildIn
    {
        public      readonly    IExprNode                           n_Expression;
        public      readonly    Node_WITHIN_GROUP_ORDER_BY          n_WithinGroupOrderBy;

        public      override    DataModel.ValueFlags                ValueFlags          => _valueFlags;
        public      override    DataModel.ISqlType                  SqlType             => _sqlType;

        private                 DataModel.ValueFlags                _valueFlags;
        private                 DataModel.ISqlType                  _sqlType;

        internal                                                    _APPROX_PERCENTILE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            n_Expression = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);

           AddChild(n_WithinGroupOrderBy = new Node_WITHIN_GROUP_ORDER_BY(reader));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression.TranspileNode(context);
                n_WithinGroupOrderBy?.TranspileNode(context);

                if (n_WithinGroupOrderBy.n_OrderByItems.Length != 1) {
                    throw new TranspileException(n_WithinGroupOrderBy, "function must have exactly one expression.");
                }

                _valueFlags = DataModel.ValueFlags.Aggregaat|DataModel.ValueFlags.Nullable;
                _sqlType    = null;

                Validate.ConstNumber(n_Expression, 0, 1);

                if (n_WithinGroupOrderBy.n_OrderByItems[0].n_Expression.isValid()) {
                    Validate.ValueIntFloat(n_WithinGroupOrderBy.n_OrderByItems[0].n_Expression);
                    _sqlType = n_WithinGroupOrderBy.n_OrderByItems[0].n_Expression.SqlType;
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                _sqlType    = null;
                context.AddError(this, err);
            }
        }
    }
}
