using System;
using Jannesen.Language.TypedTSql.Transpile;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_SimpleWrapper: Core.AstParseNode, Node.IExprNode
    {
        public      DataModel.ValueFlags    ValueFlags              { get { return n_Expr.ValueFlags;       } }
        public      DataModel.ISqlType      SqlType                 { get { return n_Expr.SqlType;          } }
        public      string                  CollationName           { get { return n_Expr.CollationName;    } }
        public      ExprType                ExpressionType          { get { return _exprType;               } }
        public      bool                    NoBracketsNeeded        { get { return true;                    } }

        public      readonly                bool                    n_ConstValue;
        public      readonly                IExprNode               n_Expr;

        private                             ExprType                _exprType;
        private                             bool                    _constEmit;
        private                             object                  _value;

        public                                                      Expr_SimpleWrapper(IExprNode expr, bool constValue)
        {
            n_ConstValue = constValue;
            n_Expr       = expr;
            _exprType    = ExprType.NeedsTranspile;
            AddChild(expr);
        }

        public                              bool                    ValidateConst(DataModel.ISqlType sqlType)
        {
            return n_Expr.ValidateConst(sqlType);
        }
        public                              object                  ConstValue()
        {
            return n_Expr.ConstValue();
        }
        public                              DataModel.Variable      GetVariable(Transpile.Context context)
        {
            return n_Expr.GetVariable(context);
        }

        public      override                void                    TranspileNode(Context context)
        {
            _exprType  = ExprType.NeedsTranspile;
            _constEmit = false;
            _value     = null;

            n_Expr.TranspileNode(context);

            var exprType = n_Expr.ExpressionType;

            switch(exprType) {
            case ExprType.Const:
            case ExprType.Variable:
                _exprType = exprType;
                break;

            case ExprType.Complex:
                object value = n_Expr.ConstValue();

                if (value is Exception err) {
                    context.AddError(n_Expr, "Expression not allowed.");
                    return;
                }

                _exprType  = ExprType.Const;
                _constEmit = true;
                _value     = value;
                break;

            default:
                throw new InvalidOperationException("Invalid expressiontype " + exprType);
            }

            if (n_ConstValue && _exprType != ExprType.Const)
                context.AddError(n_Expr, "Constant expected.");
        }
        public      override                void                    Emit(Core.EmitWriter emitWriter)
        {
            if (_constEmit) {
                ((Core.AstParseNode)n_Expr).EmitCustom(emitWriter, (ew) => {
                                                                        ew.WriteValue(_value);
                                                                    });
            }
            else
                n_Expr.EmitSimple(emitWriter);
        }
        public                              void                    EmitSimple(Core.EmitWriter emitWriter)
        {
            Emit(emitWriter);
        }
    }
}
