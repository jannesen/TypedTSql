using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_InterfaceList: Core.AstParseNode
    {
        public      readonly    Node_Interface[]        n_Interfaces;
        public                  DataModel.InterfaceList Interfaces          { get; private set; }

        public                                          Node_InterfaceList(Core.ParserReader reader)
        {
            var interfaces = new List<Node_Interface>();

            if (ParseOptionalToken(reader, Core.TokenID.BEGIN) != null) {
                do {
                    interfaces.Add(AddChild(new Node_Interface(reader)));
                }
                while (reader.CurrentToken.isToken(TokenID.PROPERTY, TokenID.METHOD));

                ParseToken(reader, Core.TokenID.END);
            }

            n_Interfaces = interfaces.ToArray();
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            n_Interfaces.TranspileNodes(context);

            Interfaces = new DataModel.InterfaceList();

            foreach (var nodeInterface in n_Interfaces)
                Interfaces.Add(nodeInterface.Interface);
        }
        public      override    void                    Emit(EmitWriter emitWriter)
        {
        }
    }
}
