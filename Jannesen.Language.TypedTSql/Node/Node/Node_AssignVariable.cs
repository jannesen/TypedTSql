using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_AssignVariable: Node_VarVariable, DataModel.IExprResult
    {
        public                  DataModel.ValueFlags                ValueFlags
        {
            get {
                return Variable != null && Variable.isNullable ? DataModel.ValueFlags.Variable|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Variable;
            }
        }
        public                  DataModel.ISqlType                  SqlType
        {
            get {
                return Variable?.SqlType ?? DataModel.SqlTypeAny.Instance;
            }
        }
        public                  string                              CollationName       { get { return null;                            } }

        public                                                      Node_AssignVariable(Core.ParserReader reader): base(reader)
        {
        }

        public                  void                                TranspileAssign(Transpile.Context context, DataModel.ISqlType type, bool readWrite=false)
        {
            if (n_Scope != VarDeclareScope.None) {
                if (type == null) {
                    context.AddError(n_Name, "Can't determin type of variable.");
                }

                Variable = context.VarVariableSet(n_Name, n_Scope, type ?? DataModel.SqlTypeAny.Instance);
            }
            else { 
                Variable = context.VariableGet(n_Name);

                if (Variable != null) {
                    if (!Variable.isReadonly) {
                        Variable.setAssigned();
                    }
                    else {
                        context.AddError(this, "Not allowed to assign a readonly variable.");
                    }

                    if (type != null) { 
                        try {
                            if (!Validate.Assign(Variable.SqlType, type)) {
                                throw new Exception("Not allowed to assign '" + type + "' to '" + Variable.SqlType + "'");
                            }
                        }
                        catch(Exception err) {
                            context.AddError(this, err);
                        }
                    }
                }
            }

            _setSymbolUsage(readWrite);
        }
        public                  void                                TranspileAssign(Transpile.Context context, DataModel.IExprResult expr, bool readWrite=false)
        {
            if (n_Scope != VarDeclareScope.None) {
                Variable = context.VarVariableSet(n_Name, n_Scope, expr.SqlType);
            }
            else { 
                Variable = context.VariableGet(n_Name);
                context.VariableSet(this, Variable, expr);
            }

            _setSymbolUsage(readWrite);
        }

        public                  bool                                ValidateConst(DataModel.ISqlType sqlType)
        {
            return false;
        }

        private                 void                                _setSymbolUsage(bool readWrite)
        {
            if (Variable != null) {
                var usage = DataModel.SymbolUsageFlags.Write;
                if (readWrite)                       usage |= DataModel.SymbolUsageFlags.Read;
                if (n_Scope != VarDeclareScope.None) usage |= DataModel.SymbolUsageFlags.Declaration;

                n_Name.SetSymbolUsage(Variable, usage);
            }
        }
    }
}
