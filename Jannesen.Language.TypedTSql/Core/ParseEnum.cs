using System;
using System.Collections.Generic;
using System.Text;

namespace Jannesen.Language.TypedTSql.Core
{
    public class ParseEnum<T>
    {
        public class Seq
        {
            public  readonly    T           Value;
            public  readonly    object[]    ID_Names;

            public                          Seq(T value, params object[] id_names)
            {
                Value    = value;
                ID_Names = id_names;
            }

            public              bool        CanParse(Core.ParserReader reader)
            {
                if (ID_Names.Length > 1) {
                    Core.Token[]    readahead = reader.Peek(ID_Names.Length);

                    for (int i = 1 ; i < ID_Names.Length ; ++i) {
                        if (!readahead[i].isToken(ID_Names[i]))
                            return false;
                    }
                }

                return true;
            }

            public              T           Parse(Core.AstParseNode parseNode, Core.ParserReader reader)
            {
                for (int i = 0 ; i < ID_Names.Length ; ++i)
                    TokenWithSymbol.SetKeyword(parseNode.ParseToken(reader));

                return Value;
            }
        }

        public readonly string          Name;
        public readonly Seq[]           Seqs;

        public                          ParseEnum(string name, params Seq[] seqs)
        {
            Name = name;
            Seqs = seqs;
        }

        public          bool            CanParse(Core.ParserReader reader)
        {
            return _find(reader) != null;
        }
        public          T               Parse(Core.AstParseNode parseNode, Core.ParserReader reader)
        {
            var seq = _find(reader);

            if (seq != null)
                return seq.Parse(parseNode, reader);

            throw new ParseException(reader.CurrentToken, "Invalid " + Name + ".");
        }

        public          Seq             _find(Core.ParserReader reader)
        {
            Core.Token  firstToken     = reader.CurrentToken;
            string      firstTokenText = null;

            for (int i = 0 ; i < Seqs.Length ; ++i) {
                var seq = Seqs[i];

                if (seq.ID_Names[0] is string) {
                    if (firstTokenText == null) {
                        if (!firstToken.isNameOrKeyword)
                            continue;

                        firstTokenText = firstToken.Text.ToUpperInvariant();
                    }
                }

                if (seq.ID_Names[0] is string ? (string)seq.ID_Names[0] == firstTokenText
                                              : (Core.TokenID)seq.ID_Names[0] == firstToken.ID)
                {
                    if (seq.CanParse(reader))
                        return seq;
                }
            }

            return null;
        }
    }
}
