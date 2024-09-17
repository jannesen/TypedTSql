using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    [DeclarationParser("DEFINE", prio:1)]
    public class Declaration_DEFINE_ATTRIBUTE: Declaration
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    DataModel.TAttributeType        n_Type;
        public      readonly    TokenWithSymbol[]               n_Names;

        public      static      bool                            CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.isToken("DEFINE") && reader.NextPeek().isToken("ATTRIBUTE");
        }
        public                                                  Declaration_DEFINE_ATTRIBUTE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, "DEFINE");
            ParseToken(reader, "ATTRIBUTE");
            n_Name = ParseName(reader);
            n_Type = (DataModel.TAttributeType)ParseToken(reader, _types);

            switch(n_Type) {
            case DataModel.TAttributeType.Enum:
            case DataModel.TAttributeType.Flags:
                var names = new List<TokenWithSymbol>();
                ParseToken(reader, Core.TokenID.LrBracket);
                do {
                    names.Add(ParseName(reader));
                } while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
                ParseToken(reader, Core.TokenID.RrBracket);
                n_Names = names.ToArray();
                break;

            }
        }

        public      override    void                            TranspileInit(Transpile.TranspileContext transpileContext, SourceFile sourceFile)
        {
            List<TAttributeEnumValue> names = null;

            if (n_Names != null) {
                names = new List<TAttributeEnumValue>();
                foreach(var n in n_Names) {
                    var v = new DataModel.TAttributeEnumValue(n, n.ValueString);
                    n.SetSymbolUsage(v, SymbolUsageFlags.Declaration);
                    names.Add(v);
                }
            }

            var attr = new DataModel.TAttribute(n_Name, n_Name.ValueString, n_Type, names?.ToArray());
            n_Name.SetSymbolData(new SymbolUsage(attr, SymbolUsageFlags.Declaration));
            transpileContext.DefineAttribute(attr);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "attribute " + n_Name;
        }

        private     static      Core.TokenNameID[]              _types  = new Core.TokenNameID[]
                                                                        {
                                                                            new Core.TokenNameID("STRING",   (int)DataModel.TAttributeType.String),
                                                                            new Core.TokenNameID("INTEGER",  (int)DataModel.TAttributeType.Integer),
                                                                            new Core.TokenNameID("NUMBER",   (int)DataModel.TAttributeType.Number),
                                                                            new Core.TokenNameID("ENUM",     (int)DataModel.TAttributeType.Enum),
                                                                            new Core.TokenNameID("FLAGS",    (int)DataModel.TAttributeType.Flags)
                                                                        };

    }
}
