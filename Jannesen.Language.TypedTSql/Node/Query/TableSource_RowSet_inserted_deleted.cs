using System;

namespace Jannesen.Language.TypedTSql.Node
{
    // Data_TableSource_alias_object ::=
    //      Objectname [ [AS] aliasname ] [ WITH ( tablehints ) ]
    public class TableSource_RowSet_inserted_deleted: TableSource_RowSet_alias
    {
        public      readonly    Core.Token                      n_Name;
        public      override    DataModel.IColumnList           ColumnList      { get { return _t_table.Columns ; } }

        private                 DataModel.EntityObjectTable     _t_table;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("INSERTED", "DELETED");
        }
        public                                                  TableSource_RowSet_inserted_deleted(Core.ParserReader reader)
        {
            n_Name = ParseToken(reader);
            ParseTableAlias(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _t_table      = null;

            try {
                if (!(context.DeclarationEntity is Declaration_TRIGGER declarationTrigger))
                    throw new ErrorException("Not in a trigger.");

                if ((_t_table = (declarationTrigger.n_Table.Entity as DataModel.EntityObjectTable)) == null)
                    throw new ErrorException("Can't find trigger table.");
            }
            catch(Exception err) {
                context.AddError(n_Name, err);
            }

            TranspileRowSet(context);
        }
    }
}
