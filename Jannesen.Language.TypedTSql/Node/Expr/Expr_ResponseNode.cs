using System;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public interface IExprResponseNode
    {
        DataModel.ResponseNodeType          ResponseNodeType        { get; }
        Query_Select_ColumnResponse[]       ResponseColumns         { get; }

        void                                EmitResponseNode(Core.EmitWriter emitWriter, string fieldname);
    }

    public class Expr_ResponseNode: ExprCalculation, IExprResponseNode
    {
        public      readonly    DataModel.ResponseNodeType      n_NodeType;
        public      readonly    Query_Select                    n_Select;
        public      override    DataModel.ValueFlags            ValueFlags          { get { return SqlResponceType != null ? DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Error;  } }
        public      override    DataModel.ISqlType              SqlType             { get { return SqlResponceType;                                                                                                     } }

        public                  DataModel.ResponseNodeType      ResponseNodeType     { get { return n_NodeType;       } }
        public                  Query_Select_ColumnResponse[]   ResponseColumns      { get; private set; }
        public                  DataModel.SqlTypeResponseNode   SqlResponceType      { get; private set; }


        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            return _responseType.CanParse(reader);
        }
        public                                                  Expr_ResponseNode(Core.ParserReader reader)
        {
            n_NodeType = _responseType.Parse(this, reader);
            AddLeading(reader);
            n_Select = AddChild(new Query_Select(reader, n_NodeType == DataModel.ResponseNodeType.ArrayValue ? Query_SelectContext.ExpressionResponseValue :  Query_SelectContext.ExpressionResponseObject));
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            ResponseColumns = null;
            SqlResponceType         = null;

            try {
                var contextSubquery = new Transpile.ContextSubquery(context);

                n_Select.TranspileNode(contextSubquery);

                DataModel.IColumnList columns;

                if (n_NodeType != DataModel.ResponseNodeType.ArrayValue) {
                    columns = n_Select.Resultset.GetUniqueNamedList();
                }
                else {
                    columns = n_Select.Resultset;
                    if (columns.Count != 1) {
                        context.AddError(this, "ARRAY VALUE only has 1 unnamed column.");
                        return;
                    }
                }


                var responseColumns = n_Select.n_Selects[0].n_Columns.n_Columns;

                ResponseColumns = new Query_Select_ColumnResponse[responseColumns.Length];
                for (int i = 0 ; i < responseColumns.Length ; ++i)
                    ResponseColumns[i] = (Query_Select_ColumnResponse)(responseColumns[i]);

                SqlResponceType = new DataModel.SqlTypeResponseNode(n_NodeType, columns);
            }
            catch(Exception err) {
                context.AddError(this, err);
            }
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            throw new InvalidOperationException("Expr_ResponseNode.Emit");
        }

        public                  void                            EmitResponseNode(Core.EmitWriter emitWriter, string fieldname)
        {
            int     i = 0;

            // Emit pre spaces and comment.
            while (!(Children[i] is Core.Token token && token.ID == Core.TokenID.LrBracket)) {
                if (Children[i].isWhitespaceOrComment)
                    Children[i].Emit(emitWriter);

                ++i;
            }


            // Emit ( if not root node
            {
                if (fieldname != null)
                    Children[i].Emit(emitWriter);

                ++i;
            }

            // Emit Select
            {
                while (Children[i] != n_Select)
                    Children[i++].Emit(emitWriter);

                var indent_for = emitWriter.Linepos;
                n_Select.Emit(emitWriter);
                ++i;

                emitWriter.WriteNewLine(indent_for + 2, " FOR XML ");

                var nodeName = fieldname != null ? SqlStatic.QuoteString(fieldname) : "'root'";

                switch(n_NodeType) {
                case DataModel.ResponseNodeType.Object:
                case DataModel.ResponseNodeType.ObjectMandatory: emitWriter.WriteText("RAW(" + nodeName + ")");                break;
                case DataModel.ResponseNodeType.ArrayValue:      emitWriter.WriteText("PATH(''),ROOT(" + nodeName + ")");      break;
                case DataModel.ResponseNodeType.ArrayObject:     emitWriter.WriteText("RAW('object'),ROOT(" + nodeName + ")"); break;
                }

                if (_hasBinaryColumn())
                    emitWriter.WriteText(",BINARY BASE64");

                emitWriter.WriteText(",TYPE");
                emitWriter.WriteNewLine(1);
            }

            // Emit ) if not root node
            {
                while (!(Children[i] is Core.Token token && token.ID == Core.TokenID.RrBracket))
                    Children[i++].Emit(emitWriter);

                if (fieldname != null)
                    Children[i].Emit(emitWriter);

                ++i;
            }

            // Emit rest
            {
                while (i < Children.Count) {
                    if (Children[i] is Core.Token token && (token.isWhitespaceOrComment || token.ID == Core.TokenID.Semicolon))
                        token.Emit(emitWriter);

                    ++i;
                }
            }
        }
        public                  void                            EmitComplexTypeReturn(Core.EmitWriter emitWriter)
        {
            int     i = 0;

            while (Children[i].isWhitespaceOrComment)
                Children[i++].Emit(emitWriter);

            emitWriter.WriteText("TABLE RETURN");

            for (;;) {
                if (Children[i++] is Core.Token token) {
                    if (token.isWhitespaceOrComment)
                        token.Emit(emitWriter);
                    else
                    if (token.ID == Core.TokenID.LrBracket) {
                        token.Emit(emitWriter);
                        break;
                    }
                }
            }

            while (i < Children.Count)
                Children[i++].Emit(emitWriter);
        }

        private                 bool                            _hasBinaryColumn()
        {
            foreach(var c in n_Select.Resultset) {
                var sqlType = c.SqlType;

                if (sqlType is DataModel.SqlTypeNative sqlTypeNative) {
                    if (sqlTypeNative.SystemType == DataModel.SystemType.Binary || sqlTypeNative.SystemType == DataModel.SystemType.VarBinary)
                        return true;
                }
                else
                if (sqlType is DataModel.EntityTypeExternal)
                    return true;
            }

            return false;
        }

        private     static      Core.ParseEnum<DataModel.ResponseNodeType>      _responseType = new Core.ParseEnum<DataModel.ResponseNodeType>(
                                                                    "ResponseNodeType",
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.Object,          "OBJECT",               Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ObjectMandatory, "OBJECT_MANDATORY",     Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ArrayValue,      "ARRAY_VALUE",          Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ArrayObject,     "ARRAY_OBJECT",         Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ObjectMandatory, "OBJECT", "MANDATORY",  Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ArrayValue,      "ARRAY",  "VALUE",      Core.TokenID.LrBracket),
                                                                    new Core.ParseEnum<DataModel.ResponseNodeType>.Seq(DataModel.ResponseNodeType.ArrayObject,     "ARRAY",  "OBJECT",     Core.TokenID.LrBracket)
                                                                );

    }
}
