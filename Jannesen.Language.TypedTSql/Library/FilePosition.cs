using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Library
{
    public struct FilePosition: IEquatable<FilePosition>
    {
        public              int             Filepos;
        public              int             Lineno;
        public              int             Linepos;

        public              bool            hasValue
        {
            get {
                return Filepos > 0 || Lineno > 0 || Linepos > 0;
            }
        }

        public                              FilePosition(int filepos, int lineno, int linepos)
        {
            Filepos = filepos;
            Lineno  = lineno;
            Linepos = linepos;
        }
        public                              FilePosition(int lineno)
        {
            Filepos = -1;
            Lineno  = lineno;
            Linepos = -1;
        }

        public  static      bool            operator == (FilePosition p1, FilePosition p2)
        {
            if ((object)p1 == (object)p2) return true;
            if ((object)p1 == null || (object)p2 == null) return false;

            return p1.Filepos == p2.Filepos && p1.Lineno == p2.Lineno && p1.Linepos == p2.Linepos;
        }
        public  static      bool            operator != (FilePosition p1, FilePosition p2)
        {
            return !(p1 == p2);
        }
        public  override    bool            Equals(object obj)
        {
            if (obj is FilePosition)
                return this == (FilePosition)obj;

            return false;
        }
        public              bool            Equals(FilePosition o)
        {
            return this == o;
        }
        public  override    int             GetHashCode()
        {
            return Filepos ^ Lineno ^ Linepos;
        }
        public  override    string          ToString()
        {
            return Lineno.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + Linepos.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public  static      FilePosition    Null
        {
            get {
                return new FilePosition() { Lineno = 0, Linepos = 0 };
            }
        }
    }
}
