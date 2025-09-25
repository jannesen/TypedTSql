using System;
using System.Collections.Generic;
using System.IO;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class DeclarationServiceComplexType: DeclarationObjectCode
    {
        public class Declaration: Core.AstParseNode
        {
            public      readonly        Node_ServiceEntityName              n_ServiceTypeName;
            public      readonly        DataModel.EntityName                n_EntityName;

            public                                                          Declaration(Core.ParserReader reader)
            {
                ParseToken(reader, "WEBCOMPLEXTYPE");
                n_ServiceTypeName = AddChild(new Node_ServiceEntityName(reader));
                n_EntityName = BuildEntityName(n_ServiceTypeName.n_ServiceEntitiyName, n_ServiceTypeName.n_Name.ValueString);
            }

            public      override        void                                TranspileNode(Transpile.Context context)
            {
                n_ServiceTypeName.TranspileNode(context);
            }
            public      override        void                                Emit(Core.EmitWriter emitWriter)
            {
                EmitCustom(emitWriter, (ew) => {
                                if (!ew.EmitOptions.DontEmitCustomComment)
                                    ew.WriteText("\n");

                                ew.WriteText("CREATE FUNCTION " + n_EntityName.Fullname);
                           });
            }
        }

        public      override    DataModel.SymbolType            EntityType                  { get { return DataModel.SymbolType.ServiceComplexType;  } }
        public      override    bool                            callableFromCode            { get { return true;                                     } }

        public      abstract    string                          ComplexTypeName             { get; }
        public                  Node.Expr_ResponseNode          ResponseNode                { get { return (Expr_ResponseNode)n_Statement;           } }

        public                                                  DeclarationServiceComplexType(Core.ParserReader reader)
        {
        }

        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            stringWriter.Write("IF EXISTS (SELECT * FROM sys.sysobjects WHERE [id] = object_id(");
                stringWriter.Write(Library.SqlStatic.QuoteString(EntityName.Fullname));
                stringWriter.WriteLine(") AND [type] in ('FN','IF','TF','AF', 'FS', 'FT'))");
            stringWriter.Write("    DROP FUNCTION ");
                stringWriter.WriteLine(EntityName.Fullname);
        }

        public      static      DataModel.EntityName            BuildEntityName(DataModel.EntityName serviceName, string name)
        {
            return new DataModel.EntityName(serviceName.Schema, serviceName.Name + ":ct:" + name);
        }
    }

    public class DeclarationServiceComplexTypeList: Library.ListHash<DeclarationServiceComplexType, string>
    {
        public                                          DeclarationServiceComplexTypeList(int capacity): base(capacity)
        {
        }

        protected   override    string                  ItemKey(DeclarationServiceComplexType item)
        {
            return item.ComplexTypeName;
        }
    }
}
