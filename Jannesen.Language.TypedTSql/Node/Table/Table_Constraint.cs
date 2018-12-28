using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class Table_Constraint: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol        n_Name;

        public                                              Table_Constraint(Core.ParserReader reader, TableType type)
        {
//          if (type == TableType.Temp)
//          {
//              ParseToken(reader, Core.TokenID.CONSTRAINT);
//              n_Name = ParseName(reader);
//          }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
//          if (n_Name != null)
//              context.AddError(this, "Named constraint are system width and there for not allowed.");
        }
    }
}
