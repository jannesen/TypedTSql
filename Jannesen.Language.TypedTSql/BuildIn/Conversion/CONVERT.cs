using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // Expression_CONVERT:
    //      CONVERT '(' Datatype ',' Expression (',' Integer)? ')'
    public class CONVERT: ExprCalculationBuildIn
    {
        public      readonly    IExprNode                   n_Expression;
        public      readonly    Node_Datatype               n_Datatype;
        public      readonly    IExprNode                   n_Style;

        public      override    DataModel.ValueFlags        ValueFlags      { get { return _valueFlags;                 } }
        public      override    DataModel.ISqlType          SqlType         { get { return n_Datatype.SqlType;          } }
        public      override    string                      CollationName   { get { return n_Expression.CollationName;  } }

        private                 DataModel.ValueFlags        _valueFlags;

        internal                                            CONVERT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Datatype = AddChild(new Node_Datatype(reader, defaultLength:true));
            ParseToken(reader, Core.TokenID.Comma);
            n_Expression = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                n_Style = ParseExpression(reader);
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression.TranspileNode(context);
                n_Datatype.TranspileNode(context);
                n_Style?.TranspileNode(context);

                _valueFlags = LogicStatic.ComputedValueFlags(n_Expression.ValueFlags) | DataModel.ValueFlags.Cast;

                if (n_Datatype.SqlType == null)
                    _valueFlags |= DataModel.ValueFlags.Error;

                if (_valueFlags.isValid()) {
                    Validate.ValueOrNull(n_Expression);
                    Validate.CastConvert(n_Datatype, n_Expression, n_Style);
                }
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
        public      override    void                        Emit(EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (node == n_Datatype) {
                    n_Datatype.EmitNative(emitWriter);
                }
                else
                    node.Emit(emitWriter);
            }
        }
    }
}
