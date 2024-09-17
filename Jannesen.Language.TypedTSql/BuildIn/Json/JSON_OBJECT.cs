using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/json-object-transact-sql
    public class JSON_OBJECT: ExprCalculationBuildIn
    {
        public class KeyValue: Core.AstParseNode
        {
            public      readonly    IExprNode                   n_Key;
            public      readonly    IExprNode                   n_Value;

            internal                                            KeyValue(Core.ParserReader reader)
            {
                n_Key = ParseExpression(reader);
                ParseToken(reader, Core.TokenID.Colon);
                n_Value = ParseExpression(reader);
            }

            public      override    void                        TranspileNode(Transpile.Context context)
            {
                n_Key.TranspileNode(context);
                n_Value.TranspileNode(context);

                Validate.ValueString(n_Key);
            }
        }

        public      readonly    KeyValue[]                  n_Properties;

        public      override    DataModel.ValueFlags        ValueFlags          => DataModel.ValueFlags.ValueExpression;
        public      override    DataModel.ISqlType          SqlType             => DataModel.SqlTypeNative.NVarChar_MAX;


        internal                                            JSON_OBJECT(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            var properties = new List<KeyValue>();

            do {
                properties.Add(AddChild(new KeyValue(reader)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Properties = properties.ToArray();

            if (ParseOptionalToken(reader, "NULL", "ABSENT") != null) {
                ParseToken(reader, "ON");
                ParseToken(reader, "NULL");
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                n_Properties.TranspileNodes(context);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
