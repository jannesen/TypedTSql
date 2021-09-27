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
        public      readonly    bool                        n_AsJSon;

        public                                              TableSource_WithDeclarationColumn(Core.ParserReader reader, TableSourceWithType type)
        {
            n_Name   = ParseName(reader);
            n_Type   = AddChild(new Node_Datatype(reader));
            n_Xquery = ParseOptionalToken(reader, Core.TokenID.String);

            if (type == TableSourceWithType.Json && ParseOptionalToken(reader, Core.TokenID.AS) != null) {
                ParseToken(reader, "JSON");
                n_AsJSon = true;
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            n_Type.TranspileNode(context);

            if (n_AsJSon) {
                var nativetype = n_Type.SqlType?.NativeType;
                if (nativetype != null &&
                    !(nativetype.SystemType == DataModel.SystemType.NVarChar && nativetype.MaxLength == -1)) {
                    context.AddError(n_Type, "Expect VARCHAR(MAX) for AS JSON.");
                }
            }
        }
    }
}
