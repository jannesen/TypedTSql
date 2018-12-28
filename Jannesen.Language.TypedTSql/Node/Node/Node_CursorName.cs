using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_CursorName: Core.AstParseNode
    {
        public      readonly    bool                        n_Global;
        public      readonly    Core.TokenWithSymbol        n_Name;

        public                  DataModel.Cursor            Cursor;

        public                                              Node_CursorName(Core.ParserReader reader)
        {
            n_Global = ParseOptionalToken(reader, "GLOBAL") != null;
            n_Name   = ParseName(reader);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            Cursor = null;

            if (!(n_Global ? context.Catalog.GetGlobalCursorList() : context.RootContext.GetCursorList()).TryGetValue(n_Name.ValueString, out Cursor)) {
                context.AddError(n_Name, "Unknown cursor '" + n_Name + "'");
                return;
            }

            n_Name.SetSymbol(Cursor);
        }
    }
}
