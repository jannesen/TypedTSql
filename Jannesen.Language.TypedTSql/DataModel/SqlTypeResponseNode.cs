using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public enum ResponseNodeType
    {
        Object          = 1,
        ObjectMandatory,
        ArrayValue,
        ArrayObject
    }

    public class SqlTypeResponseNode: SqlType
    {
        public      override    SqlTypeFlags            TypeFlags           { get { return SqlTypeFlags.ReponseNode;   } }

        public                  ResponseNodeType        NodeType            { get { return _nodeType; } }
        public      override    IColumnList             Columns             { get { return _columns; } }

        private                 ResponseNodeType        _nodeType;
        private                 IColumnList             _columns;

        public                                          SqlTypeResponseNode(ResponseNodeType nodeType, IColumnList columns)
        {
            _nodeType = nodeType;
            _columns  = columns;
        }

        public      override    string                  ToString()
        {
            return "response-node";
        }
    }
}
