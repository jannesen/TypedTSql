using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public interface IExprNode: DataModel.IExprResult, Core.IAstNode
    {
        ExprType                    ExpressionType          { get; }
        bool                        NoBracketsNeeded        { get; }
        object                      ConstValue();
        DataModel.Variable          GetVariable(Transpile.Context context);
        void                        TranspileNode(Transpile.Context context);
        void                        EmitSimple(Core.EmitWriter emitWriter);
    }

    public interface IParseContext
    {
        bool                        StatementCanParse(Core.ParserReader reader);
        Node.Statement              StatementParse(Core.ParserReader reader);
    }

    public interface ITableSource: Core.IAstNode
    {
        DataModel.ISymbol           getDataSource();
        DataModel.IColumnList       getColumnList(Transpile.Context context);
        void                        TranspileNode(Transpile.Context context);
    }

    public interface IWithDeclaration: Core.IAstNode
    {
        DataModel.IColumnList       getColumnList(Transpile.Context context, Node.IExprNode docexpr, Node.IExprNode pathexpr);
        void                        TranspileNode(Transpile.Context context);
    }

    public interface ISqlType
    {
        DataModel.ISqlType          SqlType             { get; }
    }
}
