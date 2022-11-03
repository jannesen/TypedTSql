using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_Variable: Expr
    {
        public      readonly    Token.TokenLocalName                n_Variable;

        public      override    DataModel.ValueFlags                ValueFlags
        {
            get {
                return _variable != null && _variable.isNullable ? DataModel.ValueFlags.Variable|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Variable;
            }
        }
        public      override    DataModel.ISqlType                  SqlType
        {
            get {
                return _variable?.SqlType ?? DataModel.SqlTypeAny.Instance;
            }
        }
        public      override    ExprType                            ExpressionType      { get { return ExprType.Variable;   } }
        public      override    DataModel.Variable                  ReferencedVariable  { get { return _variable;           } }
        public      override    bool                                NoBracketsNeeded    { get { return true;                } }

        private                 DataModel.Variable                  _variable;

        public                                                      Expr_Variable(Core.ParserReader reader)
        {
            n_Variable = (Token.TokenLocalName)ParseToken(reader, Core.TokenID.LocalName);
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                _variable = context.VariableGet(n_Variable, allowGlobal:true);
                if (_variable != null) {
                    _variable?.setUsed();
                    n_Variable.SetSymbolUsage(_variable, DataModel.SymbolUsageFlags.Read);
                }
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
        public      override    Token.TokenLocalName                GetVariableToken()
        {
            return n_Variable;
        }
    }
}
