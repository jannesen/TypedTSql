using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SUBSTRING: Func_Scalar
    {
        internal                                            SUBSTRING(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 3);
            Validate.ValueString(arguments[0]);
            Validate.ValueInt(arguments[1]);
            Validate.ValueInt(arguments[2]);

            var typeExpr = arguments[0].SqlType;
            if (typeExpr is DataModel.SqlTypeAny)
                return typeExpr;

            if (typeExpr.NativeType.MaxLength > 0 && arguments[2].isConstant()) {
                var constLength = arguments[2].ConstValue();

                if (constLength is int) {
                    var constStart = arguments[1].ConstValue();

                    if (constStart is int) {
                        if ((typeExpr.NativeType.SystemType == DataModel.SystemType.Char ||
                             typeExpr.NativeType.SystemType == DataModel.SystemType.NChar) &&
                            (int)constStart - 1 + (int)constLength <= typeExpr.NativeType.MaxLength)
                            return new DataModel.SqlTypeNative((typeExpr.NativeType.isUnicode ? DataModel.SystemType.NChar : DataModel.SystemType.Char),
                                                               maxLength: (int)constLength);
                    }

                    return new DataModel.SqlTypeNative((typeExpr.NativeType.isUnicode ? DataModel.SystemType.NVarChar : DataModel.SystemType.VarChar),
                                                        maxLength: Math.Min(typeExpr.NativeType.MaxLength - (constStart is int ? (int)constStart-1 : 0),
                                                                            (int)constLength));
                }
            }

            return typeExpr.NativeType;
        }
    }
}
