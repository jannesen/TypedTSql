using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //      GRANT Permissions TO DatabasePrincipal,...
    //
    public class Node_ObjectGrantList: Core.AstParseNode
    {
        public                      Node_ObjectGrant[]              n_Grants;

        internal                                                    Node_ObjectGrantList(Core.ParserReader reader, DataModel.SymbolType type)
        {
            var grants = new List<Node_ObjectGrant>();

            while (reader.CurrentToken.isToken(Core.TokenID.GRANT))
                grants.Add(AddChild(new Node_ObjectGrant(reader, type)));

            n_Grants = grants.ToArray();
        }

        public      override        void                            TranspileNode(Transpile.Context context)
        {
            n_Grants.TranspileNodes(context);
        }
        public      override        void                            Emit(Core.EmitWriter emitWriter)
        {
        }
        public                      void                            EmitGrant(string securable, DataModel.EntityName objectname, Core.EmitWriter emitWriter)
        {
            if (Children != null) {
                foreach(var node in Children) {
                    if (node is Node_ObjectGrant)
                        ((Node_ObjectGrant)node).EmitGrant(securable, objectname, emitWriter);
                }
            }
        }
    }
}
