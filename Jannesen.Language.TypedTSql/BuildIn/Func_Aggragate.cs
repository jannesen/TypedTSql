using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn
{
    public abstract class Func_Aggragate: ExprCalculationBuildIn
    {
        public      readonly    bool                                n_Distinct;
        public      readonly    IExprNode                           n_Expression;

        public      override    DataModel.ValueFlags                ValueFlags          { get { return _ValueFlags; } }
        public      override    DataModel.ISqlType                  SqlType             { get { return _sqlType;    } }

        private                 DataModel.ValueFlags                _ValueFlags;
        private                 DataModel.ISqlType                  _sqlType;

        internal                                                    Func_Aggragate(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            var alldistinct = ParseOptionalToken(reader, Core.TokenID.ALL, Core.TokenID.DISTINCT);
            n_Distinct = (alldistinct != null) ? alldistinct.isToken(Core.TokenID.DISTINCT) : false;

            n_Expression = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression.TranspileNode(context);

                _ValueFlags = LogicStatic.FunctionValueFlags(n_Expression.ValueFlags) | DataModel.ValueFlags.Aggregaat;

                if (_ValueFlags.isValid()) {
                    Validate.Value(n_Expression);
                    var sqlType = n_Expression.SqlType;

                    if (!(sqlType is DataModel.SqlTypeAny)) {
                        _sqlType    = TranspileReturnType(sqlType);

                        if (_sqlType == null)
                            throw new ErrorException(this.GetType().Name + "(" + n_Expression.SqlType.NativeType.ToString() + ") not possible.");

//                      if (_ValueFlags.isNullable())
//                          context.AddWarning(this, "Expression is nullable. This can give warnings during runtime execution.");
                    }
                    else
                        _sqlType    = sqlType;
                }
                else
                    _sqlType    = null;
            }
            catch(Exception err) {
                _ValueFlags = DataModel.ValueFlags.Error;
                _sqlType    = null;
                context.AddError(this, err);
            }
        }

        protected   abstract    DataModel.ISqlType                  TranspileReturnType(DataModel.ISqlType sqlType);
    }
}
