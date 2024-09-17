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
        public string                               openapi                 { get; set;}
        public OpenApiInfo                          info                    { get; set;}
        public OpenApiPaths                         paths                   { get; set;}
        public OpenApiComponents                    components              { get; set;}
    }

    internal class OpenApiInfo
    {
        public string                               title                   { get; set;}
        public string                               version                 { get; set;}
    }

    internal class OpenApiPaths: SortedDictionary<string, OpenApiPathItem>
    {
    }

    internal class OpenApiComponents
    {
        public OpenApiSecuritySchemes   securitySchemes         { get; set;}
        public OpenApiSchemas           schemas                 { get; set;}
    }

    internal class OpenApiPathItem
    {
        public OpenApiOperation         get                 {get; set;}
        public OpenApiOperation         put                 {get; set;}
        public OpenApiOperation         post                {get; set;}
        public OpenApiOperation         delete              {get; set;}
    }

    internal class OpenApiOperation
    {
        public OpenApiSecurityList                  security            { get; set; }
        public OpenApiParameters                    parameters          { get; set; }
        public OpenApiBody                          requestBody         { get; set; }
        public OpenApiResponses                     responses           { get; set; }
        [YamlMember(Alias="x-timeout")]
        public int                                  x_timeout           { get; set; }
    }

    internal class OpenApiSecurityList: List<OpenApiSecurity>
    {
    }

    internal class OpenApiSecurity: IYamlConvertible
    {
        public string                               name                { get; set; }
        public string[]                             options             { get; set; }

               void                                 IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            throw new InvalidOperationException("OpenApiSecurity Deserializer not supported."); 
        }
               void                                 IYamlConvertible.Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar(TagName.Empty, name));
            nestedObjectSerializer(options, typeof(string[]));
            emitter.Emit(new MappingEnd());

        }
    }

    internal class OpenApiParameters: List<OpenApiParameter>
    {
        public      bool            TryGet(string @in, string name, out OpenApiParameter found) {
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
        public string                               @in                 { get; set; }
        public string                               name                { get; set; }
        public OpenApiSchema                        schema              { get; set; }
        public bool                                 required            { get; set; }
    }

    internal class OpenApiResponses: Dictionary<string, OpenApiBody>
    {
    }

    internal class OpenApiBody
    {
        public string                               description         { get; set; }
        public bool?                                required            { get; set; }
        public OpenApiContentTypes                  content             { get; set; }
    }

    internal class OpenApiContentTypes: Dictionary<string, OpenApiContent>
    {
    }

    internal class OpenApiContent
    {
        public OpenApiSchema                        schema { get; set; }
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
        public          string                      @ref                { get; set; }

        public static   bool                        operator == (OpenApiSchemaRef left, OpenApiSchemaRef right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.@ref == right.@ref;
        }
        public static   bool                        operator != (OpenApiSchemaRef left, OpenApiSchemaRef right)
        {
            return !(left == right);
        }

        public override bool                        Equals(object obj)
        {
            return obj is OpenApiSchemaRef o && this == o;
        }
        public override int                         GetHashCode()
        {
            return @ref.GetHashCode();
        }
    }

    internal class OpenApiSchemaType: OpenApiSchema
    {
        public          string                      type                { get; set; }
        public          string                      format              { get; set; }
        public          OpenApiSchema               items               { get; set; }
        public          OpenApiSchemaProperties     properties          { get; set; }
        public          HashSet<string>             required            { get; set; }
        public          int?                        maxLength           { get; set; }
        public          Int64?                      minimum             { get; set; }
        public          Int64?                      maximum             { get; set; }
        [YamlMember(Alias="x-values")]
        public          OpenApiX_Values             x_values            { get; set; }
        [YamlMember(Alias="x-post-schema")]
        public          string                      x_post_schema       { get; set; }

        public static   bool                        operator == (OpenApiSchemaType left, OpenApiSchemaType right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.type          == right.type          &&
                   left.format        == right.format        &&
                   left.items         == right.items         &&
                   left.properties    == right.properties    &&
                   left.maxLength     == right.maxLength     &&
                   left.minimum       == right.minimum       &&
                   left.maximum       == right.maximum       &&
                   left.x_post_schema == right.x_post_schema &&
                   EnumerableExtensions.EqualItems(left.required, right.required)  &&
                   EnumerableExtensions.EqualItems(left.x_values, right.x_values);
        }
        public static   bool                        operator != (OpenApiSchemaType left, OpenApiSchemaType right)
        {
            return !(left == right);
        }

        public override bool                        Equals(object obj)
        {
            return obj is OpenApiSchemaType o && this == o;
        }
        public override int                         GetHashCode()
        {
            unchecked {
                var hashCode = 17;
                hashCode = (hashCode * 397) ^ (type          != null ? type.GetHashCode()           : 0);
                hashCode = (hashCode * 397) ^ (format        != null ? format.GetHashCode()        : 0);
                hashCode = (hashCode * 397) ^ (items         != null ? items.GetHashCode()         : 0);
                hashCode = (hashCode * 397) ^ (properties    != null ? properties.GetHashCode()    : 0);
                hashCode = (hashCode * 397) ^ (required      != null ? required.GetItemsHashCode() : 0);
                hashCode = (hashCode * 397) ^ maxLength.GetHashCode();
                hashCode = (hashCode * 397) ^ minimum.GetHashCode();
                hashCode = (hashCode * 397) ^ maximum.GetHashCode();
                hashCode = (hashCode * 397) ^ (x_values      != null ? x_values.GetItemsHashCode() : 0);
                hashCode = (hashCode * 397) ^ (x_post_schema != null ? x_post_schema.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    internal class OpenApiSchemaProperties: Dictionary<string, OpenApiSchema>
    {
        public static   bool                        operator == (OpenApiSchemaProperties left, OpenApiSchemaProperties right)
        {
            return EnumerableExtensions.EqualItems(left, right);
        }
        public static   bool                        operator != (OpenApiSchemaProperties left, OpenApiSchemaProperties right)
        {
            return !(left == right);
        }

        public override bool                        Equals(object obj)
        {
            return obj is OpenApiSchemaProperties o && this == o;
        }
        public override int                         GetHashCode()
        {
            return this.GetItemsHashCode();
        }
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
        public  SecuritySchemeType      type;
        public  string                  scheme;
        public  string                  @in;
        public  string                  name;
    }

    internal class OpenApiX_Values: List<OpenApiX_Value>
    {
    }

    internal class OpenApiX_Value
    {
        public string                               name                { get; set; }
        public object                               value               { get; set; }
        public Dictionary<string, object>           fields              { get; set; }

        public static   bool                        operator == (OpenApiX_Value left, OpenApiX_Value right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left is null || right is null) return false;
            return left.name  == right.name &&
                   left.value == right.value &&
                   EnumerableExtensions.EqualItems(left.fields, right.fields);
        }
        public static   bool                        operator != (OpenApiX_Value left, OpenApiX_Value right)
        {
            return !(left == right);
        }

        public override bool                        Equals(object obj)
        {
            return obj is OpenApiX_Value o && this == o;
        }
        public override int                         GetHashCode()
        {
            unchecked {
                var hashCode = name != null ? name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (value  != null ? value.GetHashCode()       : 0);
                hashCode = (hashCode * 397) ^ (fields != null ? fields.GetItemsHashCode() : 0);
                return hashCode;
            }
        }

    }

    internal class OpenApiSchemaOneOf: OpenApiSchema
    {
        public HashSet<OpenApiSchema>               oneOf               { get; set; }

        public static   bool                        operator == (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            if (ReferenceEquals(obj1, obj2))  return true;
            if (obj1 is null || obj2 is null) return false;

            return EnumerableExtensions.EqualItems(obj1.oneOf, obj2.oneOf);
        }
        public static   bool                        operator != (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            return !(obj1 == obj2);
        }
        public override int                         GetHashCode()
        {
            return oneOf.GetItemsHashCode();
        }
        public override bool                        Equals(object obj)
        {
            return obj is OpenApiSchemaOneOf o && this == o;
        }
    }
}
