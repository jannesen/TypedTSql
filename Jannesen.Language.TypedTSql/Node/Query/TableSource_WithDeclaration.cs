using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public enum TableSourceWithType
    {
        Xml  = 1,
        Json = 2
    }

    //  Data_SchemaDeclaration ::=
    //      WITH ( { Data_SchemaDeclarationColumn } [, ...n] )
    public class TableSource_WithDeclaration: Core.AstParseNode, IWithDeclaration
    {
        public      readonly    TableSource_WithDeclarationColumn[]         n_Columns;

        public                                                              TableSource_WithDeclaration(Core.ParserReader reader, TableSourceWithType type)
        {
            ParseToken(reader, Core.TokenID.WITH);
            ParseToken(reader, Core.TokenID.LrBracket);

            var columns = new List<TableSource_WithDeclarationColumn>();

            do {
                columns.Add(AddChild(new TableSource_WithDeclarationColumn(reader, type)));
            }
            while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            n_Columns = columns.ToArray();

            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public                  DataModel.IColumnList                       getColumnList(Transpile.Context context, Node.IExprNode docexpr, Node.IExprNode pathexpr)
        {
            var columnList = new DataModel.ColumnList(n_Columns.Length);

            foreach(var c in n_Columns) {
                var column = new DataModel.ColumnWith(c.n_Name.ValueString, c.n_Name, c.n_Type.SqlType);
                c.n_Name.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Declaration);

                if (!columnList.TryAdd(column))
                    context.AddError(c.n_Name, "Column [" + c.n_Name.ValueString + "] already declared.");
            }

            return columnList;
        }

        public      override    void                                        TranspileNode(Transpile.Context context)
        {
            n_Columns.TranspileNodes(context);
        }
    }
}
