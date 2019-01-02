using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_Interface: Core.AstParseNode
    {
        public      readonly    DataModel.SymbolType        n_Type;
        public      readonly    Core.TokenWithSymbol        n_Name;
        public      readonly    Node_ParameterList          n_Parameters;
        public      readonly    Node_Datatype               n_Returns;

        public                  DataModel.Interface         Interface       { get; private set; }

        public                                              Node_Interface(Core.ParserReader reader)
        {
            var propertymethod = ParseToken(reader, TokenID.PROPERTY, TokenID.METHOD).ID;

            if (ParseOptionalToken(reader, TokenID.STATIC) != null) {
                switch(propertymethod) {
                case TokenID.METHOD:    n_Type = DataModel.SymbolType.ExternalStaticMethod;     break;
                case TokenID.PROPERTY:  n_Type = DataModel.SymbolType.ExternalStaticProperty;   break;
                }
            }
            else {
                switch(propertymethod) {
                case TokenID.METHOD:    n_Type = DataModel.SymbolType.ExternalMethod;           break;
                case TokenID.PROPERTY:  n_Type = DataModel.SymbolType.ExternalProperty;         break;
                }
            }

            n_Name       = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.Name, Core.TokenID.QuotedName);
            n_Parameters = new Node_ParameterList(reader, Node_SqlParameter.InterfaceType.Interface);

            if (reader.CurrentToken.isToken("VOID"))
                return;

            n_Returns = new Node_Datatype(reader);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            n_Parameters?.TranspileNode(context);
            n_Returns?.TranspileNode(context);

            Interface = new DataModel.Interface(n_Type, n_Name.ValueString, n_Name, n_Parameters?.t_Parameters, n_Returns?.SqlType);
            n_Name.SetSymbol(Interface);
        }
        public      override    void                        Emit(EmitWriter emitWriter)
        {
        }
    }
}
