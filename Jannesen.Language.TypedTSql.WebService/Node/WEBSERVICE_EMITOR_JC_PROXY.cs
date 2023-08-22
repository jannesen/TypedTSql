using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class WEBSERVICE_EMITOR_JC_PROXY: WEBSERVICE_EMITOR
    {
        public class JcTypeMapDictionary: Dictionary<object, Emit.JcNSExpression>
        {
            public                          JcTypeMapDictionary()
            {
            }
        }

        public class TypeMap: LTTSQL.Core.AstParseNode
        {
            public      readonly    TypeMapEntry[]                                          n_Entrys;

            public                  JcTypeMapDictionary                                     TypeMapDictionary      { get; private set; }

            public                                                                          TypeMap(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, "TYPEMAP");
                ParseToken(reader, Core.TokenID.LrBracket);

                var entries = new List<TypeMapEntry>();

                do {
                    entries.Add(AddChild(new TypeMapEntry(reader)));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);
                n_Entrys = entries.ToArray();
            }

            public      override    void                                                    TranspileNode(LTTSQL.Transpile.Context context)
            {
                TypeMapDictionary = null;

                n_Entrys.TranspileNodes(context);

                var typeMapDictionary = new JcTypeMapDictionary();

                foreach (var entry in n_Entrys) {
                    var sqlType = entry.SourceType;

                    if (sqlType != null) {
                        if (!typeMapDictionary.TryGetValue(sqlType, out var found)) {
                            try {
                                typeMapDictionary.Add(sqlType, new Emit.JcNSExpression(entry.n_TypeScriptType.ValueString));
                            }
                            catch(Exception err) {
                                context.AddError(entry.n_TypeScriptType, err);
                            }
                        }
                        else
                            context.AddError(entry, "Duplicate declaration of '" + sqlType.ToString() + "'.");
                    }
                }

                TypeMapDictionary = typeMapDictionary;
            }
        }

        public class TypeMapEntry: LTTSQL.Core.AstParseNode
        {
            public      readonly    LTTSQL.Core.AstParseNode            n_Type;
            public      readonly    LTTSQL.Token.DataIsland             n_TypeScriptType;

            public                  object                              SourceType
            {
                get {
                    if (n_Type is LTTSQL.Node.Node_Datatype dataType) return dataType.SqlType;
                    if (n_Type is ComplexType complexType)            return complexType.WebComplexType;
                    throw new InvalidOperationException("SqlType failed.");
                }
            }

            public                                                      TypeMapEntry(LTTSQL.Core.ParserReader reader)
            {
                n_Type   = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                 : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                ParseToken(reader, LTTSQL.Core.TokenID.AS);
                n_TypeScriptType = (LTTSQL.Token.DataIsland)ParseToken(reader, LTTSQL.Core.TokenID.DataIsland);
            }

            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                n_Type.TranspileNode(context);
            }
        }

        public      readonly    string                          n_BaseUrl;
        public      readonly    TypeMap                         n_TypeMap;

        public                                                  WEBSERVICE_EMITOR_JC_PROXY(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext)
        {
            ParseToken(reader, "JC_PROXY");
            ParseToken(reader, Core.TokenID.LrBracket);

            while(!reader.CurrentToken.isToken(Core.TokenID.RrBracket)) {
                switch(reader.CurrentToken.Text.ToUpper()) {
                case "BASEURL":
                    ParseToken(reader, "BASEURL");
                    ParseToken(reader, LTTSQL.Core.TokenID.Equal);
                    n_BaseUrl = ParseToken(reader, LTTSQL.Core.TokenID.String).ValueString;
                    break;

                case "TYPEMAP":
                    n_TypeMap = new TypeMap(reader);
                    break;

                default:
                    throw new ParseException(reader.CurrentToken, "Except DATABASE got " + reader.CurrentToken.Text.ToString() + ".");
                }
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_TypeMap?.TranspileNode(context);
        }
        internal    override    Emit.FileEmitor                 ConstructEmitor(string basedirectory)
        {
            return new Emit.JcProxyEmitor(this, basedirectory); 
        }
    }
}
