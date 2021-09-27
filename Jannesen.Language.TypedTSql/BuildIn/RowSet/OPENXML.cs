using System;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    //https://msdn.microsoft.com/en-us/library/ms190312.aspx
    public class OPENXML: TableSource_RowSetBuildIn
    {
        public      readonly    Node.IExprNode                      n_VariableIDoc;
        public      readonly    Node.IExprNode                      n_RowPattern;
        public      readonly    Core.Token                          n_Flags;
        public      readonly    TableSource_WithDeclaration         n_With;
        public      override    DataModel.IColumnList               ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList               _t_ColumnList;

        internal                                                    OPENXML(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader, bool allowAlias): base(declaration, reader, allowAlias)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_VariableIDoc = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.Comma);
            n_RowPattern = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Comma) != null)
                n_Flags = ParseInteger(reader);

            ParseToken(reader, Core.TokenID.RrBracket);

            //!!TODO Support WITH EDGE
            n_With = AddChild(new TableSource_WithDeclaration(reader, TableSourceWithType.Xml));

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            n_VariableIDoc.TranspileNode(context);
            n_RowPattern.TranspileNode(context);
            n_With.TranspileNode(context);

            try {
                var flags = n_VariableIDoc.ValueFlags;

                if ((flags & DataModel.ValueFlags.Error) == 0) {
                    if ((flags & (DataModel.ValueFlags.SourceFlags|DataModel.ValueFlags.BooleanExpression|DataModel.ValueFlags.ValueExpression)) != DataModel.ValueFlags.Variable)
                        throw new TranspileException(n_VariableIDoc, "Expect variable.");

                    var sqlType = n_VariableIDoc.SqlType;
                    if (!(sqlType == null || sqlType is DataModel.SqlTypeAny || sqlType.NativeType.SystemType == DataModel.SystemType.Int))
                        throw new TranspileException(n_VariableIDoc, "Expect integer variable.");
                }

                Validate.ConstString(n_RowPattern);
                _t_ColumnList = n_With.getColumnList(context, n_VariableIDoc, n_RowPattern);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }

            TranspileRowSet(context);
        }
    }
}
