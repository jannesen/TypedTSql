using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/string-split-transact-sql
    public class GENERATE_SERIES: TableSource_RowSetBuildIn
    {
        public      readonly    Node.IExprNode                      n_Start;
        public      readonly    Node.IExprNode                      n_Stop;
        public      readonly    Node.IExprNode                      n_Step;
        public      override    DataModel.IColumnList               ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList               _t_ColumnList;

        internal                                                    GENERATE_SERIES(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Start = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Stop = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                n_Step = ParseExpression(reader);
            }

            ParseToken(reader, Core.TokenID.RrBracket);

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Start.TranspileNode(context);
            n_Stop.TranspileNode(context);
            n_Step?.TranspileNode(context);

            DataModel.SqlTypeNative sqlType = null;

            try {
                sqlType = n_Start.SqlType.NativeType;
                Validate.ValueIntNumber(n_Start);
                Validate.ValueIntNumber(n_Stop);
                Validate.ValueIntNumber(n_Step);

                if (n_Stop.SqlType.NativeType != sqlType) {
                    if ((sqlType.SystemType                   == DataModel.SystemType.Numeric ||
                         sqlType.SystemType                   == DataModel.SystemType.Decimal) &&
                        n_Stop.SqlType.NativeType.SystemType == sqlType.SystemType &&
                        n_Stop.SqlType.NativeType.Scale      == sqlType.Scale) {
                        sqlType = new DataModel.SqlTypeNative(sqlType.SystemType,
                                                              precision:Math.Max(sqlType.Precision, n_Stop.SqlType.NativeType.Precision),
                                                              scale:sqlType.Scale);
                    }
                    else {
                        context.AddError(n_Stop, "Invalid type expect " + sqlType.ToString() + ".");
                    }
                }

                if (n_Step != null && n_Step.SqlType.NativeType != sqlType) {
                    if (!((sqlType.SystemType                   == DataModel.SystemType.Numeric ||
                           sqlType.SystemType                   == DataModel.SystemType.Decimal) &&
                          n_Stop.SqlType.NativeType.SystemType == sqlType.SystemType &&
                          n_Stop.SqlType.NativeType.Scale      == sqlType.Scale)) {
                        context.AddError(n_Step, "Invalid type expect " + sqlType.ToString() + ".");
                    }
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }

            
            _t_ColumnList = new DataModel.ColumnList(1) {
                                    new DataModel.ColumnNative("value",  (DataModel.ISqlType)sqlType ?? new DataModel.SqlTypeAny())
                                };
        }
    }
}
