using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class REPLACE: Func_Scalar
    {
        internal                                            REPLACE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.ValueString(arguments[0]);
            Validate.ValueString(arguments[1]);
            Validate.ValueString(arguments[2]);

            var t_string      = arguments[0].SqlType;
            var t_pattern     = arguments[1].SqlType;
            var t_replacement = arguments[2].SqlType;

            if (t_string is DataModel.SqlTypeAny)
                return t_string;
            if (t_replacement is DataModel.SqlTypeAny)
                return t_replacement;

            var nt_string      = t_string.NativeType;
            var nt_pattern     = t_pattern.NativeType;
            var nt_replacement = t_replacement.NativeType;

            if (nt_string.MaxLength == -1)
                return nt_string;

            return new DataModel.SqlTypeNative((nt_string.isUnicode || nt_replacement.isUnicode ? DataModel.SystemType.NVarChar : DataModel.SystemType.VarChar),
                                               maxLength:(nt_replacement.MaxLength == -1)
                                                            ? -1
                                                            : nt_replacement.MaxLength <= nt_pattern.MaxLength
                                                                ? nt_string.MaxLength
                                                                : ((nt_string.MaxLength + (nt_pattern.MaxLength - 1)) / nt_pattern.MaxLength) * nt_replacement.MaxLength);
        }
    }
}
