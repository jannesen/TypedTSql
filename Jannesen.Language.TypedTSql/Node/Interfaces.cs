using System;

namespace Jannesen.Language.TypedTSql.Node
{
    public interface IExprNode: DataModel.IExprResult, Core.IAstNode
    {
        ExprType                    ExpressionType          { get; }
        DataModel.Variable          ReferencedVariable      { get; }
        DataModel.Column            ReferencedColumn        { get; }
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

    public interface IDataTarget: Core.IAstNode
    {
        bool                        isVarDeclare    { get; }
        DataModel.ISymbol           Table           { get; }
        DataModel.IColumnList       Columns         { get; }
        void                        TranspileNode(Transpile.Context context);
        DataModel.Column            GetColumnForAssign(string name, DataModel.ISqlType sqlType, string collationName, DataModel.ValueFlags flags, object declaration, DataModel.ISymbol nameReference, out bool declared);
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
    public interface ILoopStatement
    {
        void        UseGotoLabel(Core.Token token);
        string      GetGotoLabel(Core.Token token);
    }
}
