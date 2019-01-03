using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class STUFF: Func_Scalar
    {
        internal                                            STUFF(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 4);
            var t_expression = arguments[0].SqlType;
            var t_replace    = arguments[3].SqlType;

            if (t_expression is DataModel.SqlTypeAny)
                return t_expression;
            if (t_replace is DataModel.SqlTypeAny)
                return t_replace;

            var nt_expression = t_expression.NativeType;
            var nt_replace    = t_replace.NativeType;

            if (nt_expression.SystemType == DataModel.SystemType.Binary || nt_expression.SystemType == DataModel.SystemType.VarBinary) {
                Validate.ValueBinary(arguments[0]);
                Validate.ValueInt(arguments[1]);
                Validate.ValueInt(arguments[2]);
                Validate.ValueBinary(arguments[3]);

                return new DataModel.SqlTypeNative(DataModel.SystemType.VarBinary,
                                                    maxLength:  _constLength(nt_expression, nt_replace, arguments));
            }
            else {
                Validate.ValueString(arguments[0]);
                Validate.ValueInt(arguments[1]);
                Validate.ValueInt(arguments[2]);
                Validate.ValueString(arguments[3]);

                return new DataModel.SqlTypeNative((nt_expression.isUnicode || nt_replace.isUnicode ? DataModel.SystemType.NVarChar : DataModel.SystemType.VarChar),
                                                   maxLength:   _constLength(nt_expression, nt_replace, arguments));
            }
        }

        private                 int                         _constLength(DataModel.SqlTypeNative nt_expression, DataModel.SqlTypeNative nt_replace, IExprNode[] arguments)
        {
            if (nt_expression.MaxLength < 0)
                return -1;

            if (nt_replace.MaxLength < 0)
                return -1;

            if (arguments[2].isConstant()) {
                var lengthConst = arguments[2].ConstValue();

                if (lengthConst is int)
                    return nt_expression.MaxLength + nt_replace.MaxLength - (int)lengthConst;
            }

            return nt_expression.MaxLength + nt_replace.MaxLength;
        }
    }
}
