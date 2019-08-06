using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms190290.aspx
    // https://msdn.microsoft.com/en-us/library/ms174969.aspx
    // https://msdn.microsoft.com/en-us/library/ms173497.aspx
    // https://msdn.microsoft.com/en-us/library/ms173492.aspx
    [StatementParser(Core.TokenID.DROP)]
    public class Statement_DROP: Statement
    {
        public      readonly    Core.TokenWithSymbol                n_TempTableName;

        public                                                      Statement_DROP(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DROP);
            ParseToken(reader, Core.TokenID.TABLE);

            n_TempTableName = ParseName(reader);

            ParseStatementEnd(reader);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            var     name = n_TempTableName.ValueString;

            if (!name.StartsWith("#", StringComparison.InvariantCulture)) {
                context.AddError(n_TempTableName, "DROP TABLE is only allow for temp tables.");
                return;
            }

            var tempTable = context.GetDeclarationObjectCode().Entity.TempTableGet(name);
            if (tempTable == null) {
                context.AddError(n_TempTableName, "Unknown temp table '" + name + "'.");
                return;
            }

            n_TempTableName.SetSymbol(tempTable);
            context.CaseWarning(n_TempTableName, tempTable.Name);
        }
    }
}
