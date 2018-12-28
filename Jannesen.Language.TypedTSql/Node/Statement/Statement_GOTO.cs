using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms180188.aspx
    //  GOTO    label
    [StatementParser(Core.TokenID.GOTO)]
    public class Statement_GOTO: Statement
    {
        public      readonly    Core.TokenWithSymbol                n_Label;

        public                                                      Statement_GOTO(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.GOTO);
            n_Label = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Name);

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            if (!context.RootContext.GetLabelList().TryGetValue(n_Label.ValueString, out var label)) {
                context.AddError(n_Label, "Unknown label '" + n_Label.ValueString + "'.");
                return;
            }

            n_Label.SetSymbol(label);
            context.CaseWarning(n_Label, label.Name);
        }
    }
}
