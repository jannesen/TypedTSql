using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/concat-transact-sql
    public class CONCAT: Func_Scalar
    {
        internal                                            CONCAT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    FlagsTypeCollation          TranspileResult(IExprNode[] arguments)
        {
            Validate.NumberOfArguments(arguments, 2, 256);

            var     valueFlags = DataModel.ValueFlags.None;
            bool    var        = false;
            bool    n          = false;
            int     length     = 0;

            for (int i = 0 ; i < arguments.Length ; ++i) {
                valueFlags |= arguments[i].ValueFlags;
                Validate.ValueString(arguments[i]);

                var sqlType = arguments[i].SqlType;
                if (sqlType != null && !(sqlType is DataModel.SqlTypeAny)) {
                    var nativeType  = sqlType.NativeType;

                    switch(nativeType.SystemType) {
                    case DataModel.SystemType.Char:                                 break;
                    case DataModel.SystemType.NChar:                    n = true;   break;
                    case DataModel.SystemType.VarChar:      var = true;             break;
                    case DataModel.SystemType.NVarChar:     var = true; n = true;   break;
                    }

                    length = (nativeType.MaxLength == -1) ? -1 : length + nativeType.MaxLength;
                }
            }

            return new FlagsTypeCollation() {
                           ValueFlags = LogicStatic.FunctionValueFlags(valueFlags & ~(DataModel.ValueFlags.Nullable|DataModel.ValueFlags.NULL)),
                           SqlType    = DataModel.SqlTypeNative.NewString(n, var, length)
                       };
        }
    }
}
