using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class Number: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.Number;
            }
        }
        public      override        Int32                   ValueInt
        {
            get {
                return int.Parse(Text);
            }
        }
        public      override        Int64                   ValueBigInt
        {
            get {
                return int.Parse(Text);
            }
        }
        public      override        decimal                 ValueDecimal
        {
            get {
                return decimal.Parse(Text, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        public      override        double                  ValueFloat
        {
            get {
                string text = Text;

                if (text[text.Length-1] == 'e' || text[text.Length-1] == 'E')
                    text = text + "0";

                return double.Parse(text, System.Globalization.NumberStyles.AllowDecimalPoint|System.Globalization.NumberStyles.AllowExponent, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        internal                                            Number(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }
    }
}
