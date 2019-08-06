using System;
using System.Text.RegularExpressions;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.Logic
{
    class Xml
    {
        private static  Regex                       _regexXQueryVariableReference = new Regex("sql:variable\\(\"(@[a-zA-Z0-9@$_]+)\"\\)", RegexOptions.CultureInvariant);

        public  static  DataModel.ISqlType          Transpile(Transpile.Context context, Core.IAstNode node, string methodName, IExprNode[] arguments)
        {
            switch(methodName.ToLowerInvariant()) {
            case "query":
                Validate.NumberOfArguments(arguments, 1);
                _transpileXQuery(context, node, arguments[0]);
                return DataModel.SqlTypeNative.Xml;

            case "value":
                Validate.NumberOfArguments(arguments, 2);
                _transpileXQuery(context, node, arguments[0]);
                var stype = Validate.ConstString(arguments[1]);

                return stype != null ? (DataModel.ISqlType)DataModel.SqlTypeNative.ParseNativeType(stype) : (DataModel.ISqlType)new DataModel.SqlTypeAny();

            case "exists":
                Validate.NumberOfArguments(arguments, 1);
                _transpileXQuery(context, node, arguments[0]);
                return DataModel.SqlTypeNative.Bit;

            case "modify":
                Validate.NumberOfArguments(arguments, 1);
                Validate.ValueString(arguments[0]);
                return new DataModel.SqlTypeVoid();

            case "nodes":
                throw new NotImplementedException("xml.nodes() not implemented");

            default:
                throw new ErrorException("Unknown method '" + methodName + "'.");
            }
        }

        private static  void                        _transpileXQuery(Transpile.Context context, Core.IAstNode node, IExprNode expr)
        {
            Validate.ValueString(expr);

            if (expr.isConstant()) {
                var xquery = Validate.ConstString(expr);

                if (xquery != null) {
                    foreach (Match m in _regexXQueryVariableReference.Matches(xquery)) {
                        var variable = context.VariableGet(node, m.Groups[1].Value, false);

                        if (variable != null)
                            variable.setUsed();
                    }
                }
            }
        }
    }
}
