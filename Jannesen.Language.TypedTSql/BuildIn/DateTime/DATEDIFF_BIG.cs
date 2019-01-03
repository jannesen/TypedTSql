using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class DATEDIFF_BIG: DATEDIFF
    {
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.BigInt;  } }

        internal                                            DATEDIFF_BIG(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }
    }
}
