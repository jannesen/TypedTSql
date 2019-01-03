using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CHOOSE: Func_Scalar
    {
        internal                                            CHOOSE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    FlagsTypeCollation          TranspileResult(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3, 256);

            if (!arguments[0].isValid())
                return new FlagsTypeCollation() { ValueFlags = DataModel.ValueFlags.Error };

            Validate.ValueInt(arguments[0]);

            var values = new IExprNode[arguments.Length - 1];

            for (int i = 1 ; i < arguments.Length ; ++i)
                values[i - 1] = arguments[i];

            return TypeHelpers.OperationUnion(values);
        }

        public      override    bool                        ValidateConst(DataModel.ISqlType sqlType)
        {
            bool    rtn = true;

            for (int i = 1 ; i < n_Arguments.n_Expressions.Length ; ++i) {
                if (!n_Arguments.n_Expressions[i].ValidateConst(sqlType))
                    rtn = false;
            }

            return rtn;
        }
    }
}
