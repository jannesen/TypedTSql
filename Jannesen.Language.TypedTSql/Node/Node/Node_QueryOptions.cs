using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-us/library/ms190322.aspx
    public class Node_QueryOptions: Core.AstParseNode
    {
        public class OptimizeForVariable: Core.AstParseNode
        {
            public      readonly        Token.TokenLocalName        n_VariableName;
            public      readonly        IExprNode                   n_Value;

            public                                                  OptimizeForVariable(Core.ParserReader reader)
            {
                n_VariableName = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
                ParseToken(reader, Core.TokenID.Equal);
                n_Value = ParseExpression(reader);
            }

            public      override        void                        TranspileNode(Transpile.Context context)
            {
                try {
                    var variable = context.VariableGet(n_VariableName);
                    n_Value.TranspileNode(context);
                    if (n_Value.ExpressionType != ExprType.Const)
                        context.AddError(n_Value, "Expect constante");

                    if (variable != null) {
                        Validate.ConstByType(variable.SqlType, n_Value);
                    }
                }
                catch(Exception err) {
                    context.AddError(this, err);
                }
            }
        }

        public      readonly        DataModel.QueryOptions      n_Options;
        public      readonly        Core.Token                  n_Fast;
        public      readonly        Core.Token                  n_Maxdop;
        public      readonly        Core.Token                  n_Maxrecursion;
        public      readonly        Core.Token                  n_Min_grant_percent;
        public      readonly        Core.Token                  n_Max_grant_percent;
        public      readonly        Token.String                n_Use_plan;
        public      readonly        OptimizeForVariable[]       n_OptimizeForVariable;


        public                                                  Node_QueryOptions(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.OPTION);
            ParseToken(reader, Core.TokenID.LrBracket);

            do {
                var option = ParseEnum<DataModel.QueryOptions>(reader, _parseEnum);

                n_Options |= option;

                switch(option) {
                case DataModel.QueryOptions.FAST:
                    n_Fast = ParseInteger(reader);
                    break;

                case DataModel.QueryOptions.MAXDOP:
                    n_Maxdop = ParseInteger(reader);
                    break;

                case DataModel.QueryOptions.MAXRECURSION:
                    n_Maxrecursion = ParseInteger(reader);
                    break;

                case DataModel.QueryOptions.OPTIMIZE_FOR_VARIABLE: {
                        var optimizeForVariable = new List<OptimizeForVariable>();

                        do {
                            optimizeForVariable.Add(AddChild(new OptimizeForVariable(reader)));
                        }
                        while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                        n_OptimizeForVariable = optimizeForVariable.ToArray();
                    }

                    ParseToken(reader, Core.TokenID.RrBracket);
                    break;

                case DataModel.QueryOptions.MIN_GRANT_PERCENT:
                    n_Min_grant_percent = ParseToken(reader, Core.TokenID.Number);
                    break;

                case DataModel.QueryOptions.MAX_GRANT_PERCENT:
                    n_Max_grant_percent = ParseToken(reader, Core.TokenID.Number);
                    break;

                case DataModel.QueryOptions.USE_PLAN:
                    n_Use_plan = (Token.String)ParseToken(reader, Core.TokenID.String);
                    break;

                }
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override        void                        TranspileNode(Transpile.Context context)
        {
            n_OptimizeForVariable?.TranspileNodes(context);
            context.ValidateInteger(n_Maxdop,           0,     128);
            context.ValidateInteger(n_Fast,             1, 1000000);
            context.ValidateInteger(n_Maxrecursion,     0,   32767);
            context.ValidateNumber(n_Min_grant_percent, 0, 100.0);
            context.ValidateNumber(n_Max_grant_percent, 0, 100.0);
        }

        private static  Core.ParseEnum<DataModel.QueryOptions>  _parseEnum = new Core.ParseEnum<DataModel.QueryOptions>(
                                                                    "Query option",
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.FORCE_ORDER,                              "FORCE",            "ORDER"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.KEEP_PLAN,                                "KEEP",             "PLAN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.KEEPFIXED_PLAN,                           "KEEPFIXED",        "PLAN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.ROBUST_PLAN,                              "ROBUST",           "PLAN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.RECOMPILE,                                "RECOMPILE"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.LOOP_JOIN,                                "LOOP",             "JOIN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MERGE_JOIN,                               "MERGE",            "JOIN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.HASH_JOIN,                                "HASH",             "JOIN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.HASH_GROUP,                               "HASH",             "GROUP"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.ORDER_GROUP,                              "ORDER",            "GROUP"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.CONCAT_UNION,                             "CONCAT",           "UNION"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.HASH_UNION,                               "HASH",             "UNION"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MERGE_UNION,                              "MERGE",            "UNION"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.EXPAND_VIEWS,                             "EXPAND",           "VIEWS"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.FORCE_EXTERNALPUSHDOWN,                   "FORCE",            "EXTERNALPUSHDOWN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.DISABLE_EXTERNALPUSHDOWN,                 "DISABLE",          "EXTERNALPUSHDOWN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.PARAMETERIZATION_SIMPLE,                  "PARAMETERIZATION", "SIMPLE"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.PARAMETERIZATION_FORCED,                  "PARAMETERIZATION", "FORCED"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.NO_PERFORMANCE_SPOOL,                     "NO_PERFORMANCE_SPOOL"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX,    "IGNORE_NONCLUSTERED_COLUMNSTORE_INDEX"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.FAST,                                     "FAST"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MAXDOP,                                   "MAXDOP"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MAXRECURSION,                             "MAXRECURSION"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.OPTIMIZE_FOR_UNKNOWN,                     "OPTIMIZE", "for", "UNKNOWN"),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.OPTIMIZE_FOR_VARIABLE,                    "optimize", "for", Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MIN_GRANT_PERCENT,                        "MIN_GRANT_PERCENT", Core.TokenID.Equal),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.MAX_GRANT_PERCENT,                        "MAX_GRANT_PERCENT", Core.TokenID.Equal),
                                                                    new Core.ParseEnum<DataModel.QueryOptions>.Seq(DataModel.QueryOptions.USE_PLAN,                                 "USE", "PLAN")
                                                                );
    }
}
