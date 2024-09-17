using System;

namespace Jannesen.Language.TypedTSql.Node
{
    // Data_TableSource_alias_object ::=
    //      Objectname [ [AS] aliasname ] [ WITH ( tablehints ) ]
    public class TableSource_RowSet_object: TableSource_RowSet_alias
    {
        public      readonly    Node_EntityNameReference        n_Object;
        public      readonly    Node_TableHints                 n_With;
        public      override    DataModel.IColumnList           ColumnList          => _t_ColumnList;
        public      override    DataModel.ISymbol               t_Source            => n_Object.Entity;
        private                 DataModel.IColumnList           _t_ColumnList;

        public                                                  TableSource_RowSet_object(Core.ParserReader reader)
        {
            n_Object = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.TableOrView, DataModel.SymbolUsageFlags.Select));

            ParseTableAlias(reader);

            if (reader.CurrentToken.isToken(Core.TokenID.WITH)) {
                n_With = AddChild(new Node_TableHints(reader));
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            n_Object.TranspileNode(context);
            n_With?.TranspileNode(context);

            _t_ColumnList = n_Object.Columns ?? new DataModel.ColumnListErrorStub();
            TranspileRowSet(context);

            if (n_With != null)
                n_With.CheckIndexes(context, n_Object.Entity);
        }
        public      override    bool                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            n_Object.SetUsage(usage);
            return true;
        }
    }
}
