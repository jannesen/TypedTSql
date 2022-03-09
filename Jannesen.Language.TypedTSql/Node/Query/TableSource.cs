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
            var rowsets = new List<TableSource_RowSet>() { {  AddChild(TableSource_RowSet_alias.Parse(reader, true)) } };

            while (TableSource_RowSet_join.CanParse(reader))
                rowsets.Add(AddChild(new TableSource_RowSet_join(reader)));

            n_RowSets = rowsets.ToArray();
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            foreach(var n in n_RowSets) {
                try {
                    n.TranspileNode(context);

                    if (n.n_Alias == null && n_RowSets.Length > 1)
                        context.AddError(n, "Unnamed rowset is not allowed.");
                }
                catch(Exception err) {
                    context.AddError(n, err);
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
