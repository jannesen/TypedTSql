using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class Binary: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.BinaryValue;
            }
        }
        public      override        byte[]                  ValueBinary
        {
            get {
                if ((Text.Length % 2) != 0)
                    throw new InvalidOperationException("Invalid binary value.");

                byte[] rtn = new byte[(Text.Length - 2) / 2];

                for (int i = 0 ; i < rtn.Length ; ++i)
                    rtn[i] = (byte)((_charToNibble(Text[2+i*2])<<4) | _charToNibble(Text[2+i*2+1]));

                return rtn;
            }
        }

        internal                                            Binary(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        private     static          byte                    _charToNibble(char c)
        {
            if ('0' <= c && c <= '9')       return (byte)(c - '0');
            if ('A' <= c && c <= 'F')       return (byte)(c - 'A' + 10);
            if ('a' <= c && c <= 'f')       return (byte)(c - 'a' + 10);
            throw new ArgumentException("Invalid hexdecimal character '" + c + "'");
        }
    }
}
