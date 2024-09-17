using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //  Data_TableSource_join
    //      ::= [ { INNER | { { LEFT | RIGHT | FULL } [ OUTER ] } } [ <join_hint> ] ] JOIN Data_TableSource_alias ON Data_TableSource_ON
    //        | CROSS JOIN Data_TableSource_alias
    //        | { CROSS | OUTER } APPLY Data_TableSource_alias_function
    public class TableSource_RowSet_join: TableSource_RowSet
    {
        public enum JoinOption
        {
            OPTIMIZER       = 0,
            LOOP,
            HASH,
            MERGE,
            REMOTE
        }

        public      readonly    Core.Token                      n_Join;
        public      readonly    Core.Token                      n_JoinOption;
        public      readonly    JoinOption                      n_JoinOptions;
        public      readonly    TableSource_RowSet_alias        n_RowSet;
        public      readonly    IExprNode                       n_OnExpr;
        public      override    Core.TokenWithSymbol            n_Alias             => n_RowSet.n_Alias;
        public      override    DataModel.IColumnList           ColumnList          => n_RowSet.ColumnList;
        public      override    DataModel.RowSet                t_RowSet            => n_RowSet.t_RowSet;
        public      override    DataModel.JoinType              n_JoinType          => _n_JoinType;

        private                 DataModel.JoinType              _n_JoinType;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(Core.TokenID.INNER, Core.TokenID.LEFT, Core.TokenID.RIGHT, Core.TokenID.FULL, Core.TokenID.CROSS, Core.TokenID.OUTER, Core.TokenID.JOIN);
        }
        public                                                  TableSource_RowSet_join(Core.ParserReader reader)
        {
            n_JoinOptions = JoinOption.OPTIMIZER;

            n_Join = ParseToken(reader, Core.TokenID.INNER, Core.TokenID.LEFT, Core.TokenID.RIGHT, Core.TokenID.FULL, Core.TokenID.CROSS, Core.TokenID.OUTER, Core.TokenID.JOIN);

            switch(n_Join.ID) {
            default:
                if (!n_Join.isToken(Core.TokenID.JOIN)) {
                    switch(n_Join.ID) {
                    case Core.TokenID.INNER:    _n_JoinType = DataModel.JoinType.INNER;       break;
                    case Core.TokenID.LEFT:     _n_JoinType = DataModel.JoinType.LEFT_OUTER;  break;
                    case Core.TokenID.RIGHT:    _n_JoinType = DataModel.JoinType.RIGHT_OUTER; break;
                    case Core.TokenID.FULL:     _n_JoinType = DataModel.JoinType.FULL_OUTER;  break;
                    }

                    if (n_Join.isToken(Core.TokenID.LEFT, Core.TokenID.RIGHT, Core.TokenID.FULL))
                        ParseOptionalToken(reader, Core.TokenID.OUTER);

                    n_JoinOption = ParseOptionalToken(reader, "LOOP", "HASH", "MERGE", "REMOTE");

                    if (n_JoinOption != null) {
                        switch(n_JoinOption.Text.ToUpperInvariant()) {
                        case "LOOP":    n_JoinOptions = JoinOption.LOOP;        break;
                        case "HASH":    n_JoinOptions = JoinOption.HASH;        break;
                        case "MERGE":   n_JoinOptions = JoinOption.MERGE;       break;
                        case "REMOTE":  n_JoinOptions = JoinOption.REMOTE;      break;
                        }
                    }

                    ParseToken(reader, Core.TokenID.JOIN);
                }
                else
                    _n_JoinType    = DataModel.JoinType.INNER;
                break;

            case Core.TokenID.CROSS:
                switch(ParseToken(reader, Core.TokenID.JOIN, Core.TokenID.APPLY).ID) {
                case Core.TokenID.JOIN:     _n_JoinType = DataModel.JoinType.CROSS_JOIN;      break;
                case Core.TokenID.APPLY:    _n_JoinType = DataModel.JoinType.CROSS_APPLY;     break;
                }
                break;

            case Core.TokenID.OUTER:
                ParseToken(reader, Core.TokenID.APPLY);
                _n_JoinType = DataModel.JoinType.OUTER_APPLY;
                break;
            }

            n_RowSet = AddChild(TableSource_RowSet_alias.Parse(reader));

            if (ParseOptionalToken(reader, Core.TokenID.ON) != null) {
                n_OnExpr = ParseExpression(reader);
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_RowSet.TranspileNode(context);

            switch (_n_JoinType) {
            case DataModel.JoinType.INNER:
            case DataModel.JoinType.LEFT_OUTER:
            case DataModel.JoinType.RIGHT_OUTER:
                if (n_OnExpr == null) {
                    context.AddError(this, "Expect ON expression after INNER/LEFT/RIGHT JOIN.");
                }
                break;

            case DataModel.JoinType.CROSS_APPLY:
            case DataModel.JoinType.OUTER_APPLY:
                if (n_OnExpr != null) {
                    context.AddError(n_OnExpr, "Don't expect ON expression after APPLY join.");
                }
                break;
            }

            if (n_JoinOptions != JoinOption.OPTIMIZER) {
                if ((context.QueryOptions & DataModel.QueryOptions.FORCE_ORDER) == 0)
                    context.AddWarning(n_JoinOption, "Missing OPTION FORCE ORDER.");
            }
        }
        public      override    bool                            SetUsage(DataModel.SymbolUsageFlags usage)
        {
            return n_RowSet.SetUsage(usage);
        }

        internal    override    void                            TranspileRowSet(Transpile.Context context, bool nullable)
        {
            n_RowSet.TranspileRowSet(context, nullable);
            n_OnExpr?.TranspileNode(context);
        }
    }
}
