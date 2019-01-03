using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    //      column_name AS computed_column_expression
    //      [ PERSISTED [ NOT NULL ] ]
    //      [
    //          [ CONSTRAINT constraint_name ]
    //          { PRIMARY KEY | UNIQUE }
    //              [ CLUSTERED | NONCLUSTERED ]
    //              [
    //                | WITH ( <index_option> [ , ...n ] )
    //              ]
    //              [ ON { partition_scheme_name ( partition_column_name )
    //              | filegroup | "default" } ]
    //
    //          | [ FOREIGN KEY ]
    //              REFERENCES referenced_table_name [ ( ref_column ) ]
    //              [ ON DELETE { NO ACTION | CASCADE } ]
    //              [ ON UPDATE { NO ACTION } ]
    //              [ NOT FOR REPLICATION ]
    //          | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
    //      ]
    public class Table_ColumnComputed: Table_Column
    {
        public      readonly    IExprNode               n_Expression;

        public      static      bool                    CanParse(Core.ParserReader reader, TableType type)
        {
            return reader.NextPeek().isToken(Core.TokenID.AS);
        }
        public                                          Table_ColumnComputed(Core.ParserReader reader, TableType type): base(reader)
        {
            ParseToken(reader, Core.TokenID.AS);
            //TODO: Expression context.
            n_Expression = ParseExpression(reader);
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            Column = null;

            try {
                n_Expression.TranspileNode(context);

                Column = new DataModel.ColumnDS(n_Name.ValueString,
                                                sqlType:     n_Expression.SqlType,
                                                flags:       DataModel.ValueFlags.Nullable,
                                                declaration: n_Name);
                n_Name.SetSymbol(Column);

                Validate.Value(n_Expression);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
