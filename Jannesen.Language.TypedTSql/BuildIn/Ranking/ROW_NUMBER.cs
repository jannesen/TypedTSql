using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/row-number-transact-sq
    public class ROW_NUMBER: ExprCalculationBuildIn
    {
        public      readonly    Node_OVER                           n_Over;

        public      override    DataModel.ValueFlags                ValueFlags          => DataModel.ValueFlags.Function;
        public      override    DataModel.ISqlType                  SqlType             => DataModel.SqlTypeNative.BigInt;

        internal                                                    ROW_NUMBER(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            ParseToken(reader, Core.TokenID.RrBracket);
            AddChild(n_Over = new Node_OVER(reader));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Over.TranspileNode(context);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
