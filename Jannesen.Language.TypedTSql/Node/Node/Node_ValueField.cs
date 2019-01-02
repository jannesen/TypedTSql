using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_ValueField: Core.AstParseNode
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    IExprNode                       n_Value;
        public                  DataModel.ValueField            ValueField          { get; private set; }

        public                                                  Node_ValueField(Core.ParserReader reader)
        {
            n_Name  = ParseName(reader);
            ParseToken(reader, Core.TokenID.Equal);
            n_Value = ParseExpression(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Value.TranspileNode(context);

            ValueField = new DataModel.ValueField(n_Name.ValueString,
                                                  n_Value.getConstValue());
        }

        internal                void                            TranspileField(DataModel.ValueRecordFieldList fields)
        {
            var name = n_Name.ValueString;

            if (!fields.TryGetValue(name, out var field))
                fields.Add(field = new DataModel.ValueRecordField(name));

            n_Name.SetSymbol(field);
        }
    }
}
