using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/datepart-transact-sql
    public class DATETRUNC: ExprCalculationBuildIn
    {
        public      readonly    Core.Token                  n_DatePart;
        public      readonly    IExprNode                   n_Date;

        public      override    DataModel.ValueFlags        ValueFlags          => _valueFlags;
        public      override    DataModel.ISqlType          SqlType             => _sqlType;

        private                 DataModel.ValueFlags        _valueFlags;
        private                 DataModel.ISqlType          _sqlType;

        internal                                            DATETRUNC(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            n_DatePart = ParseToken(reader, Core.TokenID.Name);
            ParseToken(reader, Core.TokenID.Comma);
            n_Date = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Date.TranspileNode(context);

                _valueFlags = LogicStatic.FunctionValueFlags(n_Date.ValueFlags);
                _sqlType    = n_Date.SqlType;

                if (_valueFlags.isValid()) {
                    var mode = Validate.DatePart(n_DatePart);
                    _sqlType = Validate.ValueDateTime(n_Date, mode);
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                _sqlType    = new DataModel.SqlTypeAny();
                context.AddError(this, err);
            }
        }
    }
}
