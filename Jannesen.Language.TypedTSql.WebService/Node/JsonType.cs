using System;
using System.Collections.Generic;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class JsonType: LTTSQL.Core.AstParseNode, LTTSQL.Node.ISqlType
    {
        public class JsonSchema: LTTSQL.Core.AstParseNode
        {
            public      readonly    JsonSchemaElement                   n_Schema;

            public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return _sqlType;        } }

            public                  LTTSQL.DataModel.ISqlType           _sqlType;

            public abstract class JsonSchemaElement: LTTSQL.Core.AstParseNode
            {
                public                  DataModel.JsonFlags                 n_Flags     { get; private set; }

                public                  LTTSQL.DataModel.JsonSchema         JsonSchema  { get { return _jsonSchema;         } }

                protected               LTTSQL.DataModel.JsonSchema         _jsonSchema;

                public      static      JsonSchemaElement                   Parse(LTTSQL.Core.ParserReader reader)
                {
                    if (reader.CurrentToken.isToken("OBJECT"))  return new JsonSchemaObject(reader);
                    if (reader.CurrentToken.isToken("ARRAY"))   return new JsonSchemaArray(reader);
                    return new JsonSchemaValue(reader, false);
                }

                protected               void                                ReadElement(LTTSQL.Core.ParserReader reader)
                {
                    if (ParseOptionalToken(reader, LTTSQL.Core.TokenID.REQUIRED) != null)
                        n_Flags |= DataModel.JsonFlags.Required;
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                }
            }
            public class JsonSchemaObject: JsonSchemaElement
            {
                public class JsonSchemaObjectProperty: LTTSQL.Core.AstParseNode
                {
                    public      readonly    Core.TokenWithSymbol                n_Name;
                    public      readonly    JsonSchemaElement                   n_JsonSchemaElement;

                    public                  LTTSQL.DataModel.JsonSchemaObject.Property  JsonProperty        { get; private set; }

                    public                                                      JsonSchemaObjectProperty(LTTSQL.Core.ParserReader reader)
                    {
                        n_Name = ParseName(reader);
                        n_JsonSchemaElement = JsonSchemaElement.Parse(reader);
                    }
                    public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                    {
                        JsonProperty = null;
                        n_JsonSchemaElement.TranspileNode(context);
                        JsonProperty = new LTTSQL.DataModel.JsonSchemaObject.Property(n_Name.ValueString, n_Name, n_JsonSchemaElement.JsonSchema);
                        n_Name.SetSymbolUsage(JsonProperty, DataModel.SymbolUsageFlags.Declaration);
                    }
                }

                public      readonly    JsonSchemaObjectProperty[]          n_Properties;

                public                                                      JsonSchemaObject(LTTSQL.Core.ParserReader reader)
                {
                    ParseToken(reader, "OBJECT");
                    ParseToken(reader, Core.TokenID.LrBracket);

                    var properties = new List<JsonSchemaObjectProperty>();

                    do {
                        properties.Add(AddChild(new JsonSchemaObjectProperty(reader)));
                    }
                    while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                    ParseToken(reader, Core.TokenID.RrBracket);
                    n_Properties = properties.ToArray();
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    n_Properties.TranspileNodes(context);
                    base.TranspileNode(context);

                    var properties = new LTTSQL.DataModel.JsonSchemaObject.PropertyList(n_Properties.Length);

                    foreach (var p in n_Properties) {
                        if (p.JsonProperty.JsonSchema != null) {
                            if (!properties.TryAdd(p.JsonProperty))
                                context.AddError(p.n_Name, "Property '" + p.JsonProperty.Name + "' already defined.");
                        }
                    }

                    _jsonSchema = new LTTSQL.DataModel.JsonSchemaObject(properties);
                }
            }
            public class JsonSchemaArray: JsonSchemaElement
            {
                public      readonly    JsonSchemaElement                   n_JsonSchemaElement;

                public                                                      JsonSchemaArray(LTTSQL.Core.ParserReader reader)
                {
                    ParseToken(reader, "ARRAY");

                    switch(reader.CurrentToken.validateToken("OBJECT", "VALUE")) {
                    case "OBJECT":
                        n_JsonSchemaElement = new JsonSchemaObject(reader);
                        break;

                    case "VALUE":
                        n_JsonSchemaElement = new JsonSchemaValue(reader, true);
                        break;
                    }

                    ReadElement(reader);
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    base.TranspileNode(context);
                    n_JsonSchemaElement.TranspileNode(context);
                    _jsonSchema = new LTTSQL.DataModel.JsonSchemaArray(n_JsonSchemaElement.JsonSchema);
                }
            }
            public class JsonSchemaValue: JsonSchemaElement
            {
                public      readonly    LTTSQL.Core.AstParseNode            n_Type;

                public                  LTTSQL.DataModel.ISqlType           SqlType             { get { return ((LTTSQL.Node.ISqlType)n_Type).SqlType;  } }

                public                                                      JsonSchemaValue(LTTSQL.Core.ParserReader reader, bool hasValue)
                {
                    if (hasValue)
                        ParseToken(reader, "VALUE");

                    n_Type   = AddChild(ComplexType.CanParse(reader) ? (LTTSQL.Core.AstParseNode)new ComplexType(reader)
                                                                     : (LTTSQL.Core.AstParseNode)new LTTSQL.Node.Node_Datatype(reader));
                    ReadElement(reader);
                }
                public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
                {
                    _jsonSchema = null;
                    n_Type.TranspileNode(context);
                    base.TranspileNode(context);

                    _jsonSchema = (SqlType is DataModel.SqlTypeJson jsonType) ? jsonType.JsonSchema
                                                                              : new DataModel.JsonSchemaValue(SqlType, n_Flags);
                }
            }

            public                                                      JsonSchema(LTTSQL.Core.ParserReader reader)
            {
                ParseToken(reader, "WITH");
                n_Schema = AddChild(JsonSchemaElement.Parse(reader));
            }
            public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
            {
                _sqlType = null;
                n_Schema.TranspileNode(context);
                _sqlType = new LTTSQL.DataModel.SqlTypeJson(LTTSQL.DataModel.SqlTypeNative.NVarChar_MAX, n_Schema.JsonSchema);
            }
            public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
            {
                EmitCommentNewine(emitWriter);
            }
        }

        public                  LTTSQL.Core.Token                   n_json;
        public                  JsonSchema                          n_Schema        { get; private set; }

        public                  LTTSQL.DataModel.ISqlType           SqlType         { get { return n_Schema.SqlType;                        } }

        public      static      bool                                CanParse(LTTSQL.Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken("JSON");
        }
        public                                                      JsonType(LTTSQL.Core.ParserReader reader, bool parseSchema)
        {
            reader.CurrentToken.validateToken("JSON");
            n_json = Core.TokenWithSymbol.SetKeyword(reader.ReadToken(this, true));

            if (parseSchema) {
                AddChild(ParseSchema(reader));
            }
        }
        public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
        {
            n_Schema.TranspileNode(context);
        }
        public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (object.ReferenceEquals(c, n_json))
                    emitWriter.WriteText("nvarchar(max)");
                else
                if (c.isWhitespaceOrComment)
                    c.Emit(emitWriter);
            }
        }

        public                  JsonSchema                          ParseSchema(LTTSQL.Core.ParserReader reader)
        {
            return n_Schema = new JsonSchema(reader);
        }
    }
}
