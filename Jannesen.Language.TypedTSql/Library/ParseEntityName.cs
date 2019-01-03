using System;
using System.Text;

namespace Jannesen.Language.TypedTSql.Library
{
    class ParseEntityName
    {
        public          string      Server      { get; private set; }
        public          string      Database    { get; private set; }
        public          string      Schema      { get; private set; }
        public          string      Name        { get; private set; }

        public                      ParseEntityName(string fullname)
        {
            var part = new StringBuilder(64);
            var q    = false;

            int pos = 0;
            while (pos < fullname.Length) {
                char c = fullname[pos++];

                switch(c) {
                case '[':
                    if (q) {
                        if (pos < fullname.Length)
                            part.Append(fullname[pos++]);
                    }
                    else
                        q = true;
                    break;

                case ']':
                    if (q) {
                        _set(part.ToString());
                        if (pos >= fullname.Length)
                            return;

                        if (fullname[pos] != '.')
                            throw new FormatException("Invalid full-entity-name.");

                        q = false;
                    }
                    else
                        part.Append(c);
                    break;

                case '.':
                    _set(part.ToString());
                    part.Clear();
                    break;

                default:
                    part.Append(c);
                    break;
                }
            }

            if (q)
                throw new FormatException("Invalid full-entity-name.");

            _set(part.ToString());
        }

        private         void        _set(string part)
        {
            Server   = Database;
            Database = Schema;
            Schema   = Name;
            Name     = part;
        }
    }
}
