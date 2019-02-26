using System;
using System.IO;
using System.Xml;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public class RETURNS: LTTSQL.Node.Statement
    {
        public      readonly    LTTSQL.Node.IExprNode               n_Expression;
        public      readonly    LTTSQL.Node.Node_QueryOptions       n_QueryOptions;

        public                  DataModel.ISqlType                  SqlType             { get; private set; }
        public                  string                              ResponseTypeName    { get; private set; }

        public      static      bool                                CanParse(LTTSQL.Core.ParserReader reader)
        {
            return reader.CurrentToken.isToken(LTTSQL.Core.TokenID.RETURNS);
        }
        public                                                      RETURNS(LTTSQL.Core.ParserReader reader)
        {
            ParseToken(reader, Core.TokenID.RETURNS);
            n_Expression = ParseExpression(reader, LTTSQL.Node.ParseExprContext.ServiceReturns);

            if (reader.CurrentToken.isToken(Core.TokenID.OPTION))
                n_QueryOptions = AddChild(new LTTSQL.Node.Node_QueryOptions(reader));

            ParseStatementEnd(reader, false);
        }

        public      override    void                                TranspileNode(LTTSQL.Transpile.Context context)
        {
            this.SqlType = null;

            if (n_Expression is LTTSQL.Node.IExprResponseNode) {
                var contextStatement = new LTTSQL.Transpile.ContextStatementQuery(context);

                if (n_QueryOptions != null) {
                    n_QueryOptions.TranspileNode(contextStatement);
                    contextStatement.SetQueryOptions(n_QueryOptions.n_Options);
                }

                n_Expression.TranspileNode(contextStatement);
                this.SqlType          = n_Expression.SqlType;
                this.ResponseTypeName = _responseMsgName(SqlType);
            }
            else
            if (n_Expression != null) {
                n_Expression?.TranspileNode(context);
                SqlType = n_Expression.SqlType;

                if (SqlType != null && (SqlType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) == 0)
                    context.AddError(n_QueryOptions, "Invalid type.");

                if (n_QueryOptions != null)
                    context.AddError(n_QueryOptions, "option to possible with expression result.");

                this.ResponseTypeName = _responseMsgName(SqlType);
            }
        }
        public      override    void                                Emit(LTTSQL.Core.EmitWriter emitWriter)
        {
            foreach(var node in Children) {
                if (node is Core.Token token) {
                    if (node.isWhitespaceOrComment || token.ID == LTTSQL.Core.TokenID.Semicolon)
                        node.Emit(emitWriter);
                }
                else
                if (node == n_Expression) {
                    int indent = emitWriter.Linepos;

                    emitWriter.WriteText("BEGIN");
                    emitWriter.WriteNewLine(indent + 4, "SELECT [responsemsg]=", SqlStatic.QuoteString(ResponseTypeName));
                    emitWriter.WriteNewLine(indent + 4);

                    if (node is LTTSQL.Node.IExprResponseNode exprResponseNode) {
                        exprResponseNode.EmitResponseNode(emitWriter, null);

                        if (n_QueryOptions != null) {
                            emitWriter.WriteNewLine(indent + 4);
                            n_QueryOptions.Emit(emitWriter);
                        }
                        emitWriter.WriteText(";");
                    }
                    else {
                        emitWriter.WriteText("SELECT ");
                        node.Emit(emitWriter);
                        emitWriter.WriteText(" FOR XML RAW('value'),ELEMENTS,TYPE;");
                    }

                    emitWriter.WriteNewLine(indent + 4, "RETURN;");
                    emitWriter.WriteNewLine(indent    , "END");
                }
                else
                if (node != n_QueryOptions)
                    node.Emit(emitWriter);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public      static      void                                WriteResponseXml(XmlWriter xmlWriter, LTTSQL.DataModel.ISqlType sqlType)
        {
            if (sqlType is LTTSQL.DataModel.SqlTypeResponseNode reponseNodeType) {
                switch(reponseNodeType.NodeType) {
                case LTTSQL.DataModel.ResponseNodeType.Object:
                    xmlWriter.WriteAttributeString("type", "object");
                    _writeResponseObject(xmlWriter, reponseNodeType);
                    break;
                case LTTSQL.DataModel.ResponseNodeType.ObjectMandatory:
                    xmlWriter.WriteAttributeString("type", "object:mandatory");
                    _writeResponseObject(xmlWriter, reponseNodeType);
                    break;
                case LTTSQL.DataModel.ResponseNodeType.ArrayValue: {
                        var arrayItemType = reponseNodeType.Columns[0].SqlType;

                        if (arrayItemType is DataModel.SqlTypeResponseNode responseNode) {
                            xmlWriter.WriteAttributeString("type", "array:object");
                            _writeResponseObject(xmlWriter, responseNode);
                        }
                        else
                            xmlWriter.WriteAttributeString("type", "array:" + SqlTypeToString(arrayItemType));
                    }
                    break;
                case LTTSQL.DataModel.ResponseNodeType.ArrayObject:
                    xmlWriter.WriteAttributeString("type", "array:object");
                    _writeResponseObject(xmlWriter, reponseNodeType);
                    break;
                }
            }
            else
                xmlWriter.WriteAttributeString("type",  SqlTypeToString(sqlType));;
        }
        public      static      string                              SqlTypeToString(DataModel.ISqlType sqlType)
        {
            if (sqlType == null)
                return "";

            if ((sqlType.TypeFlags & LTTSQL.DataModel.SqlTypeFlags.SimpleType) != 0)
                return sqlType.NativeType.NativeTypeString;

            if (sqlType is LTTSQL.DataModel.SqlTypeVoid)
                return "void";

            if (sqlType is LTTSQL.DataModel.EntityTypeExternal entityExternal)
                return "clr(" + entityExternal.Assembly.EntityName.Name + ":" + entityExternal.ClassName + ")";

            throw new NotSupportedException("Unsupported sqltype " + sqlType.GetType().Name + ".");
        }
        public      static      void                                _writeResponseObject(XmlWriter xmlWriter,  LTTSQL.DataModel.SqlTypeResponseNode reponseNodeType)
        {
            foreach(var field in reponseNodeType.Columns) {
                xmlWriter.WriteStartElement("field");
                    xmlWriter.WriteAttributeString("name", field.Name);

                    WriteResponseXml(xmlWriter, field.SqlType);
                xmlWriter.WriteEndElement();
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private     static      string                              _responseMsgName(DataModel.ISqlType returns)
        {
            using (var buffer = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(buffer, new XmlWriterSettings()
                                                                {
                                                                    CloseOutput = false,
                                                                    Indent      = false
                                                                }))
                {
                    xmlWriter.WriteStartElement("response");
                        WriteResponseXml(xmlWriter, returns);
                    xmlWriter.WriteEndElement();
                }

                using (var sha1 = new  System.Security.Cryptography.SHA1Managed())
                {
                    return Convert.ToBase64String(sha1.ComputeHash(buffer.GetBuffer(), 0, (int)buffer.Length));
                }
            }
        }
    }
}
