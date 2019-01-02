using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //      < table_constraint > ::=
    //      [ CONSTRAINT constraint_name ]
    //      {
    //          { PRIMARY KEY | UNIQUE }
    //              [ CLUSTERED | NONCLUSTERED ]
    //              (column [ ASC | DESC ] [ ,...n ] )
    //              [
    //                 |WITH ( <index_option> [ , ...n ] )
    //              ]
    //              [ ON { partition_scheme_name (partition_column_name)
    //                  | filegroup | "default" } ]
    //          | FOREIGN KEY
    //              ( column [ ,...n ] )
    //              REFERENCES referenced_table_name [ ( ref_column [ ,...n ] ) ]
    //              [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
    //              [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
    //              [ NOT FOR REPLICATION ]
    //          | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
    public class Table_ConstraintIndex: Table_Constraint
    {
        public class Column: Core.AstParseNode
        {
            public      readonly    Core.TokenWithSymbol        n_Column;
            public      readonly    bool                        n_Descending;
            public                  DataModel.IndexColumn       t_IndexColumn               { get; private set; }

            public                                              Column(Core.ParserReader reader)
            {
                n_Column = ParseName(reader);

                Core.Token token = ParseOptionalToken(reader, Core.TokenID.ASC, Core.TokenID.DESC);
                if (token != null)
                    n_Descending = token.ID == Core.TokenID.DESC;
            }

            public      override    void                        TranspileNode(Transpile.Context context)
            {
                var     column = context.ColumnList?.FindColumn(n_Column.ValueString, out bool ambiguous);

                if (column != null) {
                    n_Column.SetSymbol(column);
                    context.CaseWarning(n_Column, column.Name);
                    t_IndexColumn = new DataModel.IndexColumn(column, n_Descending);
                }
                else
                    context.AddError(n_Column, "Unknown column in table.");
            }
        }

        public      readonly    DataModel.IndexFlags        n_Flags;
        public      readonly    Column[]                    n_Columns;
        public                  DataModel.Index             t_Index                 { get; private set; }

        public      static      bool                        CanParse(Core.ParserReader reader, TableType type)
        {
            switch(type) {
            case TableType.Temp:
                return reader.CurrentToken.isToken(Core.TokenID.CONSTRAINT) &&
                       reader.Peek(3)[2].isToken(Core.TokenID.PRIMARY, Core.TokenID.UNIQUE);

            case TableType.Variable:
            case TableType.Type:
                return reader.CurrentToken.isToken(Core.TokenID.PRIMARY, Core.TokenID.UNIQUE);
            }

            return false;
        }
        public                                              Table_ConstraintIndex(Core.ParserReader reader, TableType type): base (reader, type)
        {
            switch(reader.CurrentToken.validateToken(Core.TokenID.PRIMARY, Core.TokenID.UNIQUE)) {
            case Core.TokenID.PRIMARY:
                ParseToken(reader, Core.TokenID.PRIMARY);
                ParseToken(reader, Core.TokenID.KEY);
                n_Flags |= DataModel.IndexFlags.PrimaryKey;
                break;

            case Core.TokenID.UNIQUE:
                ParseToken(reader, Core.TokenID.UNIQUE);
                n_Flags |= DataModel.IndexFlags.Unique;
                break;
            }

            ParseToken(reader, Core.TokenID.LrBracket);

            var columns = new List<Column>();

            do {
                columns.Add(AddChild(new Column(reader)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Columns = columns.ToArray();
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            n_Columns.TranspileNodes(context);
            base.TranspileNode(context);

            var columns = new DataModel.IndexColumn[n_Columns.Length];

            for (int i = 0 ; i < n_Columns.Length ; ++i)
                columns[i] = n_Columns[i].t_IndexColumn;

            var columnList = context.RowSets[0].Columns;

            if (n_Name != null) {
                t_Index = new DataModel.Index(n_Flags,
                                              n_Name.ValueString,
                                              columns,
                                              declaration:n_Name);
                if (t_Index != null)
                    n_Name.SetSymbol(t_Index);
            }
            else {
                t_Index = new DataModel.Index(n_Flags, "", columns);
            }
        }
    }
}
