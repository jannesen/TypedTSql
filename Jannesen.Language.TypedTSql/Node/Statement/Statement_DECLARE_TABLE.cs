using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms188927.aspx
    //  DECLARE LocalName [AS] TABLE <TableTypeDefinition>
    [StatementParser(Core.TokenID.DECLARE, prio:3)]
    public class Statement_DECLARE_TABLE: Statement
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    Table                           n_Table;
        public                  DataModel.VariableLocal         t_Variable          { get; private set; }

        public      static      bool                            CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            Core.Token[]        peek = reader.Peek(3);

            return (peek[0].ID == Core.TokenID.DECLARE && peek[1].ID == Core.TokenID.LocalName && peek[2].ID == Core.TokenID.TABLE);
        }
        public                                                  Statement_DECLARE_TABLE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.DECLARE);
            n_Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
            ParseToken(reader, Core.TokenID.TABLE);
            n_Table = AddChild(new Table(reader, TableType.Type));
            ParseStatementEnd(reader, parseContext);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            t_Variable = null;

            n_Table.TranspileNode(context);

            if (n_Table.Columns != null) {
                t_Variable = new DataModel.VariableLocal(n_Name.Text,
                                                         new DataModel.SqlTypeTable(n_Table.Columns, n_Table.Indexes),
                                                         n_Name,
                                                         DataModel.VariableFlags.None);
                context.VariableDeclare(n_Name, VarDeclareScope.BlockScope, t_Variable);
                n_Name.SetSymbolUsage(t_Variable, DataModel.SymbolUsageFlags.Declaration);
            }
        }
    }
}
