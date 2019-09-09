﻿using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_IntoVariables: AstParseNode
    {
        public      readonly    ISetVariable[]                      n_VariableNames;

        public                                                      Node_IntoVariables(Core.ParserReader reader)
        {
            var setvars = new List<ISetVariable>();

            do {
                setvars.Add(ParseVarVariable(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_VariableNames = setvars.ToArray();
        }
    }
}
