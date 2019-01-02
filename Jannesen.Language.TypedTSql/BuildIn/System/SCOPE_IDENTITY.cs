using System;
using Jannesen.Language.TypedTSql.Node;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.BuildIn;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class SCOPE_IDENTITY: ExprCalculationBuildIn
    {
        public      override    DataModel.ValueFlags        ValueFlags          { get { return _sqlType != null ? DataModel.ValueFlags.Function : DataModel.ValueFlags.Error;  } }
        public      override    DataModel.ISqlType          SqlType             { get { return _sqlType;  } }

        private                 DataModel.ISqlType          _sqlType;

        internal                                            SCOPE_IDENTITY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            _sqlType = context.ScopeIndentityType;
            if (_sqlType == null)
                throw new Exception("No previous insert statement.");
        }
    }
}
