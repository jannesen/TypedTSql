using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_Subquery: ExprCalculation
    {
        public      readonly    Query_Select                    n_Select;
        public      override    DataModel.ValueFlags            ValueFlags          { get { return _sqlType != null ? DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Error;  } }
        public      override    DataModel.ISqlType              SqlType             { get { return _sqlType;                                                                                                     } }
        public      override    bool                            NoBracketsNeeded    { get { return true; } }

        private                 DataModel.ISqlType              _sqlType;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            return reader.NextPeek().ID == Core.TokenID.SELECT;
        }
        public                                                  Expr_Subquery(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.ExpressionSubquery));
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _sqlType = null;

            try {
                var contextSubquery = new Transpile.ContextSubquery(context);

                n_Select.TranspileNode(contextSubquery);

                if (n_Select.Resultset != null) {
                    if (n_Select.Resultset.Count != 1)
                        throw new TranspileException(n_Select, "Sub Query must have 1 result column.");

                    _sqlType = n_Select.Resultset[0].SqlType;
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
