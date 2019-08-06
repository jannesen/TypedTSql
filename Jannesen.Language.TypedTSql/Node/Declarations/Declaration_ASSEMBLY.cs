using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    // https://msdn.microsoft.com/en-us/library/ms189524.aspx
    //      CREATE ASSEMBLY assembly_name
    //      [ AUTHORIZATION owner_name ]
    //      FROM { <client_assembly_specifier> | <assembly_bits> [ ,...n ] }
    //      [ WITH PERMISSION_SET = { SAFE | EXTERNAL_ACCESS | UNSAFE } ]
    //      [ ; ]
    [DeclarationParser(Core.TokenID.ASSEMBLY)]
    public class Declaration_ASSEMBLY: DeclarationEntity
    {
        public      override    DataModel.SymbolType            EntityType
        {
            get {
                return DataModel.SymbolType.Assembly;
            }
        }
        public      override    DataModel.EntityName            EntityName
        {
            get {
                return new DataModel.EntityName(null, n_Name.ValueString);
            }
        }
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    Core.Token                      n_Autorisation;
        public      readonly    BuildIn.Func.FILEBINARY         n_Image;
        public      readonly    Core.Token                      n_Permissingset;
        public      override    bool                            callableFromCode        { get { return false; } }

        public                  DataModel.EntityAssembly        EntityAssembly          { get; private set; }

        public                                                  Declaration_ASSEMBLY(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.ASSEMBLY);
            n_Name = ParseName(reader);

            if (ParseOptionalToken(reader, Core.TokenID.AUTHORIZATION) != null) {
                n_Autorisation = ParseName(reader);
            }

            ParseToken(reader, Core.TokenID.FROM);

            switch(reader.CurrentToken.validateToken(Core.TokenID.String, "FILEBINARY")) {
            case Core.TokenID.String:
                reader.AddError(new ParseException(reader.CurrentToken, "Assembly declaration only supported with filebinary."));
                ParseToken(reader);
                break;

            case Core.TokenID.Name:
                n_Image = AddChild(new BuildIn.Func.FILEBINARY(BuildIn.Catalog.ScalarFunctions.GetValue("FILEBINARY"), reader));
                break;
            }

            if (ParseOptionalToken(reader, Core.TokenID.WITH) != null) {
                ParseToken(reader, "PERMISSION_SET");
                ParseToken(reader, Core.TokenID.Equal);
                n_Permissingset = ParseToken(reader, "SAFE", "EXTERNAL_ACCESS", "UNSAFE");
            }
        }

        public      override    void                            TranspileInit(Transpiler transpiler, GlobalCatalog catalog, SourceFile sourceFile)
        {
            Transpiled             = false;
            _declarationTranspiled = false;

            if ((EntityAssembly = catalog.DefineAssembly(EntityName)) == null)
                throw new TranspileException(n_Name, "Duplicate definition of assembly.");

            EntityAssembly.TranspileInit(new DataModel.DocumentSpan(sourceFile.Filename, this));
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Name.SetSymbol(EntityAssembly);
            EntityAssembly.Transpiled();
            n_Image.TranspileNode(context);

            _declarationTranspiled = true;
            Transpiled = true;
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            emitWriter.WriteText("DECLARE @image VARBINARY(max)=");
                emitWriter.WriteValue(n_Image.BinaryData);
                emitWriter.WriteText("\r\n");
            emitWriter.WriteText("DECLARE @cur VARBINARY(max)=(SELECT f.[content] FROM sys.assemblies a inner join sys.assembly_files f ON f.[assembly_id] = a.[assembly_id] AND f.[file_id] = 1 WHERE a.[name]=" + Library.SqlStatic.QuoteString(n_Name.ValueString) + ")\r\n");
            emitWriter.WriteText("IF @cur is null\r\n");
            emitWriter.WriteText("BEGIN\r\n");
                if (n_Permissingset != null && n_Permissingset.ValueString.ToUpperInvariant() == "SAFE")
                    emitWriter.WriteText("    ALTER DATABASE CURRENT SET TRUSTWORTHY ON\r\n");
                emitWriter.WriteText("    CREATE ASSEMBLY " + Library.SqlStatic.QuoteName(n_Name.ValueString) + "\r\n");
                emitWriter.WriteText("      AUTHORIZATION " + Library.SqlStatic.QuoteName(n_Autorisation != null ? n_Autorisation.ValueString : "dbo") + "\r\n");
                emitWriter.WriteText("               FROM @image\r\n");
                if (n_Permissingset != null) emitWriter.WriteText("               WITH PERMISSION_SET=" + n_Permissingset.ValueString + "\r\n");
            emitWriter.WriteText("END\r\n");
            emitWriter.WriteText("ELSE IF @cur <> @image\r\n");
            emitWriter.WriteText("BEGIN\r\n");
                emitWriter.WriteText("    ALTER ASSEMBLY " + Library.SqlStatic.QuoteName(n_Name.ValueString) + "\r\n");
                emitWriter.WriteText("              FROM @image\r\n");
                if (n_Permissingset != null) emitWriter.WriteText("              WITH PERMISSION_SET=" + n_Permissingset.ValueString + "\r\n");
            emitWriter.WriteText("END\r\n");
        }

        public      override    Core.IAstNode                   GetNameToken()
        {
            return n_Name;
        }
        public      override    string                          CollapsedName()
        {
            return "assembly " + n_Name;
        }
    }
}
