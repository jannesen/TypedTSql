using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class ISNULL: Func_Scalar
    {
        internal                                            ISNULL(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    FlagsTypeCollation          TranspileResult(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2);
            Validate.Value(arguments[0]);
            Validate.Value(arguments[1]);

            var rtn = TypeHelpers.OperationUnion(arguments);

            if (rtn.ValueFlags.isNullable()) {
                if (!arguments[1].isNullable())
                    rtn.ValueFlags &= ~(DataModel.ValueFlags.NULL|DataModel.ValueFlags.Nullable);
            }

            return rtn;
        }

        public      override    bool                        ValidateConst(DataModel.ISqlType sqlType)
        {
            var v1 = n_Arguments.n_Expressions[0].ValidateConst(sqlType);
            var v2 = n_Arguments.n_Expressions[1].ValidateConst(sqlType);

            return v1 && v2;
        }
    }
}
