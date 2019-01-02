using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //Data_SchemaDeclarationColumn ::=
    //      Name Datetype [ ColPattern | MetaProperty]
    public class TableSource_WithDeclarationColumn: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol        n_Name;
        public      readonly    Node_Datatype               n_Type;
        public      readonly    Core.Token                  n_Xquery;

        public                                              TableSource_WithDeclarationColumn(Core.ParserReader reader)
        {
            n_Name   = ParseName(reader);
            n_Type   = AddChild(new Node_Datatype(reader));
            n_Xquery = ParseOptionalToken(reader, Core.TokenID.String);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            n_Type.TranspileNode(context);
        }
    }
}
