using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Expr_ServiceComplexType: ExprCalculation, IExprResponseNode, IReferencedEntity
    {
        public      readonly    Core.TokenWithSymbol            n_Name;
        public      readonly    DataModel.EntityName            n_EntityName;
        public      readonly    IExprNode[]                     n_Arguments;

        public      override    DataModel.ValueFlags            ValueFlags              { get { return _sqlResponseType != null ? DataModel.ValueFlags.Function|DataModel.ValueFlags.Nullable : DataModel.ValueFlags.Error;  } }
        public      override    DataModel.ISqlType              SqlType                 { get { return _sqlResponseType;                                                                                                     } }

        public                  DeclarationServiceComplexType   DeclarationComplexType  { get { return _complexType;                              } }
        public                  DataModel.ResponseNodeType      ResponseNodeType        { get { return _complexType.ResponseNode.n_NodeType;      } }
        public                  Query_Select_ColumnResponse[]   ResponseColumns         { get { return _complexType.ResponseNode.ResponseColumns; } }

        private                 DeclarationServiceComplexType   _complexType;
        private                 DataModel.SqlTypeResponseNode   _sqlResponseType;

        public      static new  bool                            CanParse(Core.ParserReader reader)
        {
            if (reader.CurrentToken.isToken(Core.TokenID.DoubleColon)) {
                var x = reader.Peek(3);

                if (x[1].isNameOrQuotedName && x[2].isToken(Core.TokenID.LrBracket))
                    return true;
            }

            return  false;
        }
        public                                                  Expr_ServiceComplexType(Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.DoubleColon);
            n_Name = ParseName(reader);

            ParseToken(reader, Core.TokenID.LrBracket);

            var expressions = new List<IExprNode>();

            do {
                expressions.Add(ParseExpression(reader));
            }  while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

            ParseToken(reader, Core.TokenID.RrBracket);

            n_Arguments = expressions.ToArray();
        }

        public                  DataModel.EntityName            getReferencedEntity(DeclarationObjectCode declarationObjectCode)
        {
            return DeclarationServiceComplexType.BuildEntityName(((DeclarationServiceMethod)declarationObjectCode).ServiceName, n_Name.ValueString);
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            _complexType = null;
            _sqlResponseType     = null;

            var name = n_Name.ValueString;

            var complexTypeEntityName = DeclarationServiceComplexType.BuildEntityName(context.GetDeclarationObject<DeclarationServiceMethod>().ServiceName, n_Name.ValueString);
            var complexTypeEntity     = (context.Catalog.GetObject(complexTypeEntityName, false) as DataModel.EntityObjectCode);

            if (!(complexTypeEntity?.DeclarationObjectCode is DeclarationServiceComplexType complexType)) {
                context.AddError(n_Name, "Unknown complextype '" + name + "'.");
                return;
            }

            n_Name.SetSymbol(complexTypeEntity);
            context.CaseWarning(n_Name, complexType.ComplexTypeName);

            _complexType = complexType;
            _sqlResponseType     = ((Expr_ResponseNode)(complexType.n_Statement)).SqlResponceType;

            n_Arguments.TranspileNodes(context);

            Validate.FunctionArguments(context, this, complexTypeEntity, n_Arguments);

            if (_sqlResponseType != null && _sqlResponseType.NodeType == DataModel.ResponseNodeType.ObjectMandatory) {
                if (_isArgumentNullable(n_Arguments)) {
                    _sqlResponseType = new DataModel.SqlTypeResponseNode(DataModel.ResponseNodeType.Object, (DataModel.ColumnList)_sqlResponseType.Columns);
                }
            }
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter)
        {
            throw new InvalidOperationException("Expr_ResponseComplexType.Emit");
        }
        public                  void                            EmitResponseNode(Core.EmitWriter emitWriter, string fieldname)
        {
            int     i = 0;

            // Emit pre spaces and comment.
            {
                while (!(Children[i] is Core.Token token && token.ID == Core.TokenID.LrBracket)) {
                    if (Children[i].isWhitespaceOrComment)
                        Children[i].Emit(emitWriter);

                    ++i;
                }
            }

            // Emit ( if not root node
            {
                if (fieldname != null)
                    Children[i].Emit(emitWriter);

                ++i;
            }

            // Emit SELECT * FROM and function call
            {
                emitWriter.WriteText("SELECT * FROM " + _complexType.Entity.EntityName.Fullname + "(");

                // Skip whitespaces
                while ((Children[i] is Core.Token token && (token.ID== Core.TokenID.WhiteSpace)))
                    ++i;

                // Emit arguments
                while (!(Children[i] is Core.Token token && token.ID == Core.TokenID.RrBracket))
                    Children[i++].Emit(emitWriter);

                emitWriter.WriteText(")");
            }

            // Emit FOR XML
            {
                var nodeName = fieldname != null ? SqlStatic.QuoteString(fieldname) : "'root'";

                emitWriter.WriteText(" FOR XML ");
                switch(((Expr_ResponseNode)_complexType.n_Statement).n_NodeType) {
                case DataModel.ResponseNodeType.Object:
                case DataModel.ResponseNodeType.ObjectMandatory:  emitWriter.WriteText("RAW(" + nodeName + ")");                break;
                case DataModel.ResponseNodeType.ArrayValue:      emitWriter.WriteText("PATH('value'),ROOT(" + nodeName + ")"); break;
                case DataModel.ResponseNodeType.ArrayObject:     emitWriter.WriteText("RAW('object'),ROOT(" + nodeName + ")"); break;
                }

                if (_hasBinaryColumn())
                    emitWriter.WriteText(",BINARY BASE64");

                emitWriter.WriteText(",TYPE");
            }

            // Emit ) if not root node
            {
                if (fieldname != null)
                    Children[i].Emit(emitWriter);

                ++i;
            }

            // Emit rest
            while (i < Children.Count) {
                if (Children[i] is Core.Token token && (token.isWhitespaceOrComment || token.ID == Core.TokenID.Semicolon))
                    token.Emit(emitWriter);

                ++i;
            }
        }

        private                 bool                            _hasBinaryColumn()
        {
            foreach(var c in ((Expr_ResponseNode)_complexType.n_Statement).n_Select.Resultset) {
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

        private                 bool                            _isArgumentNullable(IExprNode[] arguments)
        {
            foreach(var arg in arguments) {
                if (arg.isNullable())
                    return true;
            }

            return false;
        }
    }
}
