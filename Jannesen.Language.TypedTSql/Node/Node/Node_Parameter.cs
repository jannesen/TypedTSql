using System;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public abstract class Node_Parameter: Core.AstParseNode
    {
        public      abstract    Core.TokenWithSymbol                n_Name              { get; }
        public      abstract    DataModel.Parameter                 Parameter           { get; }
    }

    // @parameter_name [ AS ] Datatype [ = default ] [ READONLY ]
    public class Node_SqlParameter: Node_Parameter
    {
        public enum InterfaceType
        {
            Function,
            Procedure,
            Interface
        };

        public      readonly    Node_Datatype               n_Type;
        public      readonly    IExprNode                   n_Default;
        public      readonly    DataModel.VariableFlags     n_Flags;

        public      override    Core.TokenWithSymbol        n_Name              => _Name;
        public      override    DataModel.Parameter         Parameter           => _parameter;

        private                 Core.TokenWithSymbol        _Name;
        private                 DataModel.Parameter         _parameter;

        public                                              Node_SqlParameter(Core.ParserReader reader, InterfaceType interfaceType)
        {
            _Name = (Core.TokenWithSymbol)ParseToken(reader, Core.TokenID.LocalName);
            n_Type = AddChild(new Node_Datatype(reader));

            if (interfaceType == InterfaceType.Procedure)
                ParseOptionalToken(reader, Core.TokenID.VARYING);

            n_Flags |= DataModel.VariableFlags.Nullable;

            if (ParseOptionalToken(reader, Core.TokenID.Equal) != null) {
                n_Flags |= DataModel.VariableFlags.HasDefaultValue;
                n_Default = ParseSimpleExpression(reader, constValue:true);
            }

            var tokenOutput   = ParseOptionalToken(reader, "OUT", "OUTPUT");
            var tokenReadOnly = ParseOptionalToken(reader, "READONLY");
            var tokenSaveCast = ParseOptionalToken(reader, "SAVECAST");

            if (tokenOutput != null) {
                n_Flags |= DataModel.VariableFlags.Output;

                if (interfaceType != InterfaceType.Procedure)
                    reader.AddError(new ParseException(tokenOutput, "Output not possible."));
            }

            if (tokenReadOnly != null) {
                n_Flags |= DataModel.VariableFlags.Readonly;

                if (interfaceType != InterfaceType.Function && interfaceType != InterfaceType.Procedure)
                    reader.AddError(new ParseException(tokenOutput, "Readonly not possible."));
            }

            if (tokenSaveCast != null) {
                n_Flags |= DataModel.VariableFlags.SaveCast;

                if ((n_Flags & DataModel.VariableFlags.Output) != 0)
                    reader.AddError(new ParseException(tokenSaveCast, "AutoCast with output not allowed."));
            }
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            _parameter = null;

            try {
                n_Type.TranspileNode(context);
                n_Default?.TranspileNode(context);

                _parameter = new DataModel.Parameter(n_Name.Text,
                                                     n_Type.SqlType != null ? n_Type.SqlType : new DataModel.SqlTypeAny(),
                                                     n_Name,
                                                     n_Flags,
                                                     n_Default?.getConstValue(n_Type.SqlType));
                n_Name.SetSymbolUsage(Parameter, DataModel.SymbolUsageFlags.Declaration);

                Validate.ConstByType(n_Type.SqlType, n_Default);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }
        public      override    void                        Emit(EmitWriter emitWriter)
        {
            foreach(var c in Children) {
                if (c is Core.Token && ((Core.Token)c).isToken("SAVECAST"))
                    continue;

                c.Emit(emitWriter);
            }
        }
    }
}
