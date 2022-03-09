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

        public                  DataModel.Label                     Label       { get; private set; }

        public                                                      Statement_GOTO(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.GOTO);
            n_Label = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Name);

            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            if (!context.RootContext.GetLabelList().TryGetValue(n_Label.ValueString, out var label)) {
                context.AddError(n_Label, "Unknown label '" + n_Label.ValueString + "'.");
                return;
            }

            Label = label;
            n_Label.SetSymbolUsage(label, DataModel.SymbolUsageFlags.Declaration);
            context.CaseWarning(n_Label, label.Name);
        }
    }
}
