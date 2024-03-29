﻿using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // { @parameter_name [ AS ] Datatype [ = default ] [ READONLY ] }  [ ,...n ] ]
    public class Node_ParameterList: Core.AstParseNode
    {
        public  delegate        Node_Parameter  CreateNodeParameter(Core.ParserReader reader);

        public      readonly    Node_Parameter[]                    n_Parameters;
        public                  DataModel.ParameterList             t_Parameters                { get; private set; }

        public                                                      Node_ParameterList(Core.ParserReader reader, Node_SqlParameter.InterfaceType interfaceType): this(reader, (r) => new Node_SqlParameter(r, interfaceType))
        {
        }
        public                                                      Node_ParameterList(Core.ParserReader reader, CreateNodeParameter createNodeParameter)
        {
            var parameters = new List<Node_Parameter>();

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                ParseToken(reader, Core.TokenID.LrBracket);

                if (reader.CurrentToken.isToken(Core.TokenID.LocalName)) {
                    do {
                        parameters.Add(AddChild(createNodeParameter(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
                }

                ParseToken(reader, Core.TokenID.RrBracket);
            }

            n_Parameters = parameters.ToArray();
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            t_Parameters = null;

            n_Parameters.TranspileNodes(context);

            t_Parameters = _createParameterList(context);
        }
        public      override    void                                Emit(EmitWriter emitWriter)
        {
            if (Children != null) {
                var children = new List<IAstNode>(Children);

    next:
                for (int i = 0 ; i < children.Count; i++) {
                    if (children[i] is Node_Parameter p && p.n_Name == null) {
                        int e = i;
                        int b = i;
                        --i;

                        while (i >= 0 && children[i].isWhitespaceOrComment) {
                            --i;
                        }

                        if (i >= 0 && children[i] is Token.Operator t && t.ID == TokenID.Comma) {
                            b = i;
                        }
                        children.RemoveRange(b, e-b+1);
                        goto next;
                    }
                }

                foreach(var node in children) {
                    node.Emit(emitWriter);
                }
            }
        }

        private                 DataModel.ParameterList             _createParameterList(Transpile.Context context)
        {
            var parameters = new DataModel.ParameterList(n_Parameters.Length);

            foreach(Node_Parameter parameter in n_Parameters) {
                if (parameter.Parameter != null) {
                    if (!parameters.TryAdd(parameter.Parameter))
                        context.AddError(parameter.n_Name, "Parameter " + parameter.Parameter.Name + " already declared.");
                }
            }

            return parameters;
        }
    }
}
