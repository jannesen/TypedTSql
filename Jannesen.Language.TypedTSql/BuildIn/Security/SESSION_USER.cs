﻿using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SESSION_USER: Func_WithOutArgs
    {
        public      override    DataModel.ISqlType          SqlType             { get { return DataModel.SqlTypeNative.NVarChar_128;  } }

        internal                                            SESSION_USER(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader, brackets:false)
        {
        }
    }
}
