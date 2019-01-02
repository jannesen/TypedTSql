using System;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public sealed class SqlTypeNative: SqlType, ISymbol
    {
        public  static readonly SqlTypeNative       Bit                 = new SqlTypeNative(SystemType.Bit);
        public  static readonly SqlTypeNative       TinyInt             = new SqlTypeNative(SystemType.TinyInt);
        public  static readonly SqlTypeNative       SmallInt            = new SqlTypeNative(SystemType.SmallInt);
        public  static readonly SqlTypeNative       Int                 = new SqlTypeNative(SystemType.Int);
        public  static readonly SqlTypeNative       BigInt              = new SqlTypeNative(SystemType.BigInt);
        public  static readonly SqlTypeNative       SmallMoney          = new SqlTypeNative(SystemType.SmallMoney);
        public  static readonly SqlTypeNative       Money               = new SqlTypeNative(SystemType.Money);
        public  static readonly SqlTypeNative       Date                = new SqlTypeNative(SystemType.Date);
        public  static readonly SqlTypeNative       Time                = new SqlTypeNative(SystemType.Time);
        public  static readonly SqlTypeNative       SmallDateTime       = new SqlTypeNative(SystemType.SmallDateTime);
        public  static readonly SqlTypeNative       DateTime            = new SqlTypeNative(SystemType.DateTime);
        public  static readonly SqlTypeNative       DateTime2           = new SqlTypeNative(SystemType.DateTime2, scale:7);
        public  static readonly SqlTypeNative       DateTimeOffset      = new SqlTypeNative(SystemType.DateTimeOffset, scale:7);
        public  static readonly SqlTypeNative       Real                = new SqlTypeNative(SystemType.Real);
        public  static readonly SqlTypeNative       Float               = new SqlTypeNative(SystemType.Float, precision:53);
        public  static readonly SqlTypeNative       UniqueIdentifier    = new SqlTypeNative(SystemType.UniqueIdentifier);
        public  static readonly SqlTypeNative       Xml                 = new SqlTypeNative(SystemType.Xml);
        public  static readonly SqlTypeNative       Clr                 = new SqlTypeNative(SystemType.Clr);
        public  static readonly SqlTypeNative       VarBinary_85        = new SqlTypeNative(SystemType.VarBinary, maxLength:85);
        public  static readonly SqlTypeNative       VarBinary_128       = new SqlTypeNative(SystemType.VarBinary, maxLength:128);
        public  static readonly SqlTypeNative       VarBinary_MAX       = new SqlTypeNative(SystemType.VarBinary, maxLength:-1);
        public  static readonly SqlTypeNative       Char10              = new SqlTypeNative(SystemType.Char, maxLength:10);
        public  static readonly SqlTypeNative       VarChar_4           = new SqlTypeNative(SystemType.VarChar, maxLength:4);
        public  static readonly SqlTypeNative       VarChar_16          = new SqlTypeNative(SystemType.VarChar, maxLength:16);
        public  static readonly SqlTypeNative       VarChar_48          = new SqlTypeNative(SystemType.VarChar, maxLength:48);
        public  static readonly SqlTypeNative       VarChar_MAX         = new SqlTypeNative(SystemType.VarChar, maxLength:-1);
        public  static readonly SqlTypeNative       NVarChar_32         = new SqlTypeNative(SystemType.NVarChar, maxLength:32);
        public  static readonly SqlTypeNative       NVarChar_40         = new SqlTypeNative(SystemType.NVarChar, maxLength:40);
        public  static readonly SqlTypeNative       NVarChar_128        = new SqlTypeNative(SystemType.NVarChar, maxLength:128);
        public  static readonly SqlTypeNative       NVarChar_258        = new SqlTypeNative(SystemType.NVarChar, maxLength:258);
        public  static readonly SqlTypeNative       NVarChar_4000       = new SqlTypeNative(SystemType.NVarChar, maxLength:4000);
        public  static readonly SqlTypeNative       NVarChar_MAX        = new SqlTypeNative(SystemType.NVarChar, maxLength:-1);
        public  static readonly SqlTypeNative       SysName             = new SqlTypeNative(SystemType.NVarChar, maxLength:256);
        public  static readonly SqlTypeNative       SqlVariant          = new SqlTypeNative(SystemType.SqlVariant);

        public      readonly    SystemType          SystemType;
        public      readonly    Int16               MaxLength;
        public      readonly    byte                Precision;
        public      readonly    byte                Scale;
        public                  bool                isInteger
        {
            get {
                return SystemType == SystemType.TinyInt  ||
                       SystemType == SystemType.SmallInt ||
                       SystemType == SystemType.Int      ||
                       SystemType == SystemType.BigInt;
            }
        }
        public                  bool                isVarLength
        {
            get {
                switch(SystemType) {
                case SystemType.VarBinary:
                case SystemType.VarChar:
                case SystemType.NVarChar:
                    return true;

                default:
                    return false;
                }
            }
        }
        public                  bool                isUnicode
        {
            get {
                switch(SystemType) {
                case SystemType.NChar:
                case SystemType.NVarChar:
                    return true;

                default:
                    return false;
                }
            }
        }
        public                  bool                hasCollate
        {
            get {
                return SystemType == SystemType.Char || SystemType == SystemType.VarChar;
            }
        }

        public      override    SqlTypeFlags        TypeFlags           { get { return SqlTypeFlags.SimpleType | SqlTypeFlags.CheckTSql;   } }
        public      override    SqlTypeNative       NativeType
        {
            get {
                return this;
            }
        }

        public                  byte                SystemTypeId
        {
            get {
                switch(SystemType) {
                case SystemType.Image:              return   34;
                case SystemType.Text:               return   35;
                case SystemType.UniqueIdentifier:   return   36;
                case SystemType.Date:               return   40;
                case SystemType.Time:               return   41;
                case SystemType.DateTime2:          return   42;
                case SystemType.DateTimeOffset:     return   43;
                case SystemType.TinyInt:            return   48;
                case SystemType.SmallInt:           return   52;
                case SystemType.Int:                return   56;
                case SystemType.SmallDateTime:      return   58;
                case SystemType.Real:               return   59;
                case SystemType.Money:              return   60;
                case SystemType.DateTime:           return   61;
                case SystemType.Float:              return   62;
                case SystemType.SqlVariant:         return   98;
                case SystemType.NText:              return   99;
                case SystemType.Bit:                return  104;
                case SystemType.Decimal:            return  106;
                case SystemType.Numeric:            return  108;
                case SystemType.SmallMoney:         return  122;
                case SystemType.BigInt:             return  127;
                case SystemType.VarBinary:          return  165;
                case SystemType.VarChar:            return  167;
                case SystemType.Binary:             return  173;
                case SystemType.Char:               return  175;
                case SystemType.Timestamp:          return  189;
                case SystemType.NVarChar:           return  231;
                case SystemType.NChar:              return  239;
                case SystemType.Clr:                return  240;
                case SystemType.Xml:                return  241;
                default:                            throw new InvalidOperationException("Can't convert " + SystemType + " to sysmtem_type_id.");
                }
            }
        }
        public                  string              NativeTypeString
        {
            get {
                switch(SystemType) {
                case SystemType.BigInt:             return "bigint";
                case SystemType.Binary:             return "binary("            + _maxLengthToString() + ")";
                case SystemType.Bit:                return "bit";
                case SystemType.Char:               return "char("              + _maxLengthToString() + ")";
                case SystemType.Date:               return "date";
                case SystemType.DateTime:           return "datetime";
                case SystemType.DateTime2:          return "datetime2("         + Scale.ToString() + ")";
                case SystemType.DateTimeOffset:     return "datetimeoffset("    + Scale.ToString() + ")";
                case SystemType.Decimal:            return "decimal("           + Precision.ToString() + "," + Scale.ToString() + ")";
                case SystemType.Float:              return "float("             + Precision.ToString() + ")";
                case SystemType.Image:              return "image";
                case SystemType.Int:                return "int";
                case SystemType.Money:              return "money";
                case SystemType.NChar:              return "nchar("             + _maxLengthToString() + ")";
                case SystemType.NText:              return "ntext";
                case SystemType.Numeric:            return "numeric("           + Precision.ToString() + "," + Scale.ToString() + ")";
                case SystemType.NVarChar:           return "nvarchar("          + _maxLengthToString() + ")";
                case SystemType.Real:               return "real";
                case SystemType.SmallDateTime:      return "smalldatetime";
                case SystemType.SmallInt:           return "smallint";
                case SystemType.SmallMoney:         return "smallmoney";
                case SystemType.SqlVariant:         return "sql_variant";
                case SystemType.Text:               return "text";
                case SystemType.Time:               return "time";
                case SystemType.Timestamp:          return "timestamp";
                case SystemType.TinyInt:            return "tinyint";
                case SystemType.UniqueIdentifier:   return "uniqueidentifier";
                case SystemType.VarBinary:          return "varbinary("         + _maxLengthToString() + ")";
                case SystemType.VarChar:            return "varchar("           + _maxLengthToString() + ")";
                case SystemType.Xml:                return "xml";
                default:                            return SystemType.ToString();
                }
            }
        }

        public                                      SqlTypeNative(SystemType systemType, int maxLength=0, byte precision=0, byte scale=0)
        {
            switch(systemType) {
            case SystemType.Binary:
            case SystemType.VarBinary:
            case SystemType.Char:
            case SystemType.VarChar:
                if (maxLength < -1)
                    throw new ArgumentException("Invalid value for maxLength");

                if (maxLength > 8000)
                    maxLength = 8000;
                break;

            case SystemType.NChar:
            case SystemType.NVarChar:
                if (maxLength < -1)
                    throw new ArgumentException("Invalid value for maxLength");

                if (maxLength > 4000)
                    maxLength = 4000;
                break;

            default:
                if (maxLength != 0)
                    throw new ArgumentException("Invalid value for maxLength");
                break;
            }

            switch(systemType) {
            case SystemType.Numeric:
            case SystemType.Decimal:
                if (precision < 1 || precision > 38)
                    throw new ArgumentException("Invalid value for precision");
                if (scale < 0 || scale > precision)
                    throw new ArgumentException("Invalid value for scale");
                break;

            case SystemType.Float:
                if (precision < 1 || precision > 53)
                    throw new ArgumentException("Invalid value for precision");
                if (scale != 0)
                    throw new ArgumentException("Invalid value for scale");
                break;

            case SystemType.DateTime2:
            case SystemType.DateTimeOffset:
                if (precision != 0)
                    throw new ArgumentException("Invalid value for precision");
                if (scale < 0 || scale > 7)
                    throw new ArgumentException("Invalid value for scale");
                break;

            default:
                if (precision != 0)
                    throw new ArgumentException("Invalid value for precision");
                if (scale != 0)
                    throw new ArgumentException("Invalid value for scale");
                break;
            }

            this.SystemType = systemType;
            this.MaxLength  = (Int16)maxLength;
            this.Precision  = precision;
            this.Scale      = scale;
        }

        internal    static      SqlTypeNative       ReadFromDatabase(SqlDataReader dataReader, int colOffset)
        {
            var         systemType = _mapSystemType(dataReader.GetByte(colOffset));

            switch(systemType) {
            case SystemType.Binary:
            case SystemType.VarBinary:
            case SystemType.Char:
            case SystemType.NChar:
            case SystemType.VarChar:
            case SystemType.NVarChar:
                return new SqlTypeNative(systemType,
                                            maxLength: dataReader.GetInt16(colOffset + 1));

            case SystemType.Float:
                {
                    var precision = dataReader.GetByte (colOffset + 2);
                    return (precision == 53) ? Float : new SqlTypeNative(systemType,
                                                                            precision: precision);
                }

            case SystemType.Decimal:
            case SystemType.Numeric:
                return new SqlTypeNative(systemType,
                                            precision: dataReader.GetByte (colOffset + 2),
                                            scale:     dataReader.GetByte (colOffset + 3));

            case SystemType.DateTime2:
            case SystemType.DateTimeOffset:
                return new SqlTypeNative(systemType, scale:dataReader.GetByte (colOffset + 3));

            default:
                return _constructSimpleType(systemType);
            }
        }

        public      static      SqlTypeNative       ParseNativeType(string s)
        {
            string      typeName;
            string[]    typeArgs;

            int     b = s.IndexOf('(');

            if (b > 0) {
                if (!s.EndsWith(")"))
                    throw new ArgumentException("Invalid syntax native sql-type.");

                typeName = s.Substring(0, b).Trim();
                typeArgs = s.Substring(b + 1, s.Length - 1 - (b + 1)).Split(',');

                if (typeArgs.Length > 2)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                for (int i = 0 ; i < typeArgs.Length ; ++i)
                    typeArgs[i] = typeArgs[i].Trim();
            }
            else {
                typeName = s;
                typeArgs = null;
            }

            SystemType systemTypeId = ParseSystemType(typeName.ToLower());
            if (systemTypeId == SystemType.Unknown)
                throw new ArgumentException("Unknown sqltype '" + typeName + "'");

            return ParseNativeType(systemTypeId,
                                   (typeArgs != null && typeArgs.Length >= 1 ? typeArgs[0] : null),
                                   (typeArgs != null && typeArgs.Length >= 2 ? typeArgs[1] : null));
        }
        public      static      SqlTypeNative       ParseNativeType(string name, string parm1, string parm2)
        {
            return ParseNativeType(ParseSystemType(name.ToLower()), parm1, parm2);
        }
        public      static      SqlTypeNative       ParseNativeType(SystemType systemType, string parm1, string parm2)
        {
            Int16           maxLength;
            byte            precision;
            byte            scale;

            switch(systemType) {
            case SystemType.Binary:
            case SystemType.VarBinary:
            case SystemType.Char:
            case SystemType.NChar:
            case SystemType.VarChar:
            case SystemType.NVarChar:
                if (parm1 == null || parm2 != null)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                if (string.Compare(parm1, "MAX", true) == 0)
                    maxLength = -1;
                else {
                    try {
                        maxLength =  Int16.Parse(parm1);
                    }
                    catch(Exception) {
                        throw new ArgumentException("Invalid syntax native sql-type.");
                    }

                    if (maxLength < 1)
                        throw new ArgumentException("Invalid syntax native sql-type, maxlength is minimal 1.");

                    if (maxLength > SystemTypeMaxLength(systemType))
                        throw new ArgumentException("Invalid syntax native sql-type, maxlength is maximal " + SystemTypeMaxLength(systemType) + ".");
                }

                return new SqlTypeNative(systemType, maxLength:maxLength);

            case SystemType.Float:
                if (parm2 != null)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                if (parm1 != null) {
                    try {
                        precision =  byte.Parse(parm1);
                    }
                    catch(Exception) {
                        throw new ArgumentException("Invalid syntax native sql-type.");
                    }
                }
                else
                    precision = 53;

                if (precision < 1 || precision > 53)
                    throw new ArgumentException("Invalid syntax native sql-type, precision is between 1 and 53.");

                return new SqlTypeNative(systemType, precision:precision);

            case SystemType.Decimal:
            case SystemType.Numeric:
                if (parm1 == null)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                try {
                    precision =  byte.Parse(parm1);
                }
                catch(Exception) {
                    throw new ArgumentException("Invalid syntax native sql-type.");
                }

                if (parm2 != null) {
                    try {
                        scale =  byte.Parse(parm2);
                    }
                    catch(Exception) {
                        throw new ArgumentException("Invalid syntax native sql-type.");
                    }
                }
                else
                    scale = 0;

                if (precision < 1 || precision > 38)
                    throw new ArgumentException("Invalid syntax native sql-type, precision is between 1 and 38.");

                if (scale < 0 || scale > precision)
                    throw new ArgumentException("Invalid syntax native sql-type, sacle is between 1 and precision.");

                return new SqlTypeNative(systemType, precision:precision, scale:scale);

            case SystemType.DateTime2:
            case SystemType.DateTimeOffset:
                if (parm1 == null || parm2 != null)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                try {
                    scale =  byte.Parse(parm1);
                }
                catch(Exception) {
                    throw new ArgumentException("Invalid syntax native sql-type.");
                }

                if (scale < 0 || scale > 7)
                    throw new ArgumentException("Invalid syntax native sql-type, fractional seconds precision is between 0 and 7.");

                return new SqlTypeNative(systemType, scale:scale);

            default:
                if (parm1 != null || parm2 != null)
                    throw new ArgumentException("Invalid syntax native sql-type.");

                return _constructSimpleType(systemType);
            }
        }
        public      static      SystemType          ParseSystemType(string typeName)
        {
            switch(typeName) {
            case "bigint":              return SystemType.BigInt;
            case "binary":              return SystemType.Binary;
            case "bit":                 return SystemType.Bit;
            case "char":                return SystemType.Char;
            case "date":                return SystemType.Date;
            case "datetime":            return SystemType.DateTime;
            case "datetime2":           return SystemType.DateTime2;
            case "datetimeoffset":      return SystemType.DateTimeOffset;
            case "decimal":             return SystemType.Decimal;
            case "float":               return SystemType.Float;
            case "image":               return SystemType.Image;
            case "int":                 return SystemType.Int;
            case "money":               return SystemType.Money;
            case "nchar":               return SystemType.NChar;
            case "ntext":               return SystemType.NText;
            case "numeric":             return SystemType.Numeric;
            case "nvarchar":            return SystemType.NVarChar;
            case "real":                return SystemType.Real;
            case "smalldatetime":       return SystemType.SmallDateTime;
            case "smallint":            return SystemType.SmallInt;
            case "smallmoney":          return SystemType.SmallMoney;
            case "sql_variant":         return SystemType.SqlVariant;
            case "text":                return SystemType.Text;
            case "time":                return SystemType.Time;
            case "timestamp":           return SystemType.Timestamp;
            case "tinyint":             return SystemType.TinyInt;
            case "uniqueidentifier":    return SystemType.UniqueIdentifier;
            case "varbinary":           return SystemType.VarBinary;
            case "varchar":             return SystemType.VarChar;
            case "xml":                 return SystemType.Xml;
            default:                    return SystemType.Unknown;
            }
        }

        public      static      SqlTypeNative       NewString(bool unicode, bool var, int length)
        {
            if (length > (unicode ? 4000 : 8000))
                length = (unicode ? 4000 : 8000);

            return new DataModel.SqlTypeNative(unicode ? (var ? DataModel.SystemType.NVarChar : DataModel.SystemType.NChar)
                                                       : (var ? DataModel.SystemType.VarChar  : DataModel.SystemType.Char),
                                               maxLength:(short)length);
        }

        public      static      string              SystemSchema(string name)
        {
            name = name.ToLower();

            switch(name) {
            case "hierarchyid":
            case "geometry":
            case "geography":
            case "sysname":
                return "sys";

            default:
                if (DataModel.SqlTypeNative.ParseSystemType(name) != DataModel.SystemType.Unknown)
                    return "sys";

                return null;
            }
        }
        public      static      int                 SystemTypeMaxLength(SystemType systemType)
        {
            switch(systemType) {
            case SystemType.Binary:
            case SystemType.VarBinary:
                return 8000;
            case SystemType.Char:
            case SystemType.VarChar:
                return 8000;
            case SystemType.NChar:
            case SystemType.NVarChar:
                return 4000;

            default:
                throw new InvalidOperationException("Native type " + systemType + " has no MaxLength.");
            }
        }

        public      static      bool                operator == (SqlTypeNative n1, SqlTypeNative n2)
        {
            if ((object)n1 == null)
                return ((object)n1 == null);

            if ((object)n2 == null)
                return false;

            if (object.ReferenceEquals(n1, n2))
                return true;

            return n1.SystemType == n2.SystemType &&
                   n1.MaxLength    == n2.MaxLength &&
                   n1.Precision    == n2.Precision &&
                   n1.Scale        == n2.Scale;
        }
        public      static      bool                operator != (SqlTypeNative n1, SqlTypeNative n2)
        {
            return !(n1 == n2);
        }
        public      override    int                 GetHashCode()
        {
            return ((int)SystemType)    ^
                   ((int)MaxLength <<  4) ^
                   ((int)Precision <<  8) ^
                   ((int)Scale     << 10);
        }
        public      override    bool                Equals(object obj)
        {
            if (obj is SqlTypeNative)
                return this == (SqlTypeNative)obj;

            return false;
        }
        public      override    string              ToSql()
        {
            return NativeTypeString;
        }
        public      override    string              ToString()
        {
            return NativeTypeString;
        }

                                SymbolType          ISymbol.Type                    { get { return SymbolType.NativeType;   } }
                                string              ISymbol.Name                    { get { return SystemType.ToString();   } }
                                object              ISymbol.Declaration             { get { return null;                    } }
                                ISymbol             ISymbol.Parent                  { get { return null;                    } }
                                ISymbol             ISymbol.SymbolNameReference     { get { return null;                    } }

        private                 string              _maxLengthToString()
        {
            return MaxLength >= 0 ? MaxLength.ToString() : "max";
        }

        private     static      SqlTypeNative       _constructSimpleType(SystemType systemType)
        {
            switch(systemType) {
            case SystemType.Unknown:        throw new ArgumentException("Invalid system type id.");

            case SystemType.Bit:                return Bit;
            case SystemType.TinyInt:            return TinyInt;
            case SystemType.SmallInt:           return SmallInt;
            case SystemType.Int:                return Int;
            case SystemType.BigInt:             return BigInt;
            case SystemType.SmallMoney:         return SmallMoney;
            case SystemType.Money:              return Money;
            case SystemType.Date:               return Date;
            case SystemType.Time:               return Time;
            case SystemType.SmallDateTime:      return SmallDateTime;
            case SystemType.DateTime:           return DateTime;
            case SystemType.Real:               return Real;
            case SystemType.UniqueIdentifier:   return UniqueIdentifier;
            case SystemType.Xml:                return Xml;
            case SystemType.Clr:                return Clr;

            default:                            return new SqlTypeNative(systemType);
            }
        }

        private     static      SystemType          _mapSystemType(byte system_type_id)
        {
            switch(system_type_id) {
            case  34:   return SystemType.Image;
            case  35:   return SystemType.Text;
            case  36:   return SystemType.UniqueIdentifier;
            case  40:   return SystemType.Date;
            case  41:   return SystemType.Time;
            case  42:   return SystemType.DateTime2;
            case  43:   return SystemType.DateTimeOffset;
            case  48:   return SystemType.TinyInt;
            case  52:   return SystemType.SmallInt;
            case  56:   return SystemType.Int;
            case  58:   return SystemType.SmallDateTime;
            case  59:   return SystemType.Real;
            case  60:   return SystemType.Money;
            case  61:   return SystemType.DateTime;
            case  62:   return SystemType.Float;
            case  98:   return SystemType.SqlVariant;
            case  99:   return SystemType.NText;
            case 104:   return SystemType.Bit;
            case 106:   return SystemType.Decimal;
            case 108:   return SystemType.Numeric;
            case 122:   return SystemType.SmallMoney;
            case 127:   return SystemType.BigInt;
            case 165:   return SystemType.VarBinary;
            case 167:   return SystemType.VarChar;
            case 173:   return SystemType.Binary;
            case 175:   return SystemType.Char;
            case 189:   return SystemType.Timestamp;
            case 231:   return SystemType.NVarChar;
            case 239:   return SystemType.NChar;
            case 240:   return SystemType.Clr;
            case 241:   return SystemType.Xml;

            default:    throw new InvalidOperationException("Don't system_type_id#" + system_type_id.ToString() + ".");
            }
        }
    }
}
