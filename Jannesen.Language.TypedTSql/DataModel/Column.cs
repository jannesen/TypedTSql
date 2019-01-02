using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public abstract class Column: IExprResult, ISymbol
    {
        public                  SymbolType              Type                    { get { return SymbolType.Column;    } }
        public      abstract    string                  Name                    { get; }
        public      abstract    object                  Declaration             { get; }
        public      virtual     ISymbol                 Parent                  { get { return null;                 } }
        public      virtual     ISymbol                 SymbolNameReference     { get { return null;                 } }
        public      abstract    ValueFlags              ValueFlags              { get; }
        public      abstract    ISqlType                SqlType                 { get; }
        public      abstract    string                  CollationName           { get; }
        public                  bool                    isNullable              { get { return (ValueFlags & ValueFlags.Nullable     ) != 0;    } }
        public                  bool                    isUnnammed              { get { return (ValueFlags & ValueFlags.Unnammed     ) != 0;    } }
        public                  bool                    isAnsiPadded            { get { return (ValueFlags & ValueFlags.AnsiPadded   ) != 0;    } }
        public                  bool                    isRowguidcol            { get { return (ValueFlags & ValueFlags.Rowguidcol   ) != 0;    } }
        public                  bool                    isIdentity              { get { return (ValueFlags & ValueFlags.Identity     ) != 0;    } }
        public                  bool                    isComputed              { get { return (ValueFlags & ValueFlags.Computed     ) != 0;    } }
        public                  bool                    isFilestream            { get { return (ValueFlags & ValueFlags.Filestream   ) != 0;    } }
        public                  bool                    isXmlDocument           { get { return (ValueFlags & ValueFlags.XmlDocument  ) != 0;    } }
        public                  bool                    isSparse                { get { return (ValueFlags & ValueFlags.Sparse       ) != 0;    } }
        public                  bool                    isColumnSet             { get { return (ValueFlags & ValueFlags.ColumnSet    ) != 0;    } }
        public                  bool                    hasDefault              { get { return (ValueFlags & ValueFlags.HasDefault   ) != 0;    } }

        public      virtual     bool                    ValidateConst(ISqlType sqlType)
        {
            return false;
        }

        public      virtual     void                    SetUsed()
        {
        }

        public      override    int                     GetHashCode()
        {
            return Name.GetHashCode();
        }
        public      override    bool                    Equals(Object obj)
        {
            if (obj is Column col2) {
                if (this.Name          == col2.Name        &&
                    this.Declaration   == col2.Declaration &&
                    this.ValueFlags    == col2.ValueFlags  &&
                    this.SqlType       == col2.SqlType     &&
                    this.CollationName == col2.CollationName)
                return true;
            }

            return false;
        }
    }
}
