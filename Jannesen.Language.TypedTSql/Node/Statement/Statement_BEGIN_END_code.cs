using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Statement_BEGIN_END_code: Statement_BEGIN_END
    {
        public      readonly    bool                                StandardSettings;

        public                                                      Statement_BEGIN_END_code(Core.ParserReader reader, IParseContext parseContext, bool standardSettings): base(reader, parseContext)
        {
            this.StandardSettings = standardSettings;
        }

        public      override    void                                Emit(Core.EmitWriter emitWriter)
        {
            foreach (var c in Children) {
                if (c is Node.StatementBlock sb) {
                    if (StandardSettings) {
                        _insertStandardSettings(emitWriter);
                    }

                    sb.Emit(emitWriter, 5);
                }
                else {
                    c.Emit(emitWriter);
                }
            }
        }

        private                 void                                _insertStandardSettings(Core.EmitWriter emitWriter)
        {
            bool        addLine                        = true;
            bool        setNOCOUNT                     = true;
            bool        setCONCAT_NULL_YIELDS_NULL     = true;
            bool        setXACT_ABORT                  = true;
            bool        setANSI_NULLS                  = true;
            bool        setANSI_PADDING                = true;
            bool        setANSI_WARNINGS               = true;
            bool        setARITHABORT                  = true;
            bool        setNUMERIC_ROUNDABORT          = true;
            bool        setTRANSACTION_ISOLATION_LEVEL = true;

            if (n_Statements.Children != null) {
                foreach(var child in n_Statements.Children) {
                    if (child is Core.AstParseNode) {
                        if (child is Statement_SET_option) {
                            addLine = false;

                            foreach(string option in ((Statement_SET_option)child).n_Options) {
                                switch(option) {
                                case "ANSI_NULLS":                  setANSI_NULLS                  = false; break;
                                case "ANSI_PADDING":                setANSI_PADDING                = false; break;
                                case "ANSI_WARNINGS":               setANSI_WARNINGS               = false; break;
                                case "ARITHABORT":                  setARITHABORT                  = false; break;
                                case "CONCAT_NULL_YIELDS_NULL":     setCONCAT_NULL_YIELDS_NULL     = false; break;
                                case "NOCOUNT":                     setNOCOUNT                     = false; break;
                                case "NUMERIC_ROUNDABORT":          setNUMERIC_ROUNDABORT          = false; break;
                                case "TRANSACTION":                 setTRANSACTION_ISOLATION_LEVEL = false; break;
                                case "XACT_ABORT":                  setXACT_ABORT                  = false; break;

                                case "ANSI_NULL_DFLT_ON":
                                    if (string.Compare(((Core.Token)((Statement_SET_option)child).n_Value).Text, "ON", StringComparison.OrdinalIgnoreCase) == 0) {
                                        setARITHABORT    = false;
                                        setXACT_ABORT    = false;
                                        setANSI_NULLS    = false;
                                        setANSI_PADDING  = false;
                                        setANSI_WARNINGS = false;
                                    }
                                    break;
                                }
                            }
                        }
                        else
                            break;
                    }
                    else
                    if (child is Core.Token) {
                        if (!((Core.Token)child).isWhitespaceOrComment)
                            break;
                    }
                }
            }

            List<Node_CustomNode>   children = new List<Node_CustomNode>();

            string  setOn = "";
            if (setNOCOUNT)                     setOn += ",NOCOUNT";
            if (setANSI_NULLS)                  setOn += ",ANSI_NULLS";
            if (setANSI_PADDING)                setOn += ",ANSI_PADDING";
            if (setANSI_WARNINGS)               setOn += ",ANSI_WARNINGS";
            if (setARITHABORT)                  setOn += ",ARITHABORT";
            if (setCONCAT_NULL_YIELDS_NULL)     setOn += ",CONCAT_NULL_YIELDS_NULL";
            if (setXACT_ABORT)                  setOn += ",XACT_ABORT";

            if (setOn.Length > 0) {
                emitWriter.WriteText("    SET " + setOn.Substring(1) + " ON;\n");
            }

            if (setNUMERIC_ROUNDABORT)          emitWriter.WriteText("    SET NUMERIC_ROUNDABORT OFF;\n");
            if (setTRANSACTION_ISOLATION_LEVEL) emitWriter.WriteText("    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;\n");
            if (addLine)                        emitWriter.WriteText("\n");
        }
    }
}
