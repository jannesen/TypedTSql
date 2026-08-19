using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum TableType
    {
        Temp        = 1,
        Variable,
        Type
    }

    // https://msdn.microsoft.com/en-us/library/ms174979.aspx
    public class Table: Core.AstParseNode
    {
        public      readonly    TableType                       Type;
        public      readonly    Table_Column[]                  n_Columns;
        public      readonly    Table_Constraint[]              n_Constraints;
        public      readonly    Table_Index[]                   n_Indexes;

        public                  DataModel.ColumnList            Columns                 { get; private set; }
        public                  DataModel.IndexList             Indexes                 { get; private set; }

        public                                                  Table(Core.ParserReader reader, TableType type)
        {
            Type = type;

            var columns     = new List<Table_Column>();
            var constraints = new List<Table_Constraint>();
            var indexes     = new List<Table_Index>();

            ParseToken(reader, Core.TokenID.LrBracket);

            do {
                if (reader.CurrentToken.isNameOrQuotedName) {
                    if (Table_ColumnComputed.CanParse(reader, type))
                        _addChild(ref columns, new Table_ColumnComputed(reader, type));
                    else
                        _addChild(ref columns, new Table_ColumnData(reader, type));
                }
                else {
                    if (Table_Constraint.CanParse(reader, type))
                        _addChild(ref constraints, new Table_Constraint(reader, type));
                    else
                    if (Table_Index.CanParse(reader, type))
                        _addChild(ref indexes, new Table_Index(reader, type));
                    else
                        throw new ParseException(reader.CurrentToken, "Unexpected " + reader.CurrentToken.ToString() + ".");
                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Columns     = columns.ToArray();
            n_Constraints = constraints.ToArray();
            n_Indexes     = indexes.ToArray();
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Columns?.TranspileNodes(context);

            this.Columns = null;
            this.Indexes = null;

            {
                var columns = new DataModel.ColumnList(n_Columns.Length);

                foreach (var column in n_Columns) {
                    if (column.Column != null) {
                        if (!columns.TryAdd(column.Column))
                            context.AddError(column.n_Name, "Column [" + column.n_Name.ValueString + "] already declared.");
                    }
                }

                this.Columns = columns;
            }

            if (n_Constraints != null || n_Indexes != null) {
                var contextRowSet = new Transpile.ContextRowSets(context, Columns);
                n_Constraints.TranspileNodes(contextRowSet);
                n_Indexes.TranspileNodes(contextRowSet);

                var indexes = new DataModel.IndexList(4);

                foreach (var indexNode in n_Indexes) {
                    if (indexNode.t_Index != null) {
                        if (!indexes.TryAdd(indexNode.t_Index)) {
                            context.AddError(indexNode, "Index already defined.");
                        }
                    }
                }

                indexes.OptimizeSize();
                this.Indexes = indexes;
            }
        }

        private                 void                            _addChild<T>(ref List<T> list, T child) where T: Core.AstParseNode
        {
            if (list == null)
                list = new List<T>();

            list.Add(child);
            AddChild(child);
        }
    }
}
