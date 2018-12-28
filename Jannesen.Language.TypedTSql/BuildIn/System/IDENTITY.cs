using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class IDENTITY: ExprCalculationBuildIn
    {
        public      readonly    Node_Datatype               n_Datatype;
        public      readonly    IExprNode                   n_Seed;
        public      readonly    IExprNode                   n_Increment;

        public      override    DataModel.ValueFlags        ValueFlags      { get { return _valueFlags;         } }
        public      override    DataModel.ISqlType          SqlType         { get { return n_Datatype.SqlType;  } }

        private                 DataModel.ValueFlags        _valueFlags;

        internal                                            IDENTITY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Datatype = AddChild(new Node_Datatype(reader));

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                n_Seed = ParseSimpleExpression(reader, constValue:true);
                ParseToken(reader, Core.TokenID.Comma);
                n_Increment = ParseSimpleExpression(reader, constValue:true);
            }

            ParseToken(reader, Core.TokenID.RrBracket);

        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Datatype.TranspileNode(context);
                n_Seed?.TranspileNode(context);
                n_Increment?.TranspileNode(context);

                if (n_Datatype.SqlType != null) {
                    if (n_Seed != null)         Validate.ValueInt(n_Seed);
                    if (n_Increment != null)    Validate.ValueInt(n_Increment);
                }
                else
                    _valueFlags |= DataModel.ValueFlags.Error;
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                context.AddError(this, err);
            }
        }
        public      override    bool                        ValidateConst(DataModel.ISqlType sqlType)
        {
            return false;
        }
    }
}
