using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityTypeUser: EntityType
    {
        public  override        SqlTypeFlags            TypeFlags           { get { testTranspiled(); return _typeFlags;    } }
        public  override        SqlTypeNative           NativeType          { get { if (_nativeType == null) throw new InvalidOperationException("Native type not set."); return _nativeType;   } }
        public  override        object                  DefaultValue        { get { return _nullValue;                      } }
        public                  ValueRecordFieldList    Fields              { get { testTranspiled(); return _fields;       } } //!!TODO not used
        public  override        ValueRecordList         Values              { get { testTranspiled(); return _values;       } }
        public                  IAttributes             Attributes          => _attributes;

        private                 SqlTypeFlags            _typeFlags;
        private                 SqlTypeNative           _nativeType;
        private                 object                  _nullValue;
        private                 ValueRecordFieldList    _fields;
        private                 ValueRecordList         _values;
        private                 IAttributes             _attributes;

        internal                                        EntityTypeUser(DataModel.EntityName name, EntityFlags flags): base(SymbolType.TypeUser, name, flags)
        {
        }
        internal                                        EntityTypeUser(GlobalCatalog catalog, DataModel.EntityName entityName, SqlDataReader dataReader, int coloffset): base(SymbolType.TypeUser, entityName, EntityFlags.SourceDatabase)
        {
            _typeFlags  = SqlTypeFlags.SimpleType | SqlTypeFlags.UserType | SqlTypeFlags.CheckTSql;
            _nativeType = SqlTypeNative.ReadFromDatabase(dataReader, coloffset + 5);
            _fields     = null;
            _values     = null;
        }

        internal                void                    TranspileInit(DocumentSpan location, SqlTypeNative sqlTypeNative, object nullValue)
        {
            TranspileInit(location);

            if (_nativeType == null)
                _nativeType = sqlTypeNative;
            else {
                if (_nativeType != sqlTypeNative)
                    throw new ErrorException("Can't change native type.");
            }

            _nullValue  = nullValue;
            _fields     = null;
            _values     = null;
            _attributes = null;
        }
        internal                void                    Transpiled(SqlTypeFlags typeFlags, ValueRecordFieldList fields, ValueRecordList values, IAttributes attributes)
        {
            _typeFlags = typeFlags | SqlTypeFlags.SimpleType | SqlTypeFlags.UserType;
            _fields    = fields;
            _values    = values;

            if (values != null)
                _typeFlags |= SqlTypeFlags.Values;

            _attributes = attributes;

            Transpiled();
        }
    }
}
