using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/string-split-transact-sql
    public class STRING_SPLIT: TableSource_RowSetBuildIn
    {
        public      readonly    Node.IExprNode                      n_String;
        public      readonly    Node.IExprNode                      n_Seprator;
        public      readonly    Node.IExprNode                      n_EnableOrdinal;
        public      override    DataModel.IColumnList               ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList               _t_ColumnList;

        internal                                                    STRING_SPLIT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_String = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Seprator = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                n_EnableOrdinal = ParseExpression(reader);
            }

            ParseToken(reader, Core.TokenID.RrBracket);

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_String.TranspileNode(context);
            n_Seprator.TranspileNode(context);
            n_EnableOrdinal?.TranspileNode(context);

            object ordinal = null;

            try {
                Validate.ValueString(n_String);
                Validate.ConstString(n_Seprator);
                ordinal = Validate.ConstBit(n_EnableOrdinal);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }

            
            _t_ColumnList = (ordinal is int i && i == 1)
                                ? new DataModel.ColumnList(2) {
                                          new DataModel.ColumnNative("value",   n_String.SqlType,            collationName:n_String.CollationName),
                                          new DataModel.ColumnNative("ordinal", DataModel.SqlTypeNative.Int, nullable:false)
                                      }
                                : new DataModel.ColumnList(1) {
                                          new DataModel.ColumnNative("value",   n_String.SqlType,            collationName:n_String.CollationName)
                                      };
        }
    }
}
