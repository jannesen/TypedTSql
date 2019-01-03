using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CHECKSUM_AGG: Func_Aggragate
    {
        internal                                                    CHECKSUM_AGG(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override            DataModel.ISqlType          TranspileReturnType(DataModel.ISqlType sqlType)
        {
            return DataModel.SqlTypeNative.Int;
        }
    }
}
