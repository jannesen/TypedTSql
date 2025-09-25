using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class TypeDeclaration_User: TypeDeclaration
    {
        public class With: Core.AstParseNode
        {
            public      readonly    DataModel.SqlTypeFlags          n_TypeFlags;
            public      readonly    TokenWithSymbol                 n_TimeZone;

            public                  string                          t_TimeZome  { get ; private set; }

            public                                                  With(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.WITH);

                do {
                    DataModel.SqlTypeFlags  f;

                    switch(f = _parseEnum.Parse(this, reader)) {
                    case DataModel.SqlTypeFlags.CheckTSql:
                    case DataModel.SqlTypeFlags.CheckSafe:
                    case DataModel.SqlTypeFlags.CheckStrong:
                    case DataModel.SqlTypeFlags.CheckStrict:
                        n_TypeFlags = (n_TypeFlags & ~DataModel.SqlTypeFlags.CheckMode) | f;
                        break;

                    case DataModel.SqlTypeFlags.Flags:
                    case DataModel.SqlTypeFlags.RecVersion:
                        n_TypeFlags |= f;
                        break;

                    case DataModel.SqlTypeFlags.TimeZone:
                        n_TypeFlags |= f;
                        n_TimeZone = (TokenWithSymbol)ParseToken(reader, TokenID.Name, TokenID.QuotedName, TokenID.String);
                        break;
                    }
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);
            }
            public                  void                            TranspileNode(TypeDeclaration_User typeDeclaration, Transpile.Context context)
            {
                t_TimeZome = null;

                if (n_TimeZone != null) {
                    if (!typeDeclaration.NativeType.canHaveTimeZone) {
                        context.AddError(n_TimeZone, "Only smalldatetime, datetime, datetime2 can have a timezone.");
                    }
                    t_TimeZome = n_TimeZone.ValueString;
                }
            }

            private static  Core.ParseEnum<DataModel.SqlTypeFlags>  _parseEnum = new Core.ParseEnum<DataModel.SqlTypeFlags>(
                                                                                     "Type option",
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.CheckTSql,    "TYPECHECK",    "TSQL"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.CheckSafe,    "TYPECHECK",    "SAFE"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.CheckStrong,  "TYPECHECK",    "STRONG"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.CheckStrict,  "TYPECHECK",    "STRICT"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.Flags,        "FLAGS"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.RecVersion,   "RECVERSION"),
                                                                                     new Core.ParseEnum<DataModel.SqlTypeFlags>.Seq(DataModel.SqlTypeFlags.TimeZone,     "TIME",         "ZONE")
                                                                                 );
        }

        public      override    DataModel.SymbolType            EntityType      { get { return DataModel.SymbolType.TypeUser;   } }
        public      override    DataModel.EntityType            Entity          { get { return _entity;                         } }

        public      readonly    TokenWithSymbol                 n_TypeName;
        public      readonly    Core.Token                      n_Parm1;
        public      readonly    Core.Token                      n_Parm2;
        public      readonly    IExprNode                       n_DefaultValue;
        public      readonly    With                            n_With;
        public      readonly    Node_Values                     n_Values;
        public      readonly    Node_Attributes                 n_Attributes;
        public      readonly    Node_InstallInto                n_InstallInto;

        public                  DataModel.SqlTypeNative         NativeType          { get { return _nativeType; } }

        private                 DataModel.SqlTypeNative         _nativeType;
        private                 DataModel.EntityTypeUser        _entity;

        public                                                  TypeDeclaration_User(Core.ParserReader reader)
        {
            ParseToken(reader, "FROM");

            n_TypeName = ParseName(reader);
            n_Parm1 = null;
            n_Parm2 = null;

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                ParseToken(reader, Core.TokenID.LrBracket);

                switch(reader.CurrentToken.validateToken(Core.TokenID.Number, "MAX")) {
                case Core.TokenID.Number:
                    n_Parm1 = ParseInteger(reader);

                    if (reader.CurrentToken.isToken(Core.TokenID.Comma)) {
                        ParseToken(reader, Core.TokenID.Comma);
                        n_Parm2 = ParseInteger(reader);
                    }
                    break;

                case Core.TokenID.Name:
                    n_Parm1 = ParseToken(reader, "MAX");
                    break;
                }

                ParseToken(reader, Core.TokenID.RrBracket);
            }

            else if (reader.CurrentToken.isToken(Core.TokenID.DEFAULT))
            {
                ParseToken(reader);
                ParseToken(reader, Core.TokenID.Equal);
                n_DefaultValue = ParseExpression(reader);
            }

            if (reader.CurrentToken.isToken(Core.TokenID.WITH)) {
                n_With = AddChild(new With(reader));
            }

            if (reader.CurrentToken.isToken(Core.TokenID.VALUES)) {
                n_Values = AddChild(new Node_Values(reader));
            }

            if (Node_Attributes.CanParse(reader)) {
                n_Attributes = AddChild(new Node_Attributes(reader));
            }

            if (n_Values != null && reader.CurrentToken.isToken("INSTALL")) {
                n_InstallInto = AddChild(new Node_InstallInto(reader));
            }
        }

        public      override    void                            TranspileInit(Transpile.TranspileContext transpileContext, Declaration_TYPE declaration, SourceFile sourceFile)
        {
            n_DefaultValue?.TranspileNode(new Transpile.ContextInit(transpileContext, sourceFile));

            _nativeType = DataModel.SqlTypeNative.ParseNativeType(n_TypeName.ValueString, n_Parm1?.Text, n_Parm2?.Text);
            n_TypeName.SetSymbolUsage(_nativeType, DataModel.SymbolUsageFlags.Reference);

            var defaultValue = n_DefaultValue?.getConstValue(_nativeType);

            if ((_entity = transpileContext.Catalog.DefineTypeUser(declaration.EntityName)) == null)
                throw new TranspileException(declaration.n_Name, "Duplicate definition of type.");

            _entity.TranspileInit(new DataModel.DocumentSpan(sourceFile.Filename, declaration), _nativeType, defaultValue);
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Attributes?.TranspileNode(context);
            n_Values?.TranspileNode(context);
            n_InstallInto?.TranspileNode(context);
            n_With?.TranspileNode(this, context);
            n_Values?.TranspileNode(this, context);
            n_InstallInto?.TranspileNode(this, context);
        }
        public      override    void                            Transpiled()
        {
            _entity.Transpiled((n_With != null ? n_With.n_TypeFlags : DataModel.SqlTypeFlags.CheckSafe),
                               n_Values?.Fields,
                               n_Values?.ValuesRecords,
                               n_Attributes?.Attributes,
                               n_With?.t_TimeZome);
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter, Declaration_TYPE type)
        {
            emitWriter.WriteText("DECLARE @user_type_id INT = (SELECT [user_type_id] FROM sys.types WHERE [schema_id] = SCHEMA_ID(");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Schema));
                emitWriter.WriteText(") AND [name]=");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Name));
                emitWriter.WriteText(");\n");

            emitWriter.WriteText("IF @user_type_id IS NOT NULL\n");
            emitWriter.WriteText("BEGIN\n");
                emitWriter.WriteText("    IF NOT EXISTS (");
                    emitWriter.WriteText("SELECT * FROM sys.types WHERE [user_type_id] = @user_type_id");
                    emitWriter.WriteText(" AND [system_type_id]=");
                    emitWriter.WriteText(_nativeType.SystemTypeId.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    switch(_nativeType.SystemType) {
                    case DataModel.SystemType.Binary:
                    case DataModel.SystemType.VarBinary:
                    case DataModel.SystemType.Char:
                    case DataModel.SystemType.NChar:
                    case DataModel.SystemType.VarChar:
                    case DataModel.SystemType.NVarChar:
                        emitWriter.WriteText(" AND [max_length]=");
                        emitWriter.WriteText(_nativeType.MaxLength.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;

                    case DataModel.SystemType.Float:
                        emitWriter.WriteText(" AND [precision]=");
                        emitWriter.WriteText(_nativeType.Precision.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;

                    case DataModel.SystemType.Decimal:
                    case DataModel.SystemType.Numeric:
                        emitWriter.WriteText(" AND [precision]=");
                        emitWriter.WriteText(_nativeType.Precision.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        emitWriter.WriteText(" AND [scale]=");
                        emitWriter.WriteText(_nativeType.Scale.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    }

                    emitWriter.WriteText(")\n");

                emitWriter.WriteText("        RAISERROR('User defined type ");
                    emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname.Replace("'", "''"));
                    emitWriter.WriteText(" is invalid, please fix manual.', 16, 1);\n");
            emitWriter.WriteText("END\n");

            emitWriter.WriteText("ELSE\n");

            emitWriter.WriteText("BEGIN\n");
                emitWriter.WriteText("    CREATE TYPE ");
                    emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname);
                    emitWriter.WriteText("\n");
                emitWriter.WriteText("    FROM " + _nativeType.NativeTypeString);
                    emitWriter.WriteText(";\n");
            emitWriter.WriteText("END\n");
        }
        public      override    bool                            EmitInstallInto(EmitContext emitContext, int step)
        {
            if (n_InstallInto != null && n_Values != null)
                return n_InstallInto.EmitInstallInto(emitContext, n_Values.ValuesRecords, step);

            return true;
        }
    }
}
