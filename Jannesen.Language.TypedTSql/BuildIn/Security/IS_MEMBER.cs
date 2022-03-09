using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class IS_MEMBER: Func_Scalar
    {
        public                  DataModel.DatabasePrincipal Principal      { get; private set; }

        internal                                            IS_MEMBER(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            Principal = null;
            base.TranspileNode(context);

            var stringToken = LogicHelpers.ConstString(n_Arguments.n_Expressions[0]);
            if (stringToken != null) {
                Principal = context.Catalog.GetPrincipal(stringToken.ValueString);
                if (Principal == null) {
                    context.AddError(stringToken, "Unknown principal '" + stringToken.ValueString + "'.");
                    return;
                }

                stringToken.SetSymbolUsage(Principal, DataModel.SymbolUsageFlags.Reference);
            }
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.ValueString(arguments[0]);
            return DataModel.SqlTypeNative.Int;
        }
    }
}
