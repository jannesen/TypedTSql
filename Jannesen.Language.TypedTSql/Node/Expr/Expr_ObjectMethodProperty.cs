using System;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_ObjectMethodProperty: ExprCalculation
    {
        public      readonly    IExprNode                       n_Value;
        public      readonly    Core.TokenWithSymbol            n_MethodName;
        public      readonly    Expr_Collection                 n_Arguments;

        public      override    DataModel.ValueFlags            ValueFlags          { get { return DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable;  } }
        public      override    DataModel.ISqlType              SqlType             { get { return _sqlType; } }
        public      override    bool                            NoBracketsNeeded    { get { return true;   } }

        private                 DataModel.ISqlType              _sqlType;

        public                                                  Expr_ObjectMethodProperty(Core.ParserReader reader, IExprNode valuenode)
        {
            AddChild(n_Value = valuenode);
            ParseToken(reader, Core.TokenID.Dot);
            n_MethodName = ParseName(reader);

            if (reader.CurrentToken.isToken(Core.TokenID.LrBracket)) {
                n_Arguments = AddChild(new Expr_Collection(reader, false));
            }
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _sqlType = null;

            try {
                n_Value.TranspileNode(context);
                n_Arguments?.TranspileNode(context);

                var sqlType = n_Value.SqlType;
                if (sqlType == null || sqlType is DataModel.SqlTypeAny) {
                    _sqlType = new DataModel.SqlTypeAny();
                }
                if ((sqlType as DataModel.SqlTypeNative)?.SystemType == DataModel.SystemType.Xml) {
                    var name = n_MethodName.ValueString;

                    if (n_Arguments == null)
                        throw new ErrorException("Unknown property '" + name + "'.");

                    _sqlType = Xml.Transpile(context, this, name, n_Arguments.n_Expressions);
                }
                else
                if ((sqlType.TypeFlags & DataModel.SqlTypeFlags.Interface) != 0) {
                    if (n_Arguments == null)
                        _sqlType = Validate.Property(sqlType.Interfaces, false, n_MethodName);
                    else
                        _sqlType = Validate.Method(sqlType.Interfaces, false, n_MethodName, n_Arguments.n_Expressions);
                }
                else
                    throw new ErrorException("Type '" + sqlType.ToString() + "' has no properties or methods.");
            }
            catch(Exception err) {
                context.AddError(this, err);
                _sqlType = new DataModel.SqlTypeAny();
            }
        }
    }
}
