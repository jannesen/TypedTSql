using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class ORIGINAL_LOGIN: Func_WithOutArgs
    {
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.SysName;  } }

        internal                                            ORIGINAL_LOGIN(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader, brackets:false)
        {
        }
    }
}
