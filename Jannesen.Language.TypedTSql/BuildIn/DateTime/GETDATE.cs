using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class GETDATE: Func_WithOutArgs
    {
        public      override    DataModel.ISqlType      SqlType     { get { return DataModel.SqlTypeNative.DateTime;        } }

        internal                                        GETDATE(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
