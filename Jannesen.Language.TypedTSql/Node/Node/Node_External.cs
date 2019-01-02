using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_External: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol            n_AssemblyName;
        public      readonly    Core.TokenWithSymbol            n_ClassName;
        public      readonly    Core.TokenWithSymbol            n_MethodName;
        public                  DataModel.EntityAssembly        t_Assembly              { get; private set; }


        public                                                  Node_External(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.EXTERNAL);
            ParseToken(reader, "NAME");
            n_AssemblyName = ParseName(reader);
            ParseToken(reader, Core.TokenID.Dot);
            n_ClassName = Core.TokenWithSymbol.SetNoSymbol(ParseName(reader));
            ParseToken(reader, Core.TokenID.Dot);
            n_MethodName = Core.TokenWithSymbol.SetNoSymbol(ParseName(reader));
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            var assemblyName = new DataModel.EntityName(null, n_AssemblyName.ValueString);

            t_Assembly     = context.Catalog.GetAssembly(assemblyName);
            if (t_Assembly == null) {
                context.AddError(n_AssemblyName, "Unknown assembly '" + assemblyName + "'.");
                return;
            }

            n_AssemblyName.SetSymbol(t_Assembly);
            context.CaseWarning(n_AssemblyName, t_Assembly.EntityName.Name);
        }
    }
}
