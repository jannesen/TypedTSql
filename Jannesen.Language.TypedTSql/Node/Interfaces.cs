using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public interface IExprNode: DataModel.IExprResult, Core.IAstNode
    {
        ExprType                    ExpressionType          { get; }
        bool                        NoBracketsNeeded        { get; }
        object                      ConstValue();
        Token.TokenLocalName        GetVariableToken();
        void                        TranspileNode(Transpile.Context context);
        void                        EmitSimple(Core.EmitWriter emitWriter);
    }

    public interface IParseContext
    {
        Statement                   StatementParent                      { get; }
        bool                        StatementCanParse(Core.ParserReader reader);
        Statement                   StatementParse(Core.ParserReader reader);
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

    public enum VarDeclareScope
    {
        None            = 0,
        CodeScope,
        BlockScope
    }
    public interface ISetVariable: Core.IAstNode
    {
        Token.TokenLocalName        TokenName           { get; }
        VarDeclareScope             isVarDeclare        { get; }
    }
    public interface ILoopStatement
    {
        void        UseGotoLabel(Core.Token token);
        string      GetGotoLabel(Core.Token token);
    }
}
