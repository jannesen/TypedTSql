using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/nl-nl/library/ms180188.aspx
    //  label :
    [StatementParser(Core.TokenID.Name, prio:2)]
    public class Statement_label: Statement
    {
        public      readonly    Core.TokenWithSymbol                n_Label;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.Name && reader.NextPeek().ID == Core.TokenID.Colon;
        }
        public                                                      Statement_label(Core.ParserReader reader, IParseContext parseContext)
        {
            n_Label = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Name);
            ParseToken(reader, Core.TokenID.Colon);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            context.RootContext.ScopeIndentityType = null;
            context.RootContext.GetLabelList();
        }
    }
}
