using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    // https://learn.microsoft.com/en-us/sql/t-sql/functions/json-array-transact-sql
    public class JSON_ARRAY: ExprCalculationBuildIn
    {
        public      readonly    IExprNode[]                 n_Arguments;

        public      override    DataModel.ValueFlags        ValueFlags          => DataModel.ValueFlags.ValueExpression;
        public      override    DataModel.ISqlType          SqlType             => DataModel.SqlTypeNative.NVarChar_MAX;

        internal                                            JSON_ARRAY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);

            var arguments = new List<IExprNode>();

            do {
                arguments.Add(ParseExpression(reader));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Arguments = arguments.ToArray();

            if (ParseOptionalToken(reader, "NULL", "ABSENT") != null) {
                ParseToken(reader, "ON");
                ParseToken(reader, "NULL");
            }

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Arguments.TranspileNodes(context);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
