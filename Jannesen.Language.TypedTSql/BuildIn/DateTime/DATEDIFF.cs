using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://msdn.microsoft.com/en-us/library/ms189794.aspx
    public class DATEDIFF: ExprCalculationBuildIn
    {
        public      readonly    Core.Token                  n_DatePart;
        public      readonly    IExprNode                   n_StartDate;
        public      readonly    IExprNode                   n_EndDate;

        public      override    DataModel.ValueFlags        ValueFlags          { get { return _valueFlags;                  } }
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.Int;  } }

        private                 DataModel.ValueFlags        _valueFlags;

        internal                                            DATEDIFF(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            n_DatePart = ParseToken(reader, Core.TokenID.Name);
            ParseToken(reader, Core.TokenID.Comma);
            n_StartDate = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_EndDate = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_StartDate.TranspileNode(context);
                n_EndDate.TranspileNode(context);

                _valueFlags = LogicStatic.FunctionValueFlags(n_StartDate.ValueFlags | n_EndDate.ValueFlags);

                if (_valueFlags.isValid()) {
                    var mode = Validate.DatePart(n_DatePart);
                    var sqlType1 = Validate.ValueDateTime(n_StartDate, mode);
                    var sqlType2 = Validate.ValueDateTime(n_EndDate, mode);

                    if (!_datediffPossible(sqlType1, sqlType2))
                        context.AddError(this, "DATEDIFF(" + sqlType1 + "," + sqlType2 + ") not possible.");
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                context.AddError(this, err);
            }
        }

        private                     bool                        _datediffPossible(DataModel.ISqlType sqlType1, DataModel.ISqlType sqlType2)
        {
            if (sqlType1 is DataModel.SqlTypeAny || sqlType2 is DataModel.SqlTypeAny)
                return true;

            switch(sqlType1.NativeType.SystemType) {
            case DataModel.SystemType.Time:
                switch(sqlType2.NativeType.SystemType) {
                case DataModel.SystemType.Time:             return true;
                default:                                    return false;
                }

            case DataModel.SystemType.Date:
                switch(sqlType2.NativeType.SystemType) {
                case DataModel.SystemType.Date:             return true;
                default:                                    return false;
                }

            case DataModel.SystemType.SmallDateTime:
            case DataModel.SystemType.DateTime:
            case DataModel.SystemType.DateTime2:
                switch(sqlType2.NativeType.SystemType) {
                case DataModel.SystemType.SmallDateTime:    return true;
                case DataModel.SystemType.DateTime:         return true;
                case DataModel.SystemType.DateTime2:        return true;
                default:                                    return false;
                }

            default:
                return false;
            }
        }
    }
}
