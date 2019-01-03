using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_Values: Core.AstParseNode
    {
        public      readonly    Node_ValueRecord[]              n_Records;
        public                  DataModel.ValueRecordFieldList  Fields                      { get; private set; }
        public                  DataModel.ValueRecordList       ValuesRecords               { get; private set; }

        public                                                  Node_Values(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.VALUES);
            ParseToken(reader, Core.TokenID.LrBracket);

            var n_records = new List<Node_ValueRecord>();

            do {
                n_records.Add(new Node_ValueRecord(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Records = n_records.ToArray();
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            Fields        = null;
            ValuesRecords = null;

            n_Records.TranspileNodes(context);
            _transpileValues(context);
        }
        public                  void                            TranspileNode(TypeDeclaration_User typeDeclaration, Transpile.Context context)
        {
            var nativeType = typeDeclaration.NativeType;

            if (nativeType != null) {
                foreach (var r in n_Records) {
                    try {
                        Validate.ConstByType(nativeType, r.n_Value);
                    }
                    catch(Exception err) {
                        context.AddError(r, err);
                    }
                }
            }
        }

        private                 void                            _transpileValues(Transpile.Context context)
        {
            var fields  = new DataModel.ValueRecordFieldList(16);
            var records = new DataModel.ValueRecordList(n_Records.Length);

            foreach(var r in n_Records) {
                if (r.ValueRecord != null) {
                    r.TranspileFields(fields);

                    if (!records.TryAdd(r.ValueRecord))
                        context.AddError(r.n_Name, "Value '" + r.ValueRecord.Name + "' already defined.");
                }
            }

            fields.OptimizeSize();
            Fields        = fields;
            ValuesRecords = records;
        }
    }
}
