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

        public                  DataModel.ColumnList            Columns                 { get; private set; }
        public                  DataModel.IndexList             Indexes                 { get; private set; }

        public                                                  Table(Core.ParserReader reader, TableType type)
        {
            Type = type;

            var columns    = new List<Table_Column>();
            var constraint = new List<Table_Constraint>();

            ParseToken(reader, Core.TokenID.LrBracket);

            do {
                if (reader.CurrentToken.isNameOrQuotedName) {
                    if (Table_ColumnComputed.CanParse(reader, type))
                        _addChild<Table_Column>(ref columns, new Table_ColumnComputed(reader, type));
                    else
                        _addChild<Table_Column>(ref columns, new Table_ColumnData(reader, type));
                }
                else {
                    if (Table_ConstraintCheck.CanParse(reader, type))
                        _addChild<Table_Constraint>(ref constraint, new Table_ConstraintCheck(reader, type));
                    else
                    if (Table_ConstraintIndex.CanParse(reader, type))
                        _addChild<Table_Constraint>(ref constraint, new Table_ConstraintIndex(reader, type));
                    else
                        throw new ParseException(reader.CurrentToken, "Unexpected " + reader.CurrentToken.ToString() + ".");
                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Columns     = columns.ToArray();
            n_Constraints = constraint.ToArray();
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

            if (n_Constraints != null) {
                var contextRowSet = new Transpile.ContextRowSets(context, Columns);
                n_Constraints.TranspileNodes(contextRowSet);

                var indexes = new DataModel.IndexList(4);

                foreach (var constraint in n_Constraints) {
                    if (constraint is Table_ConstraintIndex) {
                        var index = ((Table_ConstraintIndex)constraint).t_Index;

                        if (!indexes.TryAdd(index))
                            context.AddError(constraint, "Index [" + index.Name + "] already defined.");
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
