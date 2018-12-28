using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class PERMISSIONS: Func_Scalar
    {
        internal                                            PERMISSIONS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            base.TranspileNode(context);

            var stringToken = LogicHelpers.ConstString(n_Arguments.n_Expressions[0]);
            if (stringToken != null) {
                var parseEntityName = new Library.ParseEntityName(stringToken.ValueString);
                if (parseEntityName.Database == null) {
                    if (parseEntityName.Schema == null) {
                        context.AddError(stringToken, "schema missing.");
                        return;
                    }

                    var entityName = new DataModel.EntityName(parseEntityName.Schema, parseEntityName.Name);

                    var entity = context.Catalog.GetObject(entityName);
                    if (entity == null) {
                        context.AddError(stringToken, "Unknown object '" + entityName + "'.");
                        return;
                    }

                    stringToken.SetSymbol(entity);
                }
            }
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1, 2);
            Validate.ValueString(arguments[0]);

            if (arguments.Length >= 2)
                Validate.ValueString(arguments[1]);

            return DataModel.SqlTypeNative.Int;
        }
    }
}
