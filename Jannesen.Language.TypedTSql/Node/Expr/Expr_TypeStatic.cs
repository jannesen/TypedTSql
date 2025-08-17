using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    // Expression_Function:
    //      : Objectname '(' Expression ( ',' Expression )* ')'
    public class Expr_TypeStatic: ExprCalculation
    {
        public      readonly    Node_EntityNameReference        n_EntityName;
        public      readonly    Core.TokenWithSymbol            n_StaticName;
        public      readonly    Expr_Collection                 n_Arguments;

        public      override    DataModel.ValueFlags            ValueFlags              { get { return _valueFlags;         } }
        public      override    DataModel.ISqlType              SqlType                 { get { return _sqlType;            } }
        public      override    ExprType                        ExpressionType          { get { return _expressionType;     } }
        public      override    bool                            NoBracketsNeeded        { get { return true;                } }

        private                 ExprType                        _expressionType;
        private                 DataModel.ValueFlags            _valueFlags;
        private                 DataModel.ISqlType              _sqlType;
        private                 object                          _constValue;
        private                 CustomEmitor                    _customEmitor;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(8);

            return (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.DoubleColon && peek[2].isNameOrQuotedName)
                || (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot         && peek[2].isNameOrQuotedName && peek[3].ID == Core.TokenID.DoubleColon && peek[4].isNameOrQuotedName)
                || (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot         && peek[2].isNameOrQuotedName && peek[3].ID == Core.TokenID.Dot         && peek[4].isNameOrQuotedName     && peek[5].ID == Core.TokenID.DoubleColon    && peek[6].isNameOrQuotedName);
        }
        public                                                  Expr_TypeStatic(Core.ParserReader reader)
        {
            n_EntityName = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.UserDataType, DataModel.SymbolUsageFlags.Reference));
            ParseToken(reader, Core.TokenID.DoubleColon);
            n_StaticName = ParseName(reader);

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                n_Arguments = AddChild(new Expr_Collection(reader, false));
            }

            _expressionType = ExprType.NeedsTranspile;
        }

        public      override    object                          ConstValue()
        {
            if (n_EntityName.Entity.Type == DataModel.SymbolType.TypeUser)
                return _constValue;

            return new TranspileException(this, "Can't calculate constant value.");
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _expressionType = ExprType.NeedsTranspile;
            _valueFlags = DataModel.ValueFlags.Error;
            _sqlType    = null;
            _constValue = null;

            try {
                n_EntityName.TranspileNode(context);
                n_Arguments?.TranspileNode(context);

                if (n_EntityName.Entity == null)
                    return ;

                switch(n_EntityName.Entity.Type) {
                case DataModel.SymbolType.TypeUser:
                    _transpile_userdatatype(context);
                    break;

                case DataModel.SymbolType.TypeExternal:
                    _transpile_external(context);
                    break;

                default:
                    throw new ErrorException("Invalid entity-type.");
                }
            }
            catch(Exception err) {
                _valueFlags = DataModel.ValueFlags.Error;
                _sqlType    = null;
                context.AddError(this, err);
            }
        }
        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            if (_customEmitor != null) {
                EmitCustom(emitWriter, _customEmitor);
            }
            else {
                base.Emit(emitWriter);
            }
        }

        private                 void                            _transpile_userdatatype(Transpile.Context context)
        {
            var entityType = (DataModel.EntityTypeUser)(n_EntityName.Entity);

            if (n_Arguments == null) {
                if (entityType.Values == null) {
                    context.AddError(n_EntityName, "Type has no static values.");
                    return;
                }

                if (!entityType.Values.TryGetValue(n_StaticName.ValueString, out DataModel.ValueRecord valueRecord)) {
                    context.AddError(n_StaticName, "Unknown static value.");
                    return;
                }

                n_StaticName.SetSymbolUsage(valueRecord, DataModel.SymbolUsageFlags.Reference);
                context.CaseWarning(n_StaticName, valueRecord.Name);
                _expressionType = ExprType.Const;
                _valueFlags = (valueRecord.Value == null ? DataModel.ValueFlags.NULL|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Const);
                _sqlType    = ((DataModel.EntityType)(n_EntityName.Entity));
                _constValue = valueRecord.Value;

                _customEmitor = _emitConstValue;
            }
            else {
                if (entityType.NativeType.canHaveTimeZone && n_StaticName.ValueString.ToLower() == "now") {
                    if (entityType.TimeZone == null) {
                        context.AddError(n_StaticName, "Unknown time zone.");
                        return;
                    }

                    switch(entityType.TimeZone.ToLower()) {
                    case "utc":
                    case "local":
                        break;
                    default:
                        context.AddError(n_StaticName, "Unsupport time zone '" + entityType.TimeZone + "'.");
                        return;
                    }

                    n_StaticName.SetSymbolUsage(entityType.NowSymbol, DataModel.SymbolUsageFlags.Reference);
                    _expressionType = ExprType.Complex;
                    _valueFlags = DataModel.ValueFlags.Computed;
                    _sqlType    = ((DataModel.EntityType)(n_EntityName.Entity));

                    _customEmitor = _emitNow;
                }
                else {
                    throw new ErrorException("UDT has no methods.");
                }
            }
        }
        private                 void                            _transpile_external(Transpile.Context context)
        {
            _expressionType = ExprType.Complex;
            _valueFlags = DataModel.ValueFlags.Nullable;

            var sqlType = ((DataModel.EntityType)(n_EntityName.Entity));

            if (n_Arguments == null)
                _sqlType = Validate.Property(sqlType.Interfaces, true, n_StaticName);
            else
                _sqlType = Validate.Method(sqlType.Interfaces, true, n_StaticName, n_Arguments.n_Expressions);
        }

        private                 void                            _emitConstValue(Core.EmitWriter emitWriter)
        {
            emitWriter.WriteValue(_constValue);
        }
        private                 void                            _emitNow(Core.EmitWriter emitWriter)
        {
            var entityType = (DataModel.EntityTypeUser)(n_EntityName.Entity);

            switch(entityType.NativeType.SystemType) {
            case DataModel.SystemType.SmallDateTime:
                switch(entityType.TimeZone.ToLower()) {
                case "utc":     emitWriter.WriteText("CONVERT(SMALLDATETIME,GETUTCDATE())");    break;
                case "local":   emitWriter.WriteText("CONVERT(SMALLDATETIME,GETDATE())");       break;
                default:    throw new NotImplementedException("_emitNow(SmallDateTime, timezone)");
                }
                break;

            case DataModel.SystemType.DateTime:
                switch(entityType.TimeZone.ToLower()) {
                case "utc":     emitWriter.WriteText("GETUTCDATE()");                           break;
                case "local":   emitWriter.WriteText("GETDATE()");                              break;
                default:    throw new NotImplementedException("_emitNow(DateTime, timezone)");
                }
                break;

            case DataModel.SystemType.DateTime2:
                emitWriter.WriteText("CONVERT(");
                emitWriter.WriteText(_sqlType.NativeType.NativeTypeString);
                switch(entityType.TimeZone.ToLower()) {
                case "utc":   emitWriter.WriteText(",SYSUTCDATETIME())");  break;
                case "local": emitWriter.WriteText(",SYSDATETIME())");     break;
                default:    throw new NotImplementedException("_emitNow(DateTime, timezone)");
                }
                break;
            }
        }
    }
}
