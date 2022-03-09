using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms187373.aspx
    //  Data_TableHints ::=
    //      WITH ( tablehint [ [, ]...n ] )
    public class Node_TableHints: Core.AstParseNode
    {
        [Flags]
        public enum Hint
        {
            FORCESCAN               = 0x00000001,
            HOLDLOCK                = 0x00000004,
            IGNORE_CONSTRAINTS      = 0x00000008,
            IGNORE_TRIGGERS         = 0x00000010,
            KEEPDEFAULTS            = 0x00000020,
            KEEPIDENTITY            = 0x00000040,
            NOEXPAND                = 0x00000080,
            NOLOCK                  = 0x00000100,
            NOWAIT                  = 0x00000200,
            PAGLOCK                 = 0x00000400,
            READCOMMITTED           = 0x00000800,
            READCOMMITTEDLOCK       = 0x00001000,
            READPAST                = 0x00002000,
            READUNCOMMITTED         = 0x00004000,
            REPEATABLEREAD          = 0x00008000,
            ROWLOCK                 = 0x00010000,
            SERIALIZABLE            = 0x00020000,
            SNAPSHOT                = 0x00040000,
            TABLOCK                 = 0x00080000,
            TABLOCKX                = 0x00100000,
            UPDLOCK                 = 0x00200000,
            XLOCK                   = 0x00400000,
            _INDEX                  = 0x10000000
        }

        public      readonly    Hint                        n_Hints;
        public      readonly    Core.TokenWithSymbol[]      n_Indexes;

        public                                              Node_TableHints(Core.ParserReader reader)
        {
            List<Core.TokenWithSymbol>      indexes = null;

            ParseToken(reader, Core.TokenID.WITH);
            ParseToken(reader, Core.TokenID.LrBracket);

            do {
                Hint    hint = ParseEnum<Hint>(reader, _parseEnum);

                n_Hints |= hint;

                switch(hint) {
                case Hint._INDEX:
                    if (indexes == null)
                        indexes = new List<Core.TokenWithSymbol>();

                    ParseToken(reader, Core.TokenID.Equal);
                    indexes.Add(ParseName(reader));
                    break;
                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Indexes = indexes?.ToArray();
        }

        public      override        void                    TranspileNode(Transpile.Context context)
        {
        }

        public                      void                    CheckIndexes(Transpile.Context context, DataModel.ISymbol entity)
        {
            if (n_Indexes != null) {
                if (entity is DataModel.ITable entityTable) {
                    foreach(var index in n_Indexes) {
                        if (entityTable.Indexes != null && entityTable.Indexes.TryGetValue(index.ValueString, out var tableIndex))
                            index.SetSymbolUsage(tableIndex, DataModel.SymbolUsageFlags.Reference);
                        else
                            context.AddError(index, "Unknown index in "+ entity.Name + ".");
                    }
                }
                else
                    context.AddError(this, "Not a table reference.");
            }
        }

        private static  Core.ParseEnum<Hint>                _parseEnum = new Core.ParseEnum<Hint>(
                                                                "Table hint",
                                                                new Core.ParseEnum<Hint>.Seq(Hint.FORCESCAN,            "FORCESCAN"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.HOLDLOCK,             "HOLDLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.IGNORE_CONSTRAINTS,   "IGNORE_CONSTRAINTS"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.IGNORE_TRIGGERS,      "IGNORE_TRIGGERS"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.KEEPDEFAULTS,         "KEEPDEFAULTS"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.KEEPIDENTITY,         "KEEPIDENTITY"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.NOEXPAND,             "NOEXPAND"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.NOLOCK,               "NOLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.NOWAIT,               "NOWAIT"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.PAGLOCK,              "PAGLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.READCOMMITTED,        "READCOMMITTED"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.READCOMMITTEDLOCK,    "READCOMMITTEDLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.READPAST,             "READPAST"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.READUNCOMMITTED,      "READUNCOMMITTED"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.REPEATABLEREAD,       "REPEATABLEREAD"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.ROWLOCK,              "ROWLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.SERIALIZABLE,         "SERIALIZABLE"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.SNAPSHOT,             "SNAPSHOT"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.TABLOCK,              "TABLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.TABLOCKX,             "TABLOCKX"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.UPDLOCK,              "UPDLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint.XLOCK,                "XLOCK"),
                                                                new Core.ParseEnum<Hint>.Seq(Hint._INDEX,               "INDEX")
                                                            );
    }
}
