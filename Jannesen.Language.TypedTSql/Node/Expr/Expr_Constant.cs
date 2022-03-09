using System;
using Jannesen.Language.TypedTSql.Transpile;

namespace Jannesen.Language.TypedTSql.Node
{
    // Constant ::= String
    //            | Number
    //            | Binary
    //            | sign Number
    //            | NULL
    public class Expr_Constant: Expr
    {
        public      readonly    Core.Token              n_Sign;
        public      readonly    Core.Token              n_Value;

        public      override    DataModel.ValueFlags    ValueFlags          { get { return _valueFlags;         } }
        public      override    DataModel.ISqlType      SqlType             { get { return _sqlType;            } }
        public      override    string                  CollationName       { get { return null;                } }
        public      override    ExprType                ExpressionType      { get { return ExprType.Const;      } }
        public      override    bool                    NoBracketsNeeded    { get { return true;                } }

        private                 DataModel.ValueFlags    _valueFlags;
        private                 DataModel.ISqlType      _sqlType;
        private                 object                  _constValue;

        public  new static      bool                    CanParse(Core.ParserReader reader)
        {
            Core.Token[]    peek = reader.Peek(2);

            return (peek[0].isToken(Core.TokenID.Number, Core.TokenID.String, Core.TokenID.BinaryValue, Core.TokenID.NULL))
                || (peek[0].isToken(Core.TokenID.Plus, Core.TokenID.Minus                                           ) && peek[1].isToken(Core.TokenID.Number));
        }

        public                                          Expr_Constant(Core.ParserReader reader)
        {
            switch((n_Value = ParseToken(reader, Core.TokenID.Number, Core.TokenID.String, Core.TokenID.BinaryValue, Core.TokenID.NULL, Core.TokenID.Plus, Core.TokenID.Minus)).ID) {
            case Core.TokenID.Plus:
            case Core.TokenID.Minus:
                n_Sign = n_Value;
                n_Value = ParseToken(reader, Core.TokenID.Number);
                break;
            }
        }

        public      override    object                  ConstValue()
        {
            return _constValue;
        }
        public      override    void                    TranspileNode(Context context)
        {
            _valueFlags = DataModel.ValueFlags.Error;
            _sqlType    = null;
            _constValue = null;

            try {
                switch(n_Value.ID) {
                case Core.TokenID.Number:           _transpile_Number();                    break;
                case Core.TokenID.String:           _transpile_String();                    break;
                case Core.TokenID.BinaryValue:      _transpile_BinaryValue();               break;

                case Core.TokenID.NULL:
                    _valueFlags = DataModel.ValueFlags.NULL|DataModel.ValueFlags.Nullable;
                    _sqlType    = DataModel.SqlTypeNative.Int;
                    break;

                default:                            throw new InvalidOperationException("Invalid constant token.");
                }
            }
            catch(Exception) {
                _valueFlags = DataModel.ValueFlags.Error;
                _sqlType    = null;
                _constValue = null;
                context.AddError(this, "Invalid constante.");
            }
        }

        private                 void                    _transpile_Number()
        {
            string  text      = n_Value.Text;
            int     pos       = 0;
            int     digits    = 0;
            int     scale     = -1;
            bool    exponent  = false;

            while (pos < text.Length && _isDigit(text[pos]))
                ++pos;

            digits = pos;

            if (pos < text.Length && text[pos] == '.') {
                ++pos;

                while (pos < text.Length && _isDigit(text[pos]))
                    ++pos;

                scale = pos - (digits + 1);
            }

            if (pos < text.Length && (text[pos] == 'e' || text[pos] == 'E')) {
                exponent = true;
            }

            if (exponent) {
                var constValue = ((Token.Number)n_Value).ValueFloat;

                _valueFlags = DataModel.ValueFlags.Const;
                _sqlType    = DataModel.SqlTypeNative.Float;
                _constValue = (n_Sign != null && n_Sign.ID == Core.TokenID.Minus) ? -constValue : constValue;
            }
            else
            if (scale >= 0) {
                if ((digits + scale) >  38)
                    throw new InvalidOperationException("Tomany digits in const numeric value.");

                var constValue = ((Token.Number)n_Value).ValueDecimal;

                _valueFlags = DataModel.ValueFlags.Const;
                _sqlType    = new DataModel.SqlTypeNative(DataModel.SystemType.Numeric, precision:(byte)(digits + scale), scale:(byte)scale);
                _constValue = (n_Sign != null && n_Sign.ID == Core.TokenID.Minus) ? -constValue : constValue;
            }
            else {
                var constValue = n_Value.ValueBigInt;

                if (constValue > Int32.MaxValue) {
                    _valueFlags = DataModel.ValueFlags.Const;
                    _sqlType    = DataModel.SqlTypeNative.BigInt;
                    _constValue = (n_Sign != null && n_Sign.ID == Core.TokenID.Minus) ? -constValue : constValue;
                }
                else {
                    _valueFlags = DataModel.ValueFlags.Const;
                    _sqlType    = DataModel.SqlTypeNative.Int;
                    _constValue = (n_Sign != null && n_Sign.ID == Core.TokenID.Minus) ? -(Int32)constValue : (Int32)constValue;
                }
            }
        }
        private                 void                    _transpile_String()
        {
            var constValue = n_Value.ValueString;

            _valueFlags = DataModel.ValueFlags.Const;
            _sqlType    = (n_Value.Text[0] == 'N')
                                ? constValue.Length < 4000 ? new DataModel.SqlTypeNative(DataModel.SystemType.NChar, maxLength:(short)(constValue.Length))
                                                           : new DataModel.SqlTypeNative(DataModel.SystemType.NVarChar, maxLength:-1)
                                : constValue.Length < 8000 ? new DataModel.SqlTypeNative(DataModel.SystemType.Char, maxLength:(short)(constValue.Length))
                                                           : new DataModel.SqlTypeNative(DataModel.SystemType.VarChar, maxLength:-1);
            _constValue = constValue;
        }
        private                 void                    _transpile_BinaryValue()
        {
            var constValue = n_Value.ValueBinary;

            _valueFlags = DataModel.ValueFlags.Const;
            _sqlType    = new DataModel.SqlTypeNative(DataModel.SystemType.VarBinary, maxLength:(short)(constValue.Length < 8000 ? constValue.Length : -1));
            _constValue = constValue;
        }
        private     static      bool                    _isDigit(int c)
        {
            return ('0' <= c && c <= '9');
        }
    }
}
