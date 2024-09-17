using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.WebService.Library;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Jannesen.Language.TypedTSql.WebService.Emit
{
    internal class OpenApiDocument
    {
        public              string                                  openapi                 { get; set; }
        public              OpenApiInfo                             info                    { get; set; }
        public              OpenApiPaths                            paths                   { get; set; }
        public              OpenApiComponents                       components              { get; set; }
    }

    internal class OpenApiInfo
    {
        public              string                                  title                   { get; set; }
        public              string                                  version                 { get; set; }
    }

    internal class OpenApiPaths: ComparableSortedDictionary<string, OpenApiPathItem>
    {
    }

    internal class OpenApiComponents
    {
        public              OpenApiSecuritySchemes                  securitySchemes         { get; set; }
        public              OpenApiSchemas                          schemas                 { get; set; }
    }

    internal class OpenApiPathItem
    {
        public              OpenApiOperation                        get                     { get; set; }
        public              OpenApiOperation                        put                     { get; set; }
        public              OpenApiOperation                        post                    { get; set; }
        public              OpenApiOperation                        delete                  { get; set; }
    }

    internal class OpenApiOperation: IYamlConvertible
    {
        public              OpenApiSecurityList                     security                { get; set; }
        public              OpenApiParameters                       parameters              { get; set; }
        public              OpenApiBody                             requestBody             { get; set; }
        public              OpenApiResponses                        responses               { get; set; }
        public              int?                                    x_timeout               { get; set; }
        public              string                                  x_kind                  { get; set; }

        private             ComparableDictionary<string, object>    _attributes             { get; set; }

        public              void                                    SetAttribute(string name, object value)
        {
            if (_attributes == null) _attributes = new ComparableDictionary<string, object>();
            _attributes.Add(name, value);
        }

                            void                                    IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            throw new InvalidOperationException("OpenApiOperation Deserializer not supported."); 
        }
                            void                                    IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            var x = new Dictionary<string, object>();
            if (security    != null) x.Add("security",    security     );
            if (parameters  != null) x.Add("parameters",  parameters   );
            if (requestBody != null) x.Add("requestBody", requestBody  );
            if (responses   != null) x.Add("responses",   responses    );
            if (x_timeout.HasValue ) x.Add("x-timeout",   x_timeout    );
            if (x_kind      != null) x.Add("x-kind",      x_kind);

            if (_attributes  != null) {
                foreach (var o in _attributes) {
                    x.Add(o.Key, o.Value);
                }
            }

            nestedObjectSerializer(x);
        }
    }

    internal class OpenApiSecurityList: ComparableList<OpenApiSecurity>
    {
    }

    internal class OpenApiSecurity: IYamlConvertible
    {
        public          string                                      name                    { get; set; }
        public          string[]                                    options                 { get; set; }

                        void                                        IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            throw new InvalidOperationException("OpenApiSecurity Deserializer not supported."); 
        }
                        void                                        IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(TagName.Empty, name));
            nestedObjectSerializer(options, typeof(string[]));
            emitter.Emit(new MappingEnd());

        }
    }

    internal class OpenApiParameters: ComparableList<OpenApiParameter>
    {
        public              bool                                    TryGet(string @in, string name, out OpenApiParameter found) {
            for (int i = 0 ; i < Count ; ++i) {
                if (this[i].@in == @in && this[i].name == name) {
                    found = this[i];
                    return true;
                }
            }

            found = null;
            return false;
        }
    }

    internal class OpenApiParameter
    {
        public              string                                  @in                     { get; set; }
        public              string                                  name                    { get; set; }
        public              OpenApiSchema                           schema                  { get; set; }
        public              bool                                    required                { get; set; }
    }

    internal class OpenApiResponses: ComparableDictionary<string, OpenApiBody>
    {
    }

    internal class OpenApiBody
    {
        public              string                                  description             { get; set; }
        public              bool?                                   required                { get; set; }
        public              OpenApiContentTypes                     content                 { get; set; }
    }

    internal class OpenApiContentTypes: ComparableDictionary<string, OpenApiContent>
    {
    }

    internal class OpenApiContent
    {
        public              OpenApiSchema                           schema                  { get; set; }
    }

    internal class OpenApiSecuritySchemes: Dictionary<string, OpenApiSecurityScheme>
    {
    }

    internal enum SecuritySchemeType
    {
        http    = 1,
        apiKey  = 2
    }

    internal class OpenApiSecurityScheme
    {
        public              SecuritySchemeType                      type;
        public              string                                  scheme;
        public              string                                  @in;
        public              string                                  name;
    }

    internal class OpenApiSchemas: SortedDictionary<string, OpenApiSchema>
    {
    }

    internal class OpenApiSchema
    {
    }

    internal class OpenApiSchemaRef: OpenApiSchema
    {
        [YamlMember(Alias="$ref")]
        public              string                                  @ref                    { get; set; }

        [YamlIgnore]
        public              OpenApiSchema                           schema                  { get; set; }
            
        public static       bool                                    operator == (OpenApiSchemaRef left, OpenApiSchemaRef right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.@ref == right.@ref;
        }
        public static       bool                                    operator != (OpenApiSchemaRef left, OpenApiSchemaRef right)
        {
            return !(left == right);
        }
        public override     bool                                    Equals(object obj)
        {
            return obj is OpenApiSchemaRef o && this == o;
        }
        public override     int                                     GetHashCode()
        {
            return @ref.GetHashCode();
        }
    }

    internal class OpenApiSchemaType: OpenApiSchema, IYamlConvertible
    {
        public              string                                  type                    { get; set; }
        public              string                                  format                  { get; set; }
        public              string                                  description             { get; set; }
        public              OpenApiSchema                           items                   { get; set; }
        public              OpenApiSchemaProperties                 properties              { get; set; }
        public              ComparableHashSet<string>               required                { get; set; }
        public              int?                                    minLength               { get; set; }
        public              int?                                    maxLength               { get; set; }
        public              object                                  minValue                { get; set; }
        public              object                                  maxValue                { get; set; }
        public              object                                  @default                { get; set; }
        public              object                                  multipleOf              { get; set; }
        public              string                                  pattern                 { get; set; }

        private             ComparableDictionary<string, object>    _attributes             { get; set; }

        public              void                                    AddRequired(string name)
        {
            if (required == null) {
                required = new ComparableHashSet<string>();
            }
            required.Add(name);
        }

        public              object                                  GetAttribute(string name)
        {
            return  (_attributes != null && _attributes.TryGetValue(name, out var found)) ? found : null;
        }
        public              void                                    SetAttribute(string name, object value)
        {
            if (!name.StartsWith("x-")) {
                switch(name) {
                case "description":
                    description = (string)value;
                    return;

                case "min-length":      minLength   = (int)(Int64)value;    return;
                case "max-length":      maxLength   = (int)(Int64)value;    return;
                case "min-value":       minValue    = value;                return;
                case "max-value":       maxValue    = value;                return;
                case "multiple-of":     multipleOf  = value;                return;
                case "pattern":         pattern    = (string)value;         return;
                case "precision": {
                        var m = new Decimal(1);
                        var i = (int)(Int64)value;
                        while (i-- > 0) {
                            m /= 10;
                        }
                        multipleOf = m;
                    }
                    return;
                default:
                    name = "x-" + name;
                    break;
                }
            }

            if (_attributes == null) _attributes = new ComparableDictionary<string, object>();
            _attributes.Add(name, value);
        }

        public  static      bool                                    operator == (OpenApiSchemaType left, OpenApiSchemaType right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.type          == right.type         &&
                   left.format        == right.format       &&
                   left.description   == right.description  &&
                   left.items         == right.items        &&
                   left.properties    == right.properties   &&
                   left.required      == right.required     &&
                   left.minLength     == right.minLength    &&
                   left.maxLength     == right.maxLength    &&
                   left.minValue      == right.minValue     &&
                   left.maxValue      == right.maxValue     &&
                   left.multipleOf    == right.multipleOf   &&
                   left.pattern       == right.pattern      &&
                   left.@default      == right.@default     &&
                   left._attributes   == right._attributes;
        }
        public  static      bool                                    operator != (OpenApiSchemaType left, OpenApiSchemaType right)
        {
            return !(left == right);
        }
        public  override    bool                                    Equals(object obj)
        {
            return obj is OpenApiSchemaType o && this == o;
        }
        public  override    int                                     GetHashCode()
        {
            unchecked {
                var hashCode = 17;
                hashCode = (hashCode * 397) ^ (type          != null ? type.GetHashCode()          : 0);
                hashCode = (hashCode * 397) ^ (format        != null ? format.GetHashCode()        : 0);
                hashCode = (hashCode * 397) ^ (description   != null ? description.GetHashCode()   : 0);
                hashCode = (hashCode * 397) ^ (items         != null ? items.GetHashCode()         : 0);
                hashCode = (hashCode * 397) ^ (properties    != null ? properties.GetHashCode()    : 0);
                hashCode = (hashCode * 397) ^ (required      != null ? required.GetHashCode()      : 0);
                hashCode = (hashCode * 397) ^ minLength.GetHashCode();
                hashCode = (hashCode * 397) ^ maxLength.GetHashCode();
                hashCode = (hashCode * 397) ^ (minValue      != null ? minValue.GetHashCode()      : 0);
                hashCode = (hashCode * 397) ^ (maxValue      != null ? maxValue.GetHashCode()      : 0);
                hashCode = (hashCode * 397) ^ (multipleOf    != null ? multipleOf.GetHashCode()    : 0);
                hashCode = (hashCode * 397) ^ (pattern       != null ? pattern.GetHashCode()       : 0);
                hashCode = (hashCode * 397) ^ (@default      != null ? @default.GetHashCode()       : 0);
                hashCode = (hashCode * 397) ^ (_attributes   != null ? _attributes.GetHashCode()   : 0);
                return hashCode;
            }
        }

                            void                                    IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            throw new InvalidOperationException("OpenApiSchemaType Deserializer not supported."); 
        }
                            void                                    IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            var x = new Dictionary<string, object>();
            if (type        != null) x.Add("type",          type            );
            if (format      != null) x.Add("format",        format          );
            if (description != null) x.Add("description",   format          );
            if (items       != null) x.Add("items",         items           );
            if (properties  != null) x.Add("properties",    properties      );
            if (required    != null) x.Add("required",      required        );
            if (minLength.HasValue)  x.Add("minLength",     minLength.Value );
            if (maxLength.HasValue)  x.Add("maxLength",     maxLength.Value );
            if (minValue    != null) x.Add("minValue",      minValue        );
            if (maxValue    != null) x.Add("maxValue",      maxValue        );
            if (multipleOf  != null) x.Add("multipleOf",    multipleOf      );
            if (pattern     != null) x.Add("pattern",       pattern         );
            if (@default    != null) x.Add("default",       @default        );

            if (_attributes  != null) {
                foreach (var o in _attributes) {
                    x.Add(o.Key, o.Value);
                }
            }

            nestedObjectSerializer(x);
        }
    }

    internal class OpenApiSchemaProperties: ComparableDictionary<string, OpenApiSchema>
    {
    }

    internal class OpenApiX_Values: ComparableList<OpenApiX_Value>
    {
    }

    internal class OpenApiX_Value
    {
        public              string                                  name                    { get; set; }
        public              object                                  value                   { get; set; }
        public              ComparableDictionary<string, object>    fields                  { get; set; }

        public  static      bool                                    operator == (OpenApiX_Value left, OpenApiX_Value right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.name  == right.name &&
                   left.value == right.value &&
                   CompareExtensions.EqualItems(left.fields, right.fields);
        }
        public  static      bool                                    operator != (OpenApiX_Value left, OpenApiX_Value right)
        {
            return !(left == right);
        }
        public  override    bool                                    Equals(object obj)
        {
            return obj is OpenApiX_Value o && this == o;
        }
        public  override    int                                     GetHashCode()
        {
            unchecked {
                var hashCode = name != null ? name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (value  != null ? value.GetHashCode()  : 0);
                hashCode = (hashCode * 397) ^ (fields != null ? fields.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    internal class OpenApiSchemaOneOf: OpenApiSchema
    {
        public ComparableHashSet<OpenApiSchema>                     oneOf                   { get; set; }

        public  static      bool                                    operator == (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            if (ReferenceEquals(obj1, obj2))  return true;
            if (obj1 is null || obj2 is null) return false;

            return obj1.oneOf == obj2.oneOf;
        }
        public  static      bool                                    operator != (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            return !(obj1 == obj2);
        }
        public  override    bool                                    Equals(object obj)
        {
            return obj is OpenApiSchemaOneOf o && this == o;
        }
        public  override    int                                     GetHashCode()
        {
            return oneOf.GetHashCode();
        }
    }
}
