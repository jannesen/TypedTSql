using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class TypeDeclaration_External: TypeDeclarationWithGrant
    {
        public      override    DataModel.SymbolType            EntityType      { get { return DataModel.SymbolType.TypeExternal;   } }
        public      override    DataModel.EntityType            Entity          { get { return _entity;                             } }
        public      readonly    Core.TokenWithSymbol            n_AssemblyName;
        public      readonly    Core.Token                      n_AssemblyClass;
        public      readonly    Node_InterfaceList              n_Interfaces;

        public                  DataModel.EntityAssembly        Assembly              { get; private set; }

        private                 DataModel.EntityTypeExternal    _entity;

        public                                                  TypeDeclaration_External(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.EXTERNAL);
            ParseToken(reader, "NAME");
            n_AssemblyName = ParseName(reader);
            ParseToken(reader, Core.TokenID.Dot);
            n_AssemblyClass = Core.TokenWithSymbol.SetNoSymbol(ParseName(reader));
            n_Interfaces = new Node_InterfaceList(reader);
            ParseGrant(reader);
        }

        public      override    void                            TranspileInit(Transpile.TranspileContext transpileContext, Declaration_TYPE declaration, SourceFile sourceFile)
        {
            if ((_entity = transpileContext.Catalog.DefineTypeExternal(declaration.EntityName)) == null)
                throw new TranspileException(declaration.n_Name, "Duplicate definition of type.");

            _entity.TranspileInit(new DataModel.DocumentSpan(sourceFile.Filename, declaration));
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            var assemblyName = new DataModel.EntityName(null, n_AssemblyName.ValueString);

            Assembly = context.Catalog.GetAssembly(assemblyName);
            if (Assembly == null) {
                context.AddError(n_AssemblyName, "Unknown assembly '" + assemblyName + "'.");
                return;
            }

            n_AssemblyName.SetSymbolUsage(Assembly, DataModel.SymbolUsageFlags.Reference);
            context.CaseWarning(n_AssemblyName, Assembly.EntityName.Name);
            n_Interfaces.TranspileNode(context);
            TranspileGrant(context);
        }

        public      override    void                            Transpiled()
        {
            _entity.Transpiled(Assembly, n_AssemblyClass.ValueString, n_Interfaces.Interfaces);
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter, Declaration_TYPE type)
        {
            emitWriter.WriteText("DECLARE @user_type_id INT = (SELECT [user_type_id] FROM sys.types WHERE [schema_id] = SCHEMA_ID(");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Schema));
                emitWriter.WriteText(") AND [name]=");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Name));
                emitWriter.WriteText(");\r\n");

            emitWriter.WriteText("IF @user_type_id IS NOT NULL\r\n");

            emitWriter.WriteText("BEGIN\r\n");
                emitWriter.WriteText("    IF NOT EXISTS (");
                    emitWriter.WriteText("SELECT * FROM sys.assembly_types t INNER JOIN sys.assemblies a on a.[assembly_id]=t.assembly_id WHERE t.[user_type_id] = @user_type_id");
                    emitWriter.WriteText(" AND a.[name]=");
                    emitWriter.WriteText(Library.SqlStatic.QuoteString(n_AssemblyName.ValueString));
                    emitWriter.WriteText(" AND t.[assembly_class]=");
                    emitWriter.WriteText(Library.SqlStatic.QuoteString(n_AssemblyClass.ValueString));
                    emitWriter.WriteText(")\r\n");


                emitWriter.WriteText("        RAISERROR('External type ");
                    emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname.Replace("'", "''"));
                    emitWriter.WriteText(" is invalid, please fix manual.', 16, 1);\r\n");
            emitWriter.WriteText("END\r\n");

            emitWriter.WriteText("ELSE\r\n");

            emitWriter.WriteText("BEGIN\r\n");
                emitWriter.WriteText("    CREATE TYPE ");
                    emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname);
                    emitWriter.WriteText("\r\n");
                    emitWriter.WriteText("    EXTERNAL NAME ");
                    emitWriter.WriteText(Library.SqlStatic.QuoteName(n_AssemblyName.ValueString));
                    emitWriter.WriteText(".");
                    emitWriter.WriteText(Library.SqlStatic.QuoteName(n_AssemblyClass.ValueString));
                    emitWriter.WriteText(";\r\n");
            emitWriter.WriteText("END\r\n");
        }
    }
}
