using System;
using Jannesen.Language.TypedTSql.Core;

namespace Jannesen.Language.TypedTSql.Node
{
    // Data_TableSource_alias_subquery  ::=
    //      ( Data_TableSource ) alias
    public class TableSource_RowSet_subquery: TableSource_RowSet_alias
    {
        public      readonly    Query_Select                    n_Select;
        public      override    DataModel.IColumnList           ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList           _t_ColumnList;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.LrBracket);
        }
        public                                                  TableSource_RowSet_subquery(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.TableSourceSubquery));
            ParseToken(reader, Core.TokenID.RrBracket);

            ParseTableAlias(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            var contextQuery = new Transpile.ContextSubquery(context);

            _t_ColumnList = null;

            n_Select.TranspileNode(contextQuery);

            _t_ColumnList = _transpileResult(context);
        }
        public      override    void                            Emit(EmitWriter emitWriter)
        {
            foreach(var n in Children) {
                n.Emit(emitWriter);

                if (n is Core.Token token && token.ID == TokenID.RrBracket) {
                    if (n_Alias == null)
                        emitWriter.WriteText(" [$dummy]");
                }
            }
        }

        public                  DataModel.IColumnList           _transpileResult(Transpile.Context context)
        {
            try {
                if (n_Select.Resultset != null)
                    return n_Select.Resultset.GetUniqueNamedList();
            }
            catch(Exception err) {
                context.AddError(n_Select, err);
            }

            return new DataModel.ColumnListErrorStub();
        }
    }
}
