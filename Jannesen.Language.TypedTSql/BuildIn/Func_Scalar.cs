using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.BuildIn
{
    public abstract class Func_Scalar: ExprCalculationBuildIn
    {
        public      readonly    Expr_Collection                     n_Arguments;

        public      override    DataModel.ValueFlags                ValueFlags          { get { return _result.ValueFlags;     } }
        public      override    DataModel.ISqlType                  SqlType             { get { return _result.SqlType;        } }
        public      override    string                              CollationName       { get { return _result.CollationName;  } }

        public                  FlagsTypeCollation                  _result;

        internal                                                    Func_Scalar(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            n_Arguments = AddChild(new Expr_Collection(reader, false));
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            try {
                n_Arguments.TranspileNode(context);
                _result = TranspileResult(n_Arguments.n_Expressions);
            }
            catch(Exception err) {
                _result.Clear();
                context.AddError(this, err);
            }
        }

        protected   virtual     FlagsTypeCollation                  TranspileResult(IExprNode[] arguments)
        {
            var result = new FlagsTypeCollation() { ValueFlags = transpileFlags() };

            if (result.ValueFlags.isValid()) {
                result.SqlType    = TranspileReturnType(arguments);

                if (result.SqlType == null)
                    throw new ErrorException(this.GetType().Name + "(" + argumentsTypes() + ") not possible.");
            }

            return result;
        }
        protected   virtual     DataModel.ISqlType                  TranspileReturnType(IExprNode[] arguments)
        {
            throw new NotImplementedException("TranspileReturnType not implemented");
        }

        protected               DataModel.ValueFlags                transpileFlags()
        {
            var     rtn = DataModel.ValueFlags.None;

            if (n_Arguments.n_Expressions != null) {
                foreach(var expr in n_Arguments.n_Expressions)
                    rtn |= expr.ValueFlags;
            }

            return LogicStatic.FunctionValueFlags(rtn);
        }
        protected               string                              argumentsTypes()
        {
            string      rtn = "";

            foreach(var expr in n_Arguments.n_Expressions) {
                var sqlType = expr.SqlType;

                string name = (sqlType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 ? sqlType.NativeType.ToString() : sqlType.GetType().Name;

                rtn += (rtn =="") ? expr.SqlType.ToString() : rtn + ", " + expr.SqlType.ToString();
            }

            return rtn;
        }
    }

    //TODO Func_Scalar_TODO validations
    public abstract class Func_Scalar_TODO: Func_Scalar
    {
        internal                                                    Func_Scalar_TODO(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
        }

        protected   override    DataModel.ISqlType                  TranspileReturnType(IExprNode[] arguments)
        {
            System.Diagnostics.Debug.WriteLine("BuildinFunction " + this.GetType().Name + " not implemented, default to SqlTypeAny.");
            return new DataModel.SqlTypeAny();
        }
    }
}
