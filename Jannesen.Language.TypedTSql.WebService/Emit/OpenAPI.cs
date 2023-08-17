using System;
using System.Collections.Generic;
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
        public OpenApiSchemas                       schemas                 { get; set;}
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
        [YamlMember(Alias="x-handler")]
        public string                               x_handler           { get; set; }
        [YamlMember(Alias="x-timeout")]
        public int                                  x_timeout           { get; set; }
        public List<OpenApiParameter>               parameters          { get; set; }
        public OpenApiBody                          requestBody         { get; set; }
        public OpenApiResponses                     responses           { get; set; }
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
        public string                               @ref                { get; set; }
    }

    internal class OpenApiSchemaType: OpenApiSchema
    {
        public string                               type                { get; set; }
        public string                               format              { get; set; }
        public OpenApiSchema                        items               { get; set; }
        public OpenApiSchemaProperties              properties          { get; set; }
        public List<string>                         required            { get; set; }
        public int?                                 maxLength           { get; set; }
        public Int64?                               minimum             { get; set; }
        public Int64?                               maximum             { get; set; }
        [YamlMember(Alias="x-sqltype")]
        public string                               x_sqltype           { get; set; }
        [YamlMember(Alias="x-values")]
        public List<OpenApiX_Value>                 x_values            { get; set; }
        [YamlMember(Alias="x-post-schema")]
        public string                               x_post_schema       { get; set; }
    }

    internal class OpenApiSchemaProperties: Dictionary<string, OpenApiSchema>
    {
    }

    internal class OpenApiX_Value
    {
        public string                               name                { get; set; }
        public object                               value               { get; set; }
        public Dictionary<string, object>           fields              { get; set; }
    }

    internal class OpenApiSchemaOneOf: OpenApiSchema
    {
        public List<OpenApiSchema>                  oneOf               { get; set; }

        public static       bool    operator == (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            if (object.ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null || obj2 is null) return false;

            return obj1.oneOf == obj2.oneOf;
        }
        public static       bool    operator != (OpenApiSchemaOneOf obj1, OpenApiSchemaOneOf obj2)
        {
            return !(obj1 == obj2);
        }
        public override     int     GetHashCode()
        {
            int rtn = 0;
            if (oneOf != null) {
                foreach(var o in oneOf) rtn ^= o.GetHashCode();
            }
            return rtn;
        }
        public override     bool    Equals(object obj)
        {
            return (obj is OpenApiSchemaOneOf o && this == o);
        }
    }
}
