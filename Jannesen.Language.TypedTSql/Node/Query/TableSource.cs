using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms177634.aspx
    //  Data_TableSource
    //      : Data_TableSource_alias { Data_TableSource_join } [0..n]
    public class TableSource: Core.AstParseNode
    {
        public      readonly    TableSource_RowSet[]        n_RowSets;

        public                                              TableSource(Core.ParserReader reader)
        {
            var rowsets = new List<TableSource_RowSet>();

            rowsets.Add(AddChild(TableSource_RowSet_alias.Parse(reader)));

            while (TableSource_RowSet_join.CanParse(reader))
                rowsets.Add(AddChild(new TableSource_RowSet_join(reader)));

            n_RowSets = rowsets.ToArray();
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            var    rowNullable = new bool[n_RowSets.Length];

            for (int i = 0 ; i < n_RowSets.Length ; ++i) {
                switch(n_RowSets[i].n_JoinType) {
                case DataModel.JoinType.INNER:
                    break;

                case DataModel.JoinType.LEFT_OUTER:
                case DataModel.JoinType.FULL_OUTER:
                case DataModel.JoinType.CROSS_JOIN:
                case DataModel.JoinType.CROSS_APPLY:
                case DataModel.JoinType.OUTER_APPLY:
                    rowNullable[i] = true;
                    break;

                case DataModel.JoinType.RIGHT_OUTER:
                    for (int j = 0 ; j < i ; ++j) {
                        rowNullable[i] = true;
                    }
                    break;
                }
            }

            for (int i = 0; i < n_RowSets.Length ; ++i) {
                var rowset = n_RowSets[i];

                try {
                    rowset.TranspileNode(context);
                    rowset.TranspileRowSet(context, rowNullable[i]);

                    if (rowset.n_Alias == null && n_RowSets.Length > 1) {
                        context.AddError(rowset, "Unnamed rowset is not allowed.");
                    }
                }
                catch(Exception err) {
                    context.AddError(rowset, err);
                }
            }
        }

        public                  TableSource_RowSet          FindByName(string name)
        {
            foreach (var rowSet in n_RowSets) {
                if (rowSet.n_Alias?.ValueString == name) {
                    return rowSet;
                }
            }

            return null;
        }
    }
}
