using System;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class ComplexType: LTTSQL.Core.AstParseNode, LTTSQL.Node.ISqlType
    {
        public      readonly    LTTSQL.Core.TokenWithSymbol         n_Name;

        public                  LTTSQL.DataModel.EntityName         EntityName      { get; private set; }
        public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return WebComplexType?.ReceivesSqlType; } }

        public                  Node.WEBCOMPLEXTYPE                 WebComplexType  { get; private set; }

        public      static      bool                                CanParse(LTTSQL.Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.DoubleColon);
        }
        public                                                      ComplexType(LTTSQL.Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.DoubleColon);
            n_Name = ParseName(reader);
        }

        public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
        {
            WebComplexType = null;

            var name                  = n_Name.ValueString;
            var complexTypeEntityName = LTTSQL.Node.DeclarationServiceComplexType.BuildEntityName(context.GetDeclarationObject<LTTSQL.Node.DeclarationServiceMethod>().ServiceName, name);
            var webComplexTypeEntity  = (context.Catalog.GetObject(complexTypeEntityName, false) as DataModel.EntityObjectCode);

            if (!(webComplexTypeEntity?.DeclarationObjectCode is Node.WEBCOMPLEXTYPE webComplexType)) {
                context.AddError(n_Name, "Unknown WEBCOMPLEXTYPE '" + name + "'.");
                return;
            }

            n_Name.SetSymbolUsage(webComplexType.Entity, DataModel.SymbolUsageFlags.Reference);
            context.CaseWarning(n_Name, webComplexType.ComplexTypeName);

            WebComplexType = webComplexType;
        }

        public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (object.ReferenceEquals(c, n_Name))
                    emitWriter.WriteText(SqlType.ToSql());
                else
                if (c.isWhitespaceOrComment)
                    c.Emit(emitWriter);
            }
        }
    }
}
