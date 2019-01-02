using System;
using System.Collections.Generic;
using System.IO;

namespace Jannesen.Language.TypedTSql.Core
{
    public class AstParseErrorNode: Core.AstParseNode
    {
        internal                                    AstParseErrorNode(Core.ParserReader reader, Core.ParserReader.ParsePosition savedPosition)
        {
            if (savedPosition.CurrentParse < reader.Position.CurrentParse) {
                for (int i = savedPosition.Processed ; i < reader.Position.Processed ; ++i)
                    this.AddChild(reader.Tokens[i]);
            }
            else
                reader.ReadToken(this);
        }

        public      override    void                TranspileNode(Transpile.Context context)
        {
        }
    }
}
