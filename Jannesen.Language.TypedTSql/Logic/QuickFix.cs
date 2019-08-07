using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.Logic
{
    public static class QuickFixLogic
    {
        public class FoundParameter
        {
            public      string                  Filename;
            public      Node.Node_SqlParameter     Parameter;
        }

        public  static  void                    QuickFix_Expr(Transpile.Context context, DataModel.ISqlType sqlType, Node.IExprNode expr)
        {
            if (expr.isConstant() &&
                ((sqlType.NativeType == expr.SqlType.NativeType)                                                                                     ||
                 (expr.SqlType.NativeType.SystemType == DataModel.SystemType.Char && (sqlType.NativeType.SystemType == DataModel.SystemType.VarChar  ||
                                                                                      sqlType.NativeType.SystemType == DataModel.SystemType.Char))   ||
                 (expr.SqlType.NativeType.SystemType == DataModel.SystemType.Int  && (sqlType.NativeType.SystemType == DataModel.SystemType.SmallInt ||
                                                                                      sqlType.NativeType.SystemType == DataModel.SystemType.TinyInt))))
            {
                if (sqlType?.Entity is DataModel.EntityType entityType) {
                    if (entityType != null && expr is Node.Expr_Constant exprConst) {
                        var constValue = expr.ConstValue();

                        if (constValue is string || constValue is int) {
                            var valueName = FindValueName(entityType, constValue);

                            if (valueName != null) {
                                throw new TranspileException(exprConst,
                                                                "Invalid type " + expr.SqlType.ToString() + " expect type " + sqlType.ToString() + ".",
                                                                new QuickFix(context.CreateDocumentSpan(exprConst),
                                                                            (constValue is int)
                                                                                ? ((int)constValue).ToString(System.Globalization.CultureInfo.InvariantCulture)
                                                                                : Library.SqlStatic.QuoteString((string)constValue),
                                                                            entityType.EntityName.GetRelativeName(context.Options.Schema) + "::" + Library.SqlStatic.QuoteName(valueName)));
                            }
                        }
                    }
                }
            }

            if (sqlType.NativeType == expr.SqlType.NativeType) {
                if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.UserType) != 0 &&
                    expr is Node.Expr_PrimativeValue primativeValue &&
                    primativeValue.Referenced is DataModel.Variable variable)
                    QuickFix_VariableType(context, primativeValue.n_Nodes[0], variable, sqlType);
            }
        }
        public  static  void                    QuickFix_VariableType(Transpile.Context context, Core.IAstNode errorNode, DataModel.Variable variable, DataModel.ISqlType expectedType)
        {
            if (variable is DataModel.Parameter) {
                var d = FindVariableDeclaration(context.GetDeclarationObject<Node.DeclarationObjectCode>().n_Parameters, variable);
                if (d is  Node.Node_SqlParameter declaration) {
                    throw new TranspileException(errorNode,
                                                 "Invalid type " + variable.SqlType.ToString() + " expect type " + expectedType.ToString() + ".",
                                                 new QuickFix(context.CreateDocumentSpan(declaration.n_Type),
                                                              expectedType.NativeType.ToSql(),
                                                              expectedType.Entity.EntityName.GetRelativeName(context.Options.Schema)));
                }

                if (d == null) {
                    var p = FindParameterDeclaration(context, variable);
                    if (p != null) {
                        throw new TranspileException(errorNode,
                                                     "Invalid type " + variable.SqlType.ToString() + " expect type " + expectedType.ToString() + ".",
                                                     new QuickFix(new DataModel.DocumentSpan(p.Filename, p.Parameter.n_Type),
                                                                  expectedType.NativeType.ToSql(),
                                                                  expectedType.Entity.EntityName.GetRelativeName(context.Options.Schema)));
                    }
                }
            }
            else {
                var d = FindVariableDeclaration(context.GetDeclarationObjectCode().n_Statement, variable);
                if (d is  Node.Statement_DECLARE.VariableTypeValue declaration) {
                    throw new TranspileException(errorNode,
                                                 "Invalid type " + variable.SqlType.ToString() + " expect type " + expectedType.ToString() + ".",
                                                 new QuickFix(context.CreateDocumentSpan(declaration.n_Type),
                                                              expectedType.NativeType.ToSql(),
                                                              expectedType.Entity.EntityName.GetRelativeName(context.Options.Schema)));
                }
            }
        }
        public  static  string                  FindValueName(DataModel.EntityType entityType, object value)
        {
            if (entityType.Values != null) {
                foreach(var v in entityType.Values) {
                    if (v.Value.Equals(value))
                        return v.Name;
                }
            }

            return null;
        }
        public  static  FoundParameter          FindParameterDeclaration(Transpile.Context context, DataModel.Variable parameter)
        {
            foreach (var entityDeclation in context.Transpiler.EntityDeclarations) {
                var d = FindVariableDeclaration((entityDeclation.Declaration as Node.DeclarationObjectCode)?.n_Parameters, parameter);
                if (d is Node.Node_SqlParameter)
                    return new FoundParameter()
                            {
                                Filename  = entityDeclation.SourceFile.Filename,
                                Parameter = (Node.Node_SqlParameter)d
                            };
            }

            return null;
        }
        public  static  object                  FindVariableDeclaration(Core.AstParseNode node, DataModel.Variable variable)
        {
            if (node != null && node.Children != null) {
                foreach(var n in node.Children) {
                    if ((n is Node.Node_SqlParameter) && object.ReferenceEquals(((Node.Node_SqlParameter)n).Parameter, variable))
                        return n;

                    if ((n is Node.Statement_DECLARE.VariableTypeValue) && object.ReferenceEquals(((Node.Statement_DECLARE.VariableTypeValue)n).Variable, variable))
                        return n;

                    if (n is Core.AstParseNode) {
                        var r = FindVariableDeclaration((Core.AstParseNode)n, variable);
                        if (r != null)
                            return r;
                    }
                }
            }

            return null;
        }
    }
}
