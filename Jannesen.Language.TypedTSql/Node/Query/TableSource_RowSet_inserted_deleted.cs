using System;

namespace Jannesen.Language.TypedTSql.Node
{
    // Data_TableSource_alias_object ::=
    //      Objectname [ [AS] aliasname ] [ WITH ( tablehints ) ]
    public class TableSource_RowSet_inserted_deleted: TableSource_RowSet_alias
    {
        public      readonly    Core.Token                      n_Name;
        public      override    DataModel.IColumnList           ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList           _t_ColumnList;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("INSERTED", "DELETED");
        }
        public                                                  TableSource_RowSet_inserted_deleted(Core.ParserReader reader, bool allowAlias): base(allowAlias)
        {
            n_Name = ParseToken(reader);
            ParseTableAlias(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            try {
                _t_ColumnList = _getColumnList(context);
            }
            catch(Exception err) {
                context.AddError(n_Name, err);
            }

            TranspileRowSet(context);
        }

        private                 DataModel.IColumnList           _getColumnList(Transpile.Context context)
        {
            if (!(context.DeclarationEntity is Declaration_TRIGGER declarationTrigger))
                throw new ErrorException("Not in a trigger.");

            var columnList = (declarationTrigger.n_Table.Entity as DataModel.EntityObjectTable)?.Columns;
            if (columnList == null)
                throw new ErrorException("Can't get columns from trigger table.");

            return columnList;
        }
    }
}
