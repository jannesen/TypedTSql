using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.Logic
{
    public static class Calculator
    {
        public  static  object      Calculate(Core.TokenID operation, object value1, object value2)
        {
            if (value1 is Int32) {
                if (value2 is Int32)        return _calculate(operation, Convert.ToInt32  ((Int32)value1), Convert.ToInt32  ((Int32)  value2));
                if (value2 is Int64)        return _calculate(operation, Convert.ToInt64  ((Int32)value1), Convert.ToInt64  ((Int64)  value2));
                if (value2 is decimal)      return _calculate(operation, Convert.ToDecimal((Int32)value1), Convert.ToDecimal((decimal)value2));
                if (value2 is double)       return _calculate(operation, Convert.ToDouble ((Int32)value1), Convert.ToDouble ((double) value2));
            }

            if (value1 is Int64) {
                if (value2 is Int32)        return _calculate(operation, Convert.ToInt64  ((Int64)value1), Convert.ToInt64  ((Int32)  value2));
                if (value2 is Int64)        return _calculate(operation, Convert.ToInt64  ((Int64)value1), Convert.ToInt64  ((Int64)  value2));
                if (value2 is decimal)      return _calculate(operation, Convert.ToDecimal((Int64)value1), Convert.ToDecimal((decimal)value2));
                if (value2 is double)       return _calculate(operation, Convert.ToDouble ((Int64)value1), Convert.ToDouble ((double) value2));
            }

            if (value1 is decimal) {
                if (value2 is Int32)        return _calculate(operation, Convert.ToDecimal((decimal)value1), Convert.ToDecimal((Int32)  value2));
                if (value2 is Int64)        return _calculate(operation, Convert.ToDecimal((decimal)value1), Convert.ToDecimal((Int64)  value2));
                if (value2 is decimal)      return _calculate(operation, Convert.ToDecimal((decimal)value1), Convert.ToDecimal((decimal)value2));
                if (value2 is double)       return _calculate(operation, Convert.ToDouble ((decimal)value1), Convert.ToDouble ((double) value2));
            }

            if (value1 is double) {
                if (value2 is Int32)        return _calculate(operation, Convert.ToDouble((double)value1), Convert.ToDouble((Int32)  value2));
                if (value2 is Int64)        return _calculate(operation, Convert.ToDouble((double)value1), Convert.ToDouble((Int64)  value2));
                if (value2 is decimal)      return _calculate(operation, Convert.ToDouble((double)value1), Convert.ToDouble((decimal)value2));
                if (value2 is double)       return _calculate(operation, Convert.ToDouble((double)value1), Convert.ToDouble((double) value2));
            }

            if (value1 is string) {
                if (value2 is string) return _calculate(operation, (string)value1, (string)value2);
            }

            throw new InvalidOperationException();
        }

        private static  Int32       _calculate(Core.TokenID operation, Int32 value1, Int32 value2)
        {
            switch(operation) {
            case Core.TokenID.Plus:         return value1 + value2;
            case Core.TokenID.Minus:        return value1 - value2;
            case Core.TokenID.BitAnd:       return value1 & value2;
            case Core.TokenID.BitOr:        return value1 | value2;
            case Core.TokenID.BitXor:       return value1 ^ value2;
            case Core.TokenID.Star:         return value1 * value2;
            case Core.TokenID.Divide:       return value1 / value2;
            case Core.TokenID.Module:       return value1 % value2;
            default:                        throw new InvalidOperationException();
            }
        }
        private static  Int64       _calculate(Core.TokenID operation, Int64 value1, Int64 value2)
        {
            switch(operation) {
            case Core.TokenID.Plus:         return value1 + value2;
            case Core.TokenID.Minus:        return value1 - value2;
            case Core.TokenID.BitAnd:       return value1 & value2;
            case Core.TokenID.BitOr:        return value1 | value2;
            case Core.TokenID.BitXor:       return value1 ^ value2;
            case Core.TokenID.Star:         return value1 * value2;
            case Core.TokenID.Divide:       return value1 / value2;
            case Core.TokenID.Module:       return value1 % value2;
            default:                        throw new InvalidOperationException();
            }
        }
        private static  decimal     _calculate(Core.TokenID operation, decimal value1, decimal value2)
        {
            switch(operation) {
            case Core.TokenID.Plus:         return value1 + value2;
            case Core.TokenID.Minus:        return value1 - value2;
            case Core.TokenID.Star:         return value1 * value2;
            case Core.TokenID.Divide:       return value1 / value2;
            default:                        throw new InvalidOperationException();
            }
        }
        private static  double      _calculate(Core.TokenID operation, double value1, double value2)
        {
            switch(operation) {
            case Core.TokenID.Plus:         return value1 + value2;
            case Core.TokenID.Minus:        return value1 - value2;
            case Core.TokenID.Star:         return value1 * value2;
            case Core.TokenID.Divide:       return value1 / value2;
            default:                        throw new InvalidOperationException();
            }
        }
        private static  string      _calculate(Core.TokenID operation, string value1, string value2)
        {
            switch(operation) {
            case Core.TokenID.Plus:         return value1 + value2;
            default:                        throw new InvalidOperationException();
            }
        }
    }
}
