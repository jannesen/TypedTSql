using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class Table_Column: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol    n_Name;
        public                  DataModel.Column        Column                  { get; protected set; }

        public                                          Table_Column(Core.ParserReader reader)
        {
            n_Name = ParseName(reader);
        }
    }
}
