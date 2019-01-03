using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms181765.aspx
    // Expression_CASE
    //      : CASE ( Expression ) ?
    //             (WHEN Expression THEN Expression)+
    //             (ELSE Expression)?
    //        END
    //      ;
    public class Expr_CASE: ExprCalculation
    {
        public class WHEN: Core.AstParseNode
        {
            public      readonly    IExprNode       n_When;
            public      readonly    IExprNode       n_Result;

            public                                  WHEN(Core.ParserReader reader)
            {
                ParseToken(reader, Core.TokenID.WHEN);
                n_When = ParseExpression(reader);
                ParseToken(reader, Core.TokenID.THEN);
                n_Result = ParseExpression(reader);
            }

            public      override    void            TranspileNode(Transpile.Context context)
            {
                n_When.TranspileNode(context);
                n_Result.TranspileNode(context);
            }
        }

        public      readonly    IExprNode               n_Input;
        public      readonly    WHEN[]                  n_When;
        public      readonly    IExprNode               n_Else;

        public      override    DataModel.ValueFlags    ValueFlags          { get { return _result.ValueFlags;     } }
        public      override    DataModel.ISqlType      SqlType             { get { return _result.SqlType;        } }
        public      override    string                  CollationName       { get { return _result.CollationName;  } }
        public      override    bool                    NoBracketsNeeded    { get { return true; } }

        public                  FlagsTypeCollation      _result;

        public                                          Expr_CASE(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.CASE);

            if (reader.CurrentToken.ID != Core.TokenID.WHEN)
                n_Input = ParseExpression(reader);

            var when = new List<WHEN>();

            while (reader.CurrentToken.isToken(Core.TokenID.WHEN))
                when.Add(AddChild(new WHEN(reader)));

            n_When = when.ToArray();

            if (ParseOptionalToken(reader, Core.TokenID.ELSE) != null)
                n_Else = ParseExpression(reader);

            ParseToken(reader, Core.TokenID.END);
        }

        public      override    void                    TranspileNode(Transpile.Context context)
        {
            _result.Clear();

            try {
                n_Input?.TranspileNode(context);
                n_When.TranspileNodes(context);
                n_Else?.TranspileNode(context);

                _transpileWhenExpr(context);
                _transpileWhenResult(context);
            }
            catch(Exception err) {
                _result.Clear();
                context.AddError(this, err);
            }
        }
        public      override    bool                    ValidateConst(DataModel.ISqlType sqlType)
        {
            bool    rtn = true;

            foreach(var when in n_When) {
                if (!when.n_Result.ValidateConst(sqlType))
                    rtn = false;
            }

            if (n_Else != null) {
                if (!n_Else.ValidateConst(sqlType))
                    rtn = false;
            }

            return rtn;
        }

        private                 void                    _transpileWhenExpr(Transpile.Context context)
        {
            foreach (var when in n_When) {
                try {
                    if (n_Input != null)
                        TypeHelpers.OperationCompare(context, null, n_Input, when.n_When);
                    else
                        Validate.BooleanExpression(when.n_When);
                }
                catch(Exception err) {
                    context.AddError(when.n_When, err);
                }
            }
        }
        private                 void                    _transpileWhenResult(Transpile.Context context)
        {
            // Create list of all possible results.
            var expressions = new IExprNode[n_When.Length + (n_Else != null ? 1 : 0)];

            for(int i = 0 ; i < n_When.Length ; ++i)
                expressions[i] = n_When[i].n_Result;

            if (n_Else != null)
                expressions[n_When.Length] = n_Else;

            _result = TypeHelpers.OperationUnion(expressions);
        }
    }
}
