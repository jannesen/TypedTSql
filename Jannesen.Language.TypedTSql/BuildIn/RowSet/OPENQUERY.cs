using System;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn.RowSet
{
    //https://msdn.microsoft.com/en-us/library/ms188427.aspx
    public class OPENQUERY: TableSource_RowSetBuildIn
    {
        public      readonly    Core.TokenWithSymbol                n_LinkedServer;
        public      readonly    Core.Token                          n_Query;
        public      override    DataModel.IColumnList               ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList               _t_ColumnList;

        internal                                                    OPENQUERY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader, bool allowAlias): base(declaration, reader, allowAlias)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_LinkedServer   = Core.TokenWithSymbol.SetNoSymbol(ParseName(reader));
            ParseToken(reader, Core.TokenID.Comma);
            n_Query          = ParseToken(reader, Core.TokenID.String);
            ParseToken(reader, Core.TokenID.RrBracket);

            ParseTableAlias(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            TranspileRowSet(context);
            _t_ColumnList = new DataModel.ColumnListDynamic();
        }
    }
}
