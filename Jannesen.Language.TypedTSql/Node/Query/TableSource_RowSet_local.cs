using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //Data_TableSource_alias_local  ::= @variable           [ [ AS ] table_alias ]
    public class TableSource_RowSet_local: TableSource_RowSet_alias
    {
        public      readonly    Node_TableVariable              n_Variable;
        public      override    DataModel.IColumnList           ColumnList          { get { return _t_ColumnList;               } }
        public      override    DataModel.ISymbol               t_Source            { get { return n_Variable.Variable.Symbol;  } }

        private                 DataModel.IColumnList           _t_ColumnList;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.LocalName);
        }
        public                                                  TableSource_RowSet_local(Core.ParserReader reader)
        {
            n_Variable = AddChild(new Node_TableVariable(reader, DataModel.SymbolUsageFlags.Select));

            ParseTableAlias(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            n_Variable.TranspileNode(context);
            n_Variable.Variable?.setUsed();

            _t_ColumnList = n_Variable.Columns ?? new DataModel.ColumnListErrorStub();
        }
        public      override    bool                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            n_Variable.SetUsage(usage);
            return true;
        }
    }
}
