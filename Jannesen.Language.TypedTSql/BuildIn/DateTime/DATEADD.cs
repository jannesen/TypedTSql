using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://msdn.microsoft.com/en-us/library/ms186819.aspx
    public class DATEADD: ExprCalculationBuildIn
    {
        public      readonly    Core.Token                  n_DatePart;
        public      readonly    IExprNode                   n_Number;
        public      readonly    IExprNode                   n_Date;

        public      override    DataModel.ValueFlags        ValueFlags          { get { return _valueFlags;  } }
        public      override    DataModel.ISqlType          SqlType             { get { return _sqlType;     } }

        private                 DataModel.ValueFlags        _valueFlags;
        private                 DataModel.ISqlType          _sqlType;

        internal                                            DATEADD(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            n_DatePart = ParseToken(reader, Core.TokenID.Name);
            ParseToken(reader, Core.TokenID.Comma);
            n_Number = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Date = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            _sqlType    = null;

            try {
                n_Number.TranspileNode(context);
                n_Date.TranspileNode(context);

                _valueFlags = LogicStatic.FunctionValueFlags(n_Number.ValueFlags | n_Date.ValueFlags);

                if (_valueFlags.isValid()) {
                    var mode = Validate.DatePart(n_DatePart);
                    _sqlType = Validate.ValueDateTime(n_Date, mode);

                    Validate.ValueInt(n_Number);
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
                _valueFlags = DataModel.ValueFlags.Error;
                _sqlType    = null;
            }
        }
    }
}
