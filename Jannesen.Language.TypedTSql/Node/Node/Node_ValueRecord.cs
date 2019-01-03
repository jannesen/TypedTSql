using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_ValueRecord: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    IExprNode                       n_Value;
        public      readonly    Node_ValueField[]               n_Fields;
        public                  DataModel.ValueRecord           ValueRecord                 { get; private set; }

        public                                                  Node_ValueRecord(Core.ParserReader reader)
        {
            n_Name = ParseName(reader);
            ParseToken(reader, Core.TokenID.Equal);
            n_Value = ParseExpression(reader);

            if (ParseOptionalToken(reader, Core.TokenID.LrBracket) != null) {
                var fields = new List<Node_ValueField>();

                do {
                    fields.Add(new Node_ValueField(reader));
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);

                n_Fields = fields.ToArray();
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            ValueRecord = null;

            n_Fields?.TranspileNodes(context);
            n_Value.TranspileNode(context);

            ValueRecord = _createValueRecord(context);
            n_Name.SetSymbol(ValueRecord);
        }

        public                  void                            TranspileFields(DataModel.ValueRecordFieldList fields)
        {
            if (n_Fields != null) {
                foreach(var field in n_Fields)
                    field.TranspileField(fields);
            }
        }
        private                 DataModel.ValueRecord           _createValueRecord(Transpile.Context context)
        {
            DataModel.ValueFieldList    fields = null;

            if (n_Fields != null) {
                fields = new DataModel.ValueFieldList(n_Fields.Length);

                foreach(var f in n_Fields) {
                    if (f.ValueField != null) {
                        if (!fields.TryAdd(f.ValueField))
                            context.AddError(f.n_Name, "Field [" + f.ValueField.Name + "] already defined.");
                    }
                }
            }

            return new DataModel.ValueRecord(n_Name.ValueString,
                                             n_Value.getConstValue(),
                                             n_Name,
                                             fields);
        }
    }
}
