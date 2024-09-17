using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_Attributes: Core.AstParseNode, IAttributes
    {
        public  readonly        Node_Attribute[]                n_Attributes;

        public                  IAttributes                     Attributes => this;

                                IAttributeValue                 IAttributes.this[int idx]       => (IAttributeValue)n_Attributes[idx];
                                int                             IAttributes.Count               => n_Attributes.Length;
                                IAttributeValue                 IAttributes.Find(string name)
        {
            for (int i = 0 ; i < n_Attributes.Length ; ++i) {
                if (n_Attributes[i].n_Name.ValueString == name) {
                    return n_Attributes[i];
                }
            }

            return null;
        }
                                IEnumerator<IAttributeValue>    IAttributes.GetEnumerator()
        {
            return new ArrayCastEnumerator<IAttributeValue>(n_Attributes);
        }

        public  static          bool                            CanParse(Core.ParserReader reader)
        {
             return reader.CurrentToken.isToken("ATTRIBUTES");
        }
        public                                                  Node_Attributes(Core.ParserReader reader)
        {
            ParseToken(reader, "ATTRIBUTES");
            ParseToken(reader, Core.TokenID.LrBracket);

            var attributes = new List<Node_Attribute>();

            do {
                attributes.Add(AddChild(new Node_Attribute(reader)));
            }
            while(ParseOptionalToken(reader, TokenID.Comma) != null);

            n_Attributes = attributes.ToArray();

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Attributes.TranspileNodes(context);
        }
        public      override    void                            Emit(EmitWriter emitWriter)
        {
        }
    }
}
