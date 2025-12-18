using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/string-agg-transact-sql
    public class STRING_AGG: ExprCalculationBuildIn
    {
        public      readonly    IExprNode                           n_Expression;
        public      readonly    IExprNode                           n_Separator;
        public      readonly    Node_WITHIN_GROUP_ORDER_BY          n_WithinGroupOrderBy;

        public      override    DataModel.ValueFlags                ValueFlags          => _valueFlags;
        public      override    DataModel.ISqlType                  SqlType             => _sqlType;

        private                 DataModel.ValueFlags                _valueFlags;
        private                 DataModel.ISqlType                  _sqlType;

        internal                                                    STRING_AGG(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            n_Expression = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Separator = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);

            if (Node_WITHIN_GROUP_ORDER_BY.CanParse(reader)) {
                AddChild(n_WithinGroupOrderBy = new Node_WITHIN_GROUP_ORDER_BY(reader));
            }
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Expression.TranspileNode(context);
                n_Separator.TranspileNode(context);
                n_WithinGroupOrderBy?.TranspileNode(context);

                _valueFlags = LogicStatic.FunctionValueFlags(n_Expression.ValueFlags) & (~DataModel.ValueFlags.Nullable) | DataModel.ValueFlags.Aggregaat;
                _sqlType    = null;

                if (_valueFlags.isValid()) {
                    Validate.Value(n_Expression);
                    //Validate.ConstString(n_Separator);

                    var sqlType = n_Expression.SqlType;
                    if (sqlType != null && !(sqlType is DataModel.SqlTypeAny)) {
                        switch(sqlType.NativeType.SystemType) {
                        case DataModel.SystemType.Char:     _sqlType = DataModel.SqlTypeNative.VarChar_8000;    break;
                        case DataModel.SystemType.NChar:    _sqlType = DataModel.SqlTypeNative.NVarChar_4000;   break;
                        case DataModel.SystemType.VarChar:  _sqlType = DataModel.SqlTypeNative.VarChar_MAX;     break;
                        case DataModel.SystemType.NVarChar: _sqlType = DataModel.SqlTypeNative.NVarChar_MAX;    break;
                        default:
                            throw new TranspileException(n_Expression, "Not a string value.");
                        }
                    }
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
