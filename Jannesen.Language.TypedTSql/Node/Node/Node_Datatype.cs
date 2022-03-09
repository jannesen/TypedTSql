using System;

namespace Jannesen.Language.TypedTSql.Node
{
    // Datatype::= Name ('(' max | INTEGER (',' INTEGER) ? ')') ?
    public class Node_Datatype: Core.AstParseNode, ISqlType
    {
        public      readonly    Boolean                     DefaultLength;
        public      readonly    Core.Token                  n_Schema;
        public      readonly    Core.TokenWithSymbol        n_Name;
        public      readonly    Core.Token                  n_Parm1;
        public      readonly    Core.Token                  n_Parm2;
        public                  DataModel.ISqlType          SqlType                 { get { return _sqlType; } }

        private                 string                      _addSchema;
        public                  DataModel.ISqlType          _sqlType;

        public                                              Node_Datatype(Core.ParserReader reader, bool defaultLength=false)
        {
            DefaultLength = defaultLength;
            n_Name = ParseName(reader);

            if (ParseOptionalToken(reader, Core.TokenID.Dot) != null) {
                n_Schema = n_Name;
                n_Name   = ParseName(reader);
            }
            else
            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                if (reader.CurrentToken.ID == Core.TokenID.Name) {
                    n_Parm1 = ParseToken(reader, "MAX");
                }
                else {
                    n_Parm1 = ParseInteger(reader);

                    if (ParseOptionalToken(reader, Core.TokenID.Comma) != null) {
                        n_Parm2 = ParseInteger(reader);
                    }
                }

                ParseToken(reader, Core.TokenID.RrBracket);
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            _sqlType = null;
            _sqlType = _transpileNode(context);
        }

        public      override    void                        Emit(Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (object.ReferenceEquals(node, n_Name) && _addSchema != null)
                    emitWriter.WriteText(Library.SqlStatic.QuoteNameIfNeeded(_addSchema) + ".");

                node.Emit(emitWriter);
            }
        }
        public                  void                        EmitNative(Core.EmitWriter emitWriter)
        {
            if ((_sqlType.TypeFlags & DataModel.SqlTypeFlags.UserType) != 0) {
                EmitCustom(emitWriter, (ew) => {
                                            ew.WriteText(_sqlType.NativeType.NativeTypeString);
                                        });
            }
            else
                this.Emit(emitWriter);
        }

        private                 DataModel.ISqlType          _transpileNode(Transpile.Context context)
        {
        // UDT with schema
            if (n_Schema != null) {
                var entityName = new DataModel.EntityName(n_Schema.ValueString, n_Name.ValueString);
                var entity = context.Catalog.GetType(entityName);
                if (entity == null) {
                    context.AddError(n_Name, "Unknown user-type '" + entityName + "'.");
                    return null;
                }

                n_Name.SetSymbolUsage(entity, DataModel.SymbolUsageFlags.Reference);
                context.CaseWarning(n_Schema, entity.EntityName.Schema);
                context.CaseWarning(n_Name,   entity.EntityName.Name);

                return entity;
            }

        // system type
            {
                var     nameLower    = n_Name.ValueString.ToLowerInvariant();
                var     systemTypeId = DataModel.SqlTypeNative.ParseSystemType(nameLower);

                if (systemTypeId != DataModel.SystemType.Unknown) {
                    var sqlType = _parseNativeType(context, systemTypeId);
                    n_Name.SetSymbolUsage(sqlType, DataModel.SymbolUsageFlags.Reference);
                    return sqlType;
                }
                else if (nameLower == "cursor")
                {
                    return new DataModel.SqlTypeCursorRef();
                }
            }

        // UDT with out schema
            {
                if (n_Parm1 != null || n_Parm2 != null)
                    throw new ArgumentException("Unknown system-sqltype '" + n_Name.ValueString + "'");

                var schema = DataModel.SqlTypeNative.SystemSchema(n_Name.ValueString);
                if (schema == null) {
                    if ((schema = context.Options.Schema) == null)
                        throw new ParseException(n_Name, "Schema not defined.");

                    _addSchema = schema;
                }

                var entityName = new DataModel.EntityName(schema, n_Name.ValueString);
                var entity = context.Catalog.GetType(entityName);
                if (entity == null) {
                    context.AddError(n_Name, "Unknown user-type '" + entityName + "'.");
                    return null;
                }

                n_Name.SetSymbolUsage(entity, DataModel.SymbolUsageFlags.Reference);
                context.CaseWarning(n_Name, entity.EntityName.Name);

                return entity;
            }
        }

        private                 DataModel.SqlTypeNative     _parseNativeType(Transpile.Context context, DataModel.SystemType systemTypeId)
        {
            if (n_Parm1 is Token.Number) context.ValidateInteger(n_Parm1, 0, 8000);
            if (n_Parm2 is Token.Number) context.ValidateInteger(n_Parm2, 0,   38);

            if (n_Parm1 == null && DefaultLength) {
                switch(systemTypeId) {
                case DataModel.SystemType.Binary:
                case DataModel.SystemType.VarBinary:
                case DataModel.SystemType.Char:
                case DataModel.SystemType.NChar:
                case DataModel.SystemType.VarChar:
                case DataModel.SystemType.NVarChar:
                    return new DataModel.SqlTypeNative(systemTypeId, maxLength:30);
                }
            }

            return DataModel.SqlTypeNative.ParseNativeType(systemTypeId, n_Parm1?.Text, n_Parm2?.Text);
        }
    }
}
