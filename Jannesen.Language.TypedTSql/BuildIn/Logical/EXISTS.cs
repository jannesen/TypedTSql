using System;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    public class EXISTS: ExprBooleanBuildIn
    {
        // https://msdn.microsoft.com/en-us/library/ms188336.aspx
        // Expression_EXISTS ::=
        //      EXISTS ( subquery )

        public      readonly    Query_Select                n_Select;

        internal                                            EXISTS(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Select = AddChild(new Query_Select(reader, Query_SelectContext.ExpressionEXISTS));
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            try {
                var contextSubquery = new Transpile.ContextSubquery(context);

                n_Select.TranspileNode(contextSubquery);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
    }
}
