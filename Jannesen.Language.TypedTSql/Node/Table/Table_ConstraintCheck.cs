using System;

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
    public class Table_ConstraintCheck: Table_Constraint
    {
        public      readonly    IExprNode                   n_Expression;

        public      static      bool                        CanParse(Core.ParserReader reader, TableType type)
        {
            switch(type) {
            case TableType.Temp:
                return reader.CurrentToken.isToken(Core.TokenID.CONSTRAINT) &&
                       reader.Peek(3)[2].isToken(Core.TokenID.CHECK);

            case TableType.Variable:
            case TableType.Type:
                return reader.CurrentToken.isToken(Core.TokenID.CHECK);
            }

            return false;
        }
        public                                              Table_ConstraintCheck(Core.ParserReader reader, TableType type): base(reader, type)
        {
            ParseToken(reader, Core.TokenID.CHECK);
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Expression = ParseExpression(reader);
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            base.TranspileNode(context);

            n_Expression.TranspileNode(context);
        }
    }
}
