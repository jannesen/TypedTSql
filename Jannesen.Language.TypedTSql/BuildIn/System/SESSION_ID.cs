using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://docs.microsoft.com/en-us/sql/t-sql/functions/session-id-transact-sql
    public class SESSION_ID: Func_WithOutArgs
    {
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.NVarChar_32;  } }

        internal                                            SESSION_ID(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader, true)
        {
        }
    }
}
