using System;
using System.Collections.Generic;
using System.Text;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityName: IEquatable<EntityName>
    {
        public                  string              Database        { get; private set; }
        public                  string              Schema          { get; private set; }
        public                  string              Name            { get; private set; }

        private                 string              _fullname;
        private                 int?                _hashcode;

        public                  string              Fullname
        {
            get {
                if (_fullname == null) {
                    string fullname = Library.SqlStatic.QuoteName(Name);
                    if (Schema   != null)   fullname = Library.SqlStatic.QuoteNameIfNeeded(Schema)   + "." + fullname;
                    if (Database != null)   fullname = Library.SqlStatic.QuoteNameIfNeeded(Database) + "." + fullname;

                    _fullname = fullname;
                }

                return _fullname;
            }
        }
        public                  string              SchemaName
        {
            get {
                return Library.SqlStatic.QuoteNameIfNeeded(Schema) + "." + Library.SqlStatic.QuoteName(Name);
            }
        }

        public                                      EntityName(string database, string schema, string name)
        {
            if (name == null)
                throw new ArgumentException("Argument 'name' is null");

            Database = database;
            Schema   = schema;
            Name     = name;
        }
        public                                      EntityName(string schema, string name)
        {
            if (name == null)
                throw new ArgumentException("Argument 'name' is null");

            Database = null;
            Schema   = schema;
            Name     = name;
        }

        public                  string              GetRelativeName(string schema)
        {
            return (Database == null && Schema == schema) ? Library.SqlStatic.QuoteName(Name) : _fullname;
        }

        public      static      EntityName          Parse(string fullname)
        {
            var   parts = new List<string>();
            var   s     = new StringBuilder();
            var   quote = false;

            for (int p = 0 ; p < fullname.Length ; ++p) {
                char c = fullname[p];

                switch(c) {
                case '[':
                    if (quote) {
                        if (p >= fullname.Length - 1)
                            throw new ArgumentException("Invalid sql fullname '" + fullname + "'.");

                        c = fullname[++p];
                        goto add;
                    }

                    quote = true;
                    break;

                case ']':
                    if (!quote)
                        goto add;

                    quote = false;
                    break;

                case '.':
                    if (quote)
                        goto add;

                    parts.Add(s.ToString());
                    s.Clear();
                    break;

                default:
add:                s.Append(c);
                    break;
                }
            }

            if (quote)
                throw new FormatException("Invalid fullname '" + fullname + "'.");

            parts.Add(s.ToString());

            switch(parts.Count) {
            case 2:     return new EntityName(parts[0], parts[1]);
            case 3:     return new EntityName(parts[0], parts[1], parts[2]);
            default:    throw new FormatException("Invalid fullname '" + fullname + "'."); 
            }
        }
        public      static      int                 Compare(EntityName n1, EntityName n2)
        {
            int     i;

            if (n1 is null)
                return (n1 == null) ? 0 : -1;

            if (n2 is null)
                return 1;

            if ((i = _stringCompare(n1.Database, n2.Database)) != 0)
                return i;

            if ((i = _stringCompare(n1.Schema,   n2.Schema)) != 0)
                return i;

            return _stringCompare(n1.Name,     n2.Name);
        }
        public      static      bool                operator == (EntityName n1, EntityName n2)
        {
            if (object.ReferenceEquals(n1, n2))
                return true;

            if (n1 is null || n2 is null)
                return false;

            return _stringCompare(n1.Database, n2.Database) == 0 &&
                   _stringCompare(n1.Schema  , n2.Schema  ) == 0 &&
                   _stringCompare(n1.Name    , n2.Name    ) == 0;
        }
        public      static      bool                operator != (EntityName n1, EntityName n2)
        {
            return !(n1 == n2);
        }
        public      override    int                 GetHashCode()
        {
            if (!_hashcode.HasValue) {
                _hashcode = Name.ToUpperInvariant().GetHashCode() ^
                            (Schema   != null ? Schema.ToUpperInvariant().GetHashCode() : 0) ^
                            (Database != null ? Database.ToUpperInvariant().GetHashCode() : 0);
            }

            return _hashcode.Value;
        }
        public                  bool                Equals(EntityName other)
        {
            if (!(other is null)) {
                if (object.ReferenceEquals(this, other))
                    return true;

                return _stringCompare(this.Database, other.Database) == 0 &&
                       _stringCompare(this.Schema  , other.Schema  ) == 0 &&
                       _stringCompare(this.Name    , other.Name    ) == 0;
            }

            return false;
        }
        public      override    bool                Equals(object obj)
        {
            if (obj is EntityName)
                return this == (EntityName)obj;

            return false;
        }
        public      override    string              ToString()
        {
            return Fullname;
        }

        public      static      int                 _stringCompare(string s1, string s2)
        {
            if (s1 != null) {
                if (s2 != null)
                    return string.Compare(s1, s2, StringComparison.InvariantCultureIgnoreCase);

                return 1;
            }
            else {
                if (s2 != null)
                    return -1;

                return 0;
            }
        }
    }
}
