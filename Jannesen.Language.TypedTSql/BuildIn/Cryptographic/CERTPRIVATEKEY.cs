﻿using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class CERTPRIVATEKEY: Func_Scalar_TODO
    {
        internal                                            CERTPRIVATEKEY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

//      protected   override    DataModel.SqlType           TranspileReturnType(IExpr[] arguments)
//      {
//      }
    }
}
