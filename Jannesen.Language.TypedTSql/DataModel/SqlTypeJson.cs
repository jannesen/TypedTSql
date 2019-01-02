using System;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeJson: SqlType
    {
        public      override    SqlTypeFlags            TypeFlags       { get { return SqlTypeFlags.SimpleType | SqlTypeFlags.Json;   } }
        public      override    SqlTypeNative           NativeType      { get { return _nativeType;         } }
        public      override    JsonSchema              JsonSchema      { get { return _jsonSchema;         } }

        private                 SqlTypeNative           _nativeType;
        private                 JsonSchema              _jsonSchema;

        public                                          SqlTypeJson(SqlTypeNative nativeType, JsonSchema jsonSchema)
        {
            _nativeType = nativeType;
            _jsonSchema = jsonSchema;
        }

        public      override    string                  ToSql()
        {
            return _nativeType.ToSql();
        }
    }
}
