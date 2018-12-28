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

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            Core.Token[]        peek = reader.Peek(8);

            return (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.DoubleColon && peek[2].isNameOrQuotedName)
                || (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot         && peek[2].isNameOrQuotedName && peek[3].ID == Core.TokenID.DoubleColon && peek[4].isNameOrQuotedName)
                || (peek[0].isNameOrQuotedName && peek[1].ID == Core.TokenID.Dot         && peek[2].isNameOrQuotedName && peek[3].ID == Core.TokenID.Dot         && peek[4].isNameOrQuotedName     && peek[5].ID == Core.TokenID.DoubleColon    && peek[6].isNameOrQuotedName);
        }
        public                                                  Expr_TypeStatic(Core.ParserReader reader)
        {
            n_EntityName = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.UserDataType));
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

                switch(n_EntityName.Entity.Type)
                {
                case DataModel.SymbolType.TypeUser:
                    _expressionType = ExprType.Const;
                    _transpile_userdatatype(context);
                    break;

                case DataModel.SymbolType.TypeExternal:
                    _expressionType = ExprType.Complex;
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
            if (_expressionType == ExprType.Const) {
                EmitCustom(emitWriter, (ew) => {
                                            ew.WriteValue(_constValue);
                                       });
            }
            else
                base.Emit(emitWriter);
        }

        private                 void                            _transpile_userdatatype(Transpile.Context context)
        {
            if (n_Arguments != null)
                throw new ErrorException("UDT has no methods.");

            var entityType = (DataModel.EntityType)(n_EntityName.Entity);

            if (entityType.Values == null) {
                context.AddError(n_EntityName, "Type has no static values.");
                return;
            }

            if (!entityType.Values.TryGetValue(n_StaticName.ValueString, out DataModel.ValueRecord valueRecord)) {
                context.AddError(n_StaticName, "Unknown static value.");
                return;
            }

            n_StaticName.SetSymbol(valueRecord);
            context.CaseWarning(n_StaticName, valueRecord.Name);
            _valueFlags = (valueRecord.Value == null ? DataModel.ValueFlags.NULL|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Const);
            _sqlType    = ((DataModel.EntityType)(n_EntityName.Entity));
            _constValue = valueRecord.Value;
        }
        private                 void                            _transpile_external(Transpile.Context context)
        {
            var sqlType = ((DataModel.EntityType)(n_EntityName.Entity));

            _valueFlags = DataModel.ValueFlags.Nullable;

            if (n_Arguments == null)
                _sqlType = Validate.Property(sqlType.Interfaces, true, n_StaticName);
            else
                _sqlType = Validate.Method(sqlType.Interfaces, true, n_StaticName, n_Arguments.n_Expressions);
        }
    }
}
