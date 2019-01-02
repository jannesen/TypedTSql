using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://msdn.microsoft.com/en-us/library/ms187928.aspx
    // Expression_CAST
    //      : CAST '(' expression AS data_type ')'
    public class CAST: ExprCalculationBuildIn
    {
        public      readonly    IExprNode                   n_Expression;
        public      readonly    Node_Datatype               n_Datatype;

        public      override    DataModel.ValueFlags        ValueFlags          { get { return _valueFlags;                     } }
        public      override    DataModel.ISqlType          SqlType             { get { return n_Datatype.SqlType;              } }
        public      override    string                      CollationName       { get { return n_Expression.CollationName;      } }
        public      override    bool                        NoBracketsNeeded    { get { return n_Expression.NoBracketsNeeded;   } }

        private                 DataModel.ValueFlags        _valueFlags;

        internal                                            CAST(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Expression = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.AS);
            n_Datatype = AddChild(new Node_Datatype(reader, defaultLength:true));
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression.TranspileNode(context);
                n_Datatype.TranspileNode(context);

                _valueFlags = LogicStatic.ComputedValueFlags(n_Expression.ValueFlags) | DataModel.ValueFlags.Cast;

                if (n_Datatype.SqlType == null)
                    _valueFlags |= DataModel.ValueFlags.Error;

                if (_valueFlags.isValid()) {
                    Validate.ValueOrNull(n_Expression);
                    Validate.CastConvert(n_Datatype, n_Expression);
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
