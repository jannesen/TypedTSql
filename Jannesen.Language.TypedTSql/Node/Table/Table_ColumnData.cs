using System;
using Jannesen.Language.TypedTSql.Core;

namespace Jannesen.Language.TypedTSql.Node
{
    //      column_name <data_type>
    //          [ FILESTREAM ]
    //          [ COLLATE collation_name ]
    //          [ SPARSE ]
    //          [ CONSTRAINT constraint_name ] DEFAULT constant_expression ]
    //          [ IDENTITY [ ( seed,increment ) ]
    //          [ NULL | NOT NULL ]
    //          [ ROWGUIDCOL ]
    //          [ ENCRYPTED WITH
    //              ( COLUMN_ENCRYPTION_KEY = key_name ,
    //                ENCRYPTION_TYPE = { DETERMINISTIC | RANDOMIZED } ,
    //                ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
    //              ) ]
    //          [ <column_constraint> [ ...n ] ]
    //
    //      <column_constraint> ::=
    //      [ CONSTRAINT constraint_name ]
    //      {     { PRIMARY KEY | UNIQUE }
    //              [ CLUSTERED | NONCLUSTERED ]
    //              [
    //                | WITH ( < index_option > [ , ...n ] )
    //              ]
    //        | [ FOREIGN KEY ]
    //              REFERENCES [ schema_name . ] referenced_table_name [ ( ref_column ) ]
    //              [ ON DELETE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
    //              [ ON UPDATE { NO ACTION | CASCADE | SET NULL | SET DEFAULT } ]
    //              [ NOT FOR REPLICATION ]
    //
    //        | CHECK [ NOT FOR REPLICATION ] ( logical_expression )
    //      }
    public class Table_ColumnData: Table_Column
    {
        public      readonly    TableType               TableType;
        public      readonly    Node_Datatype           n_Datatype;
        public      readonly    Core.Token              n_Collation;
        public      readonly    IExprNode               n_Default;
        public      readonly    Core.Token              n_Identity_Seed;
        public      readonly    Core.Token              n_Identity_Increment;
        public      readonly    DataModel.ValueFlags    n_ColumnFlags;

        public                                          Table_ColumnData(Core.ParserReader reader, TableType type): base(reader)
        {
            TableType = type;
            n_Datatype = AddChild(new Node_Datatype(reader));

            Core.Token optionToken;

            n_ColumnFlags = DataModel.ValueFlags.Nullable;

            while ((optionToken = ParseOptionalToken(reader, "FILESTREAM", "COLLATE", "DEFAULT", "IDENTITY", "PARSE", "NULL", "NOT", "ROWGUIDCOL", "PRIMARY")) != null) {
                switch(optionToken.Text.ToUpperInvariant()) {
                case "COLLATE":
                    n_Collation = ParseToken(reader, Core.TokenID.Name);
                    break;

                case "DEFAULT":
                    n_Default = ParseExpression(reader);
                    break;

                case "IDENTITY":
                    if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                        n_Identity_Seed = ParseInteger(reader);
                        ParseToken(reader, Core.TokenID.Comma);
                        n_Identity_Increment = ParseInteger(reader);
                        ParseToken(reader, Core.TokenID.RrBracket);
                    }
                    n_ColumnFlags |= DataModel.ValueFlags.Identity;
                    break;

                case "NULL":
                    n_ColumnFlags |= DataModel.ValueFlags.Nullable;
                    break;

                case "NOT":
                    ParseToken(reader, Core.TokenID.NULL);
                    n_ColumnFlags &= ~DataModel.ValueFlags.Nullable;
                    break;

                case "PRIMARY":
                    ParseToken(reader, Core.TokenID.KEY);
                    n_ColumnFlags |= DataModel.ValueFlags.PrimaryKey;
                    break;
                }
            }
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            n_Datatype.TranspileNode(context);
            context.ValidateInteger(n_Identity_Seed, 0, int.MaxValue);
            context.ValidateInteger(n_Identity_Increment, 1, int.MaxValue);

            if (n_Default != null)
                n_Default.TranspileNode(context);

            if (n_Datatype.SqlType != null) {
                Column = new DataModel.ColumnDS(n_Name.ValueString,
                                                sqlType:       n_Datatype.SqlType,
                                                flags:         n_ColumnFlags,
                                                declaration:   n_Name,
                                                collationName: n_Collation?.ValueString);
                n_Name.SetSymbolUsage(Column, DataModel.SymbolUsageFlags.Declaration);
            }
        }

        public      override    void                    Emit(EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (node == n_Datatype && TableType == TableType.Temp) {
                    n_Datatype.EmitNative(emitWriter);

                    if (n_Datatype.SqlType.NativeType.hasCollate && n_Collation == null) {
                        emitWriter.WriteText(" COLLATE DATABASE_DEFAULT");
                    }
                }
                else
                    node.Emit(emitWriter);
            }
        }
    }
}
