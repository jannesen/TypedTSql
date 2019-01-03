using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/string-split-transact-sql
    public class STRING_SPLIT: TableSource_RowSetBuildIn
    {
        public      readonly    Node.IExprNode              n_String;
        public      readonly    Node.IExprNode              n_Seprator;
        public      override    DataModel.IColumnList       ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList       _t_ColumnList;

        internal                                            STRING_SPLIT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader, bool allowAlias): base(declaration, reader, allowAlias)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_String = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_Seprator = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.RrBracket);

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_String.TranspileNode(context);
            n_Seprator.TranspileNode(context);

            try {
                Validate.ValueString(n_String);
                Validate.ConstString(n_Seprator);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }

            var columnList = new DataModel.ColumnList(1);
            columnList.Add(new DataModel.ColumnNative("value", n_String.SqlType, n_String.CollationName));
            _t_ColumnList = columnList;

            TranspileRowSet(context);
        }
    }
}
