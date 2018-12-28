using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://msdn.microsoft.com/en-us/library/ms175997.aspx
    public class DATEPART: ExprCalculationBuildIn
    {
        public      readonly    Core.Token                  n_DatePart;
        public      readonly    IExprNode                   n_Date;

        public      override    DataModel.ValueFlags        ValueFlags          { get { return _valueFlags;                  } }
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.Int;  } }

        private                 DataModel.ValueFlags        _valueFlags;

        internal                                            DATEPART(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
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

                if (_valueFlags.isValid()) {
                    var mode = Validate.DatePart(n_DatePart);
                    Validate.ValueDateTime(n_Date, mode);
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                context.AddError(this, err);
            }
        }
    }
}
