using System;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // Expression_Function:
    //      : Objectname '(' Expression ( ',' Expression )* ')'
    public class UPDATE: ExprBooleanBuildIn
    {
        public      readonly    Core.TokenWithSymbol        n_ComlumnName;
        public                  DataModel.Column            t_Column                { get; private set; }

        internal                                            UPDATE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_ComlumnName = ParseName(reader);
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            t_Column = null;

            try {
                var triggerDeclaration = context.GetDeclarationObject<Declaration_TRIGGER>();
                if (triggerDeclaration == null)
                    throw new ErrorException("Not in a trigger.");

                var columnList = (triggerDeclaration.n_Table.Entity as DataModel.EntityObjectTable)?.Columns;
                if (columnList == null)
                    throw new ErrorException("Can't get columnlist from trigger table.");

                if ((t_Column = columnList.FindColumn(n_ComlumnName.ValueString, out bool ambiguous)) != null && !ambiguous) {
                    context.CaseWarning(n_ComlumnName, t_Column.Name);
                    n_ComlumnName.SetSymbolUsage(t_Column, DataModel.SymbolUsageFlags.Reference);
                }
                else
                    context.AddError(n_ComlumnName, "Unknown column [" + n_ComlumnName.ValueString + "].");
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
