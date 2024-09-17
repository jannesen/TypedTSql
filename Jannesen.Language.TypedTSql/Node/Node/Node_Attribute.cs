using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_Attribute: Core.AstParseNode, IAttributeValue
    {
        public class NameList: Core.AstParseNode
        {
            public                  TokenWithSymbol[]           n_Names;

            public                                              NameList(Core.ParserReader reader)
            {
                ParseToken(reader, TokenID.LrBracket);

                var names = new List<TokenWithSymbol>();
                do {
                    names.Add(ParseName(reader));
                } while(ParseOptionalToken(reader, TokenID.Comma) != null);

                n_Names = names.ToArray();
                ParseToken(reader, TokenID.RrBracket);
            }
            public      override    void                        TranspileNode(Transpile.Context context)
            {
            }
            public      override    void                        Emit(EmitWriter emitWriter)
            {
            }
        }

        public  readonly        TokenWithSymbol             n_Name;
        public  readonly        IAstNode                    n_Value;

        public                  TAttributeType              t_Type;
        public                  object                      t_Value;

                                string                      IAttributeValue.Name      => n_Name.ValueString;
                                TAttributeType              IAttributeValue.Type      => t_Type;
                                object                      IAttributeValue.Value     => t_Value;

        public                                              Node_Attribute(Core.ParserReader reader)
        {
            n_Name = ParseName(reader);

            ParseToken(reader, Core.TokenID.Equal);
            switch(reader.CurrentToken.ID) {
            case TokenID.Name:
            case TokenID.QuotedName:
            case TokenID.String:
            case TokenID.Number:
                n_Value = ParseToken(reader);
                break;

            case TokenID.LrBracket:
                n_Value = AddChild(new NameList(reader));
                break;

            default:
                throw new ParseException(reader.CurrentToken, "Except <name>,<number>,( got " + reader.CurrentToken.ID.ToString() + ".");
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            t_Value = null;

            var attr = context.TranspileContext.FindAttribute(n_Name.ValueString);
            if (attr == null) {
                context.AddError(n_Name, "Unknown attribute '" + n_Name.ValueString +"',");
                return;
            }

            n_Name.SetSymbolUsage(attr, SymbolUsageFlags.Reference);

            switch(t_Type = attr.Type) {
            case TAttributeType.String:
                {
                    if (n_Value is Token.String st) {
                        t_Value = st.ValueString;
                    }
                    else {
                        context.AddError(n_Value, "Expect string.");
                    }
                }
                break;

            case TAttributeType.Integer:
                {
                    if (n_Value is Token.Number nt && nt.isInteger()) {
                        t_Value = nt.ValueBigInt;
                    }
                    else {
                        context.AddError(n_Value, "Expect interger.");
                    }
                }
                break;

            case TAttributeType.Number:
                {
                    if (n_Value is Token.Number nt) {
                        t_Value = nt.ValueDecimal;
                    }
                    else {
                        context.AddError(n_Value, "Expect number.");
                    }
                }
                break;

            case TAttributeType.Enum: {
                    if (n_Value is TokenWithSymbol t && t.isNameOrQuotedName) {
                        var name = attr.FindName(t.ValueString);
                        if (name != null) {
                            t.SetSymbolUsage(name, SymbolUsageFlags.Reference);
                            t_Value = name.Name;
                        }
                        else {
                            context.AddError(t, "Unknown '" + t.ValueString + "'.");
                        }                }
                    else {
                        context.AddError(n_Value, "Expect name,quotedname.");
                    }
                }
                break;

            case TAttributeType.Flags:
                {
                    var names = new List<string>();

                    if (n_Value is NameList nl) {
                        foreach(var n in nl.n_Names) {
                            var name = attr.FindName(n.ValueString);
                            if (name != null) {
                                n.SetSymbolUsage(name, SymbolUsageFlags.Reference);
                                names.Add(name.Name);
                            }
                            else {
                                context.AddError(n, "Unknown '" + n.ValueString + "'.");
                            }
                        }
                    }
                    else {
                        context.AddError(n_Value, "Expect namelist.");
                    }

                    t_Value = names.ToArray();
                }
                break;
            }

        }
        public      override    void                        Emit(EmitWriter emitWriter)
        {
        }
    }
}
