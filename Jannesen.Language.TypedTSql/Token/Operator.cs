using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class Operator: Core.Token
    {
        private                     Core.TokenID        _id;

        public      override        Core.TokenID        ID
        {
            get {
                return _id;
            }
        }

        internal                                            Operator(Core.TokenID id, Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
            _id        = id;
        }

        public      override        string                  ToString()
        {
            return Text;
        }
    }
}
