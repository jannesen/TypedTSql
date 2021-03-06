﻿using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class ASCII: Func_Scalar
    {
        internal                                            ASCII(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType          TranspileReturnType(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 1);
            Validate.Value(arguments[0]);

            var sqlType = arguments[0].SqlType;
            if (sqlType is DataModel.SqlTypeAny)
                return DataModel.SqlTypeNative.Int;

            switch (sqlType.NativeType.SystemType) {
            case DataModel.SystemType.Char:
            case DataModel.SystemType.VarChar:
                return DataModel.SqlTypeNative.Int;
            default:
                return null;
            }
        }
    }
}
