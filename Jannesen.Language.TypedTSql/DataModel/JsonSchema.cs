using System;
using System.Collections;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum JsonFlags
    {
        None            = 0,
        Required        = 0x0001
    }

    public abstract class JsonSchema
    {
    }
    public class JsonSchemaObject: JsonSchema
    {
        public class Property: ISymbol
        {
            public                      SymbolType              Type                    { get { return SymbolType.JsonSchemaObjectProperty;     } }
            public                      string                  Name                    { get { return _name;                                   } }
            public                      string                  FullName                { get { return _name;                                   } }
            public                      object                  Declaration             { get { return _declaration;                            } }
            public                      ISymbol                 ParentSymbol            { get { return null;                                    } }
            public                      ISymbol                 SymbolNameReference     { get { return null;                                    } }
            public                      JsonSchema              JsonSchema              { get { return _jsonSchema;                             } }

            private                     string                  _name;
            private                     object                  _declaration;
            private                     JsonSchema              _jsonSchema;

            public                                  Property(string name, object declaration, JsonSchema jsonSchema)
            {
                _name        = name;
                _declaration = declaration;
                _jsonSchema  = jsonSchema;
            }
        }
        public class PropertyList: Library.ListHashName<Property>
        {
            public                                  PropertyList(int capacity): base(capacity)
            {
            }

            protected   override    string          ItemKey(Property item)
            {
                return item.Name;
            }
        }

        public                  PropertyList        Properties          { get; private set; }

        public                                      JsonSchemaObject(PropertyList properties)
        {
            Properties = properties;
        }
    }
    public class JsonSchemaArray: JsonSchema
    {
        public  readonly        JsonSchema          JsonSchema;

        public                                      JsonSchemaArray(JsonSchema jsonType)
        {
            JsonSchema = jsonType;
        }
    }
    public class JsonSchemaValue: JsonSchema
    {
        public  readonly        ISqlType            SqlType;
        public  readonly        JsonFlags           Flags;

        public                                      JsonSchemaValue(ISqlType sqlType, JsonFlags flags)
        {
            SqlType = sqlType;
            Flags   = flags;
        }
    }
}
