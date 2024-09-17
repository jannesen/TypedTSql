using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class TranspileContext
    {
        public  readonly        Transpiler                          Transpiler;
        public  readonly        GlobalCatalog                       Catalog;

        private                 Dictionary<string, TAttribute>      _attributes;


        public                                                      TranspileContext(Transpiler transpiler, GlobalCatalog catalog)
        {
            Transpiler = transpiler;
            Catalog    = catalog;

            _attributes = new Dictionary<string, TAttribute>();
            DefineAttribute(new TAttribute(null, "description",     TAttributeType.String));
            DefineAttribute(new TAttribute(null, "min-length",      TAttributeType.Integer));
            DefineAttribute(new TAttribute(null, "max-length",      TAttributeType.Integer));
            DefineAttribute(new TAttribute(null, "min-value",       TAttributeType.Number));
            DefineAttribute(new TAttribute(null, "max-value",       TAttributeType.Number));
            DefineAttribute(new TAttribute(null, "precision",       TAttributeType.Integer));
            DefineAttribute(new TAttribute(null, "multiple-of",     TAttributeType.Number));
            DefineAttribute(new TAttribute(null, "pattern",         TAttributeType.String));
            DefineAttribute(new TAttribute(null, "date-accuracy",   TAttributeType.Number));
            DefineAttribute(new TAttribute(null, "select-source",   TAttributeType.Enum, new TAttributeEnumValue[] {
                                                                                             new TAttributeEnumValue(null, "static"),
                                                                                             new TAttributeEnumValue(null, "remote"),
                                                                                         }));
        }

        internal                void                                DefineAttribute(TAttribute attr)
        {
            if (_attributes.ContainsKey(attr.Name)) {
                throw new ArgumentException("attribute '" + attr.Name + "' already defined.");
            }

            _attributes.Add(attr.Name, attr);
        }
        public                  TAttribute                          FindAttribute(string name)
        {
            _attributes.TryGetValue(name, out var found);
            return found;
        }
    }
}
