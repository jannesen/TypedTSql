using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Declarations: Core.AstParseNode
    {
        public  readonly        Node.Declaration[]              n_Declarations;

        internal                                                Declarations(Core.ParserReader reader)
        {
            var declarations = new List<Declaration>();

            for(;;) {
                reader.ReadBlanklines(this);

                if (reader.CurrentToken.ID == Core.TokenID.EOF)
                    break;

                var     savedPosition = reader.Position;

                try {
                    declarations.Add(AddChild(reader.Transpiler.DeclarationParsers.Parse(reader, null)));
                }
                catch(Exception err) {
                    reader.AddError(err);

                    var errNode = new Core.AstParseErrorNode(reader, savedPosition);

                    while (!(reader.CurrentToken.ID == Core.TokenID.EOF || reader.Transpiler.DeclarationParsers.CanParse(reader, null)))
                        reader.ReadToken(errNode);

                    AddChild(errNode);
                }
            }

            reader.ReadLeading(this);
            n_Declarations = declarations.ToArray();
        }

        public      override            void                    TranspileNode(Transpile.Context context)
        {
            throw new InvalidOperationException();
        }
    }
}
