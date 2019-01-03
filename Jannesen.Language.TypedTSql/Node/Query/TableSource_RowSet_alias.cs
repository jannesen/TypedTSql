using System;

namespace Jannesen.Language.TypedTSql.Node
{
    //  Data_TableSource_alias ::=
    //        Data_TableSource_alias_subquery
    //      | Data_TableSource_alias_local
    //      | Data_TableSource_alias_function
    //      | Data_TableSource_alias_object
    public abstract class TableSource_RowSet_alias: TableSource_RowSet
    {
        public      readonly    bool                                n_AllowAlias;
        public      override    Core.TokenWithSymbol                n_Alias             { get { return _n_Alias;  } }
        public      override    DataModel.RowSet                    t_RowSet            { get { return _t_RowSet; } }

        private                 Core.TokenWithSymbol                _n_Alias;
        private                 DataModel.RowSet                    _t_RowSet;

        public      static      TableSource_RowSet_alias            Parse(Core.ParserReader reader, bool allowAlias)
        {
            if (reader.CurrentToken.isNameOrQuotedName && reader.NextPeek().isToken(Core.TokenID.LrBracket)) {
                if (BuildIn.Catalog.RowSetFunctions.TryGetValue(reader.CurrentToken.Text, out Internal.BuildinFunctionDeclaration bfd))
                    return (TableSource_RowSet_alias)bfd.Parse(reader, allowAlias);
            }

            if (TableSource_RowSet_subquery.CanParse(reader))           return new TableSource_RowSet_subquery(reader, allowAlias);
            if (TableSource_RowSet_function.CanParse(reader))           return new TableSource_RowSet_function(reader, allowAlias);
            if (TableSource_RowSet_local.CanParse(reader))              return new TableSource_RowSet_local(reader, allowAlias);
            if (TableSource_RowSet_inserted_deleted.CanParse(reader))   return new TableSource_RowSet_inserted_deleted(reader, allowAlias);

            return new TableSource_RowSet_object(reader, allowAlias);
        }

        protected                                                   TableSource_RowSet_alias(bool allowAlias)
        {
            n_AllowAlias = allowAlias;
        }
        public                  void                                ParseTableAlias(Core.ParserReader reader)
        {
            if (n_AllowAlias) {
                if (ParseOptionalToken(reader, Core.TokenID.AS) != null || reader.CurrentToken.isNameOrQuotedName) {
                    _n_Alias = ParseName(reader);
                }
            }
        }

        protected               void                                TranspileRowSet(Transpile.Context context)
        {
            if (n_Alias != null) {
                _t_RowSet = new DataModel.RowSet(n_Alias.ValueString, ColumnList,
                                                 declaration:n_Alias,
                                                 source:t_Source);
                n_Alias.SetSymbol(_t_RowSet);
            }
            else {
                _t_RowSet = new DataModel.RowSet("", ColumnList,
                                                 source:t_Source);
            }

            var rowsets = context.RowSets;
            if (rowsets != null) {
                if (!rowsets.TryAdd(_t_RowSet))
                    context.AddError(n_Alias, "Rowset [" + _t_RowSet.Name + "] already defined.");
            }
        }
    }

    public abstract class TableSource_RowSetBuildIn: TableSource_RowSet_alias
    {
        public  readonly        Core.TokenWithSymbol                Name;

        internal                                                    TableSource_RowSetBuildIn(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader, bool allowAlias): base(allowAlias)
        {
            Name = (Core.TokenWithSymbol)ParseToken(reader);
            Name.SetSymbol(declaration);
        }
    }
}
