using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    //  Data_TableSource_alias_function_udf ::=
    //      Objectname ( { Expression } [, 0..n] )
    public class TableSource_RowSet_function: TableSource_RowSet_alias
    {
        public      readonly    Node_EntityNameReference        n_Function;
        public      readonly    Expr_Collection                 n_FuncArguments;
        public      override    DataModel.IColumnList           ColumnList      { get { return _t_ColumnList ; } }

        private                 DataModel.IColumnList           _t_ColumnList;

        public      static      bool                            CanParse(Core.ParserReader reader)
        {
            Core.Token[]    token = reader.Peek(6);

            return (token[0].isNameOrQuotedName && token[1].isToken(Core.TokenID.LrBracket))
                || (token[0].isNameOrQuotedName && token[1].isToken(Core.TokenID.Dot) && token[2].isNameOrQuotedName  && token[3].isToken(Core.TokenID.LrBracket))
                || (token[0].isNameOrQuotedName && token[1].isToken(Core.TokenID.Dot) && token[2].isNameOrQuotedName  && token[3].isToken(Core.TokenID.Dot) && token[4].isNameOrQuotedName && token[5].isToken(Core.TokenID.LrBracket));
        }
        public                                                  TableSource_RowSet_function(Core.ParserReader reader)
        {
            n_Function      = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.FunctionTable, DataModel.SymbolUsageFlags.Select));
            n_FuncArguments = AddChild(new Expr_Collection(reader, false));

            ParseTableAlias(reader);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _t_ColumnList = null;

            n_Function.TranspileNode(context);
            n_FuncArguments.TranspileNode(context);

            _t_ColumnList = _transpileProcess(context);

            if (_t_ColumnList != null) {
                Validate.FunctionArguments(context, this, (DataModel.EntityObjectCode)n_Function.Entity, n_FuncArguments.n_Expressions);
            }
        }

        private                 DataModel.IColumnList           _transpileProcess(Transpile.Context context)
        {
            if (n_Function.Entity != null) {
                var sqlType = (n_Function.Entity as DataModel.EntityObjectCode)?.Returns;
                if (sqlType != null && (sqlType.TypeFlags & DataModel.SqlTypeFlags.Table) != 0) {
                    return sqlType.Columns;
                }
                else
                    context.AddError(this, "Function returns not a table.");
            }

            return new DataModel.ColumnListErrorStub();
        }
    }
}
