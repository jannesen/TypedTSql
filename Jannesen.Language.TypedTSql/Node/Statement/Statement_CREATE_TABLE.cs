using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms174979.aspx
    [StatementParser(Core.TokenID.CREATE, prio:2)]
    public class Statement_CREATE_TABLE: Statement
    {
        public      readonly    Core.TokenWithSymbol                n_Name;
        public      readonly    Table                               n_Table;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.CREATE && reader.NextPeek().isToken(Core.TokenID.TABLE);
        }
        public                                                      Statement_CREATE_TABLE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.CREATE);
            ParseToken(reader, Core.TokenID.TABLE);
            n_Name  = ParseName(reader);
            n_Table = AddChild(new Table(reader, TableType.Temp));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            n_Table.TranspileNode(context);

            if (!context.GetDeclarationObjectCode().Entity.TempTableAdd(n_Name.ValueString, n_Name, n_Table.Columns, n_Table.Indexes, out var tempTable)) {
                context.AddError(n_Name, "Temp table already defined.");
                return;
            }
            n_Name.SetSymbol(tempTable);
        }
    }
}
