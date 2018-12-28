using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class IIF: Func_Scalar
    {
        internal                                            IIF(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    FlagsTypeCollation          TranspileResult(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.BooleanExpression(arguments[0]);

            Validate.ValueInt(arguments[0]);

            return TypeHelpers.OperationUnion(new IExprNode[] { arguments[1], arguments[2] });
        }

        public      override    bool                        ValidateConst(DataModel.ISqlType sqlType)
        {
            bool    rtn = true;

            for (int i = 1 ; i < n_Arguments.n_Expressions.Length ; ++i) {
                if (n_Arguments.n_Expressions[i].ValidateConst(sqlType))
                    rtn = false;
            }

            return rtn;
        }
    }
}
