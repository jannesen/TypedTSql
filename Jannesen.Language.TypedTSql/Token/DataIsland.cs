using System;
using System.Xml;

namespace Jannesen.Language.TypedTSql.Token
{
    public class DataIsland: Core.Token
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.DataIsland;
            }
        }
        public      override        string                  ValueString
        {
            get {
                int off = Text.IndexOf('[') + 1;
                return Text.Substring(off, Text.Length - off*2);
            }
        }
        public      override        XmlElement              ValueXmlFragment
        {
            get {
                XmlDocument xmlDoc = new XmlDocument() { PreserveWhitespace = false };
                xmlDoc.LoadXml("<root>" + ValueString + "</root>");
                return xmlDoc.DocumentElement;
            }
        }

        internal                                            DataIsland(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }
    }
}
