using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CONTEXT_INFO: Func_WithOutArgs
    {
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.VarBinary_128;  } }

        internal                                            CONTEXT_INFO(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
