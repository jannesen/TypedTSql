using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Core
{
    public abstract class EmitWriter
    {
        private     static      char[]                  nibbleToHex = new char[] { '0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' };

        public                  EmitContext             EmitContext     { get; private set; }
        public                  EmitOptions             EmitOptions     { get { return EmitContext.EmitOptions; } }
        public      virtual     int                     Linepos         { get { return 0; } }

        public                                          EmitWriter(EmitContext emitContext)
        {
            EmitContext = emitContext;
        }

        public                  void                    WriteNode(IAstNode node)
        {
            node.Emit(this);
        }
        public      abstract    void                    WriteText(string text);
        public                  void                    WriteText(params string[] text)
        {
            foreach(var t in text) {
                if (t != null)
                    WriteText(t);
            }
        }
        public      abstract    void                    WriteSpace(int length);
        public      virtual     void                    WriteText(string text, Library.FilePosition beginning, Library.FilePosition ending)
        {
            WriteText(text);
        }
        public      virtual     void                    WriteToken(Token token)
        {
            WriteText(token.Text, token.Beginning, token.Ending);
        }
        public                  void                    WriteValue(object value)
        {
            if (value == null) {
                WriteText("NULL");
                return;
            }

            if (value is string) {
                WriteText(Library.SqlStatic.QuoteString((string)value));
                return;
            }

            if (value is int) {
                WriteText(((int)value).ToString());
                return;
            }

            if (value is decimal) {
                WriteText(((decimal)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                return;
            }

            if (value is double) {
                WriteText(((int)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                return;
            }

            if (value is byte[] v) {
                StringBuilder   str = new StringBuilder(4 + v.Length*2);

                str.Append("0x");

                for (int i=0 ; i < v.Length ; ++i) {
                    byte b = v[i];
                    str.Append(nibbleToHex[(b >> 4) & 0xF]);
                    str.Append(nibbleToHex[(b     ) & 0xF]);
                }

                WriteText(str.ToString());
                return;
            }

            throw new InvalidOperationException("Con't know to to emit " + value.GetType().FullName + ".");
        }
        public      virtual     void                    WriteNewLine(int indent)
        {
            WriteText("\r\n");
        }
        public                  void                    WriteNewLine(int indent, string text)
        {
            WriteNewLine(indent);
            WriteText(text);
        }
        public                  void                    WriteNewLine(int indent, params string[] text)
        {
            WriteNewLine(indent);
            foreach(var t in text) {
                if (t != null)
                    WriteText(t);
            }
        }
    }

    public class EmitWriterSourceMap: EmitWriter
    {
        public      override    int                     Linepos         { get { return _emitpos.Linepos;    } }
        public                  Library.SourceMap       SourceMap
        {
            get {
                return _sourceMap;
            }
        }

        public                  EmitContext             _emitContext;
        private                 StringBuilder           _stringBuilder;
        private                 Library.FilePosition    _emitpos;
        private                 Library.SourceMap       _sourceMap;
        private                 bool                    _skipblank;
        private                 bool                    _blankline;

        public                                          EmitWriterSourceMap(EmitContext emitContext, string filename, int lineno): base(emitContext)
        {
            _emitContext   = emitContext;
            _stringBuilder = new StringBuilder(0x4000);
            _emitpos.Lineno  = 1;
            _emitpos.Linepos = 1;
            _emitpos.Filepos = 0;
            _skipblank = true;
            _blankline = true;
            _sourceMap = new Library.SourceMap(filename, lineno);
        }

        public      override    void                    WriteText(string text)
        {
            _stringBuilder.Append(text);

            _emitpos.Filepos += text.Length;

            for (int i = 0 ; i < text.Length ; ++i) {
                char c = text[i];

                if (c == '\n') {
                    ++_emitpos.Lineno;
                    _emitpos.Linepos = 1;
                    _blankline = true;
                }
                else {
                    if (_blankline && !(c == '\t' || c == ' '))
                        _blankline = false;

                    ++_emitpos.Linepos;
                }
            }
            _skipblank = false;
        }
        public      override    void                    WriteSpace(int length)
        {
            if (length > 0) {
                _stringBuilder.Append(' ', length);
                _emitpos.Linepos += length;
            }
        }
        public      override    void                    WriteText(string text, Library.FilePosition beginning, Library.FilePosition ending)
        {
            if (_skipblank) {
                int     p = 0;

                for (int i = 0; i < text.Length ; ++i) {
                    char c = text[i];

                    if (c == '\n') {
                        ++beginning.Lineno;
                        beginning.Linepos = 1;
                        p = i + 1;
                    }
                    else
                    if (!(c == ' ' || c == '\t' || c == '\r'))
                        break;
                }

                if (p>0) {
                    if (p >= text.Length)
                        return;

                    beginning.Filepos += p;
                    text = text.Substring(p);
                }

                _skipblank = false;
            }

            Library.FilePosition        emitBeginning = _emitpos;
            WriteText(text);
            _sourceMap.AddRemapEntry(beginning, ending, emitBeginning, _emitpos);
        }
        public      override    void                    WriteNewLine(int indent)
        {
            if (!_blankline) {
                _stringBuilder.Append("\r\n");
                _emitpos.Filepos += 2;
                ++_emitpos.Lineno;
                _emitpos.Linepos = 1;
            }

            _blankline = false;

            while (_emitpos.Linepos < indent) {
                _stringBuilder.Append(' ');
                ++_emitpos.Filepos;
                ++_emitpos.Linepos;
            }

            _skipblank = false;
        }

        public                  string                  GetSql()
        {
            return _stringBuilder.ToString();
        }
    }

    public class EmitWriterString: EmitWriter
    {
        public                  string                  String
        {
            get {
                return _stringBuilder.ToString();
            }
        }

        private                 StringBuilder           _stringBuilder;

        public                                          EmitWriterString(EmitContext emitContext): base(emitContext)
        {
            _stringBuilder = new StringBuilder(0x4000);
        }

        public      override    void                    WriteText(string text)
        {
            _stringBuilder.Append(text);
        }
        public      override    void                    WriteSpace(int length)
        {
            if (length > 0) {
                _stringBuilder.Append(' ', length);
            }
        }
    }

    public class EmitWriterTrim: EmitWriter
    {
        public      override    int                     Linepos
        {
            get {
                if (_full)
                    return 1;

                if (_prevToken != null) {
                    _emitWriter.WriteToken(_prevToken);
                    _prevToken = null;
                }

                return _emitWriter.Linepos;
            }
        }

        private                 EmitWriter              _emitWriter;
        private                 bool                    _full;
        private                 Token                   _prevToken;
        private                 bool                    _init;
        private                 bool                    _prevWhiteSpace;

        public                                          EmitWriterTrim(EmitWriter emitWriter, bool full): base(emitWriter.EmitContext)
        {
            _emitWriter = emitWriter;
            _full       = full;
            _init       = true;
        }

        public      override    void                    WriteText(string text)
        {
            if (_full)
                text.Replace("\r", "").Replace('\n', ' ').Replace("  ", " ");

            _emitWriter.WriteText(text);
            _prevToken      = null;
            _prevWhiteSpace = false;
            _init           = false;
        }
        public      override    void                    WriteSpace(int length)
        {
            if (length > 0) {
                if (!_full)
                    _emitWriter.WriteSpace(length);

                _prevToken      = null;
                _prevWhiteSpace = _full;
                _init           = false;
            }
        }
        public      override    void                    WriteNewLine(int indent)
        {
            if (!_full)
                _emitWriter.WriteNewLine(indent);

            _prevToken      = null;
            _prevWhiteSpace = _full;
            _init           = false;
        }
        public      override    void                    WriteText(string text, Library.FilePosition beginning, Library.FilePosition ending)
        {
            _emitWriter.WriteText(text, beginning, ending);
            _prevToken      = null;
            _prevWhiteSpace = false;
            _init           = false;
        }
        public      override    void                    WriteToken(Token token)
        {
            if (_full) {
                if (!token.isWhitespaceOrComment) {
                    if (_prevWhiteSpace && _prevToken != null &&
                        (token.ID      < TokenID._operators || token.ID      >= TokenID._beginkeywords) &&
                        (_prevToken.ID < TokenID._operators || _prevToken.ID >= TokenID._beginkeywords))
                        WriteText(" ");

                    _emitWriter.WriteToken(token);
                    _prevToken = token;
                    _prevWhiteSpace = false;
                }
                else
                    _prevWhiteSpace = true;
            }
            else {
                if (!_init) {
                    if (_prevToken != null) {
                        _emitWriter.WriteToken(_prevToken);
                        _prevToken = null;
                    }
                }

                if (!token.isWhitespaceOrComment) {
                    _emitWriter.WriteToken(token);
                    _init = false;
                }
                else {
                    if (!_init) {
                        _prevToken = token;
                    }
                }
            }
        }
    }

    public class EmitWriterArray: EmitWriter
    {
        public                  List<IAstNode>          Nodes           { get ; private set; }

        public                                          EmitWriterArray(EmitContext emitContext): base(emitContext)
        {
            Nodes = new List<IAstNode>();
        }

        public      override    void                    WriteText(string text)
        {
            Nodes.Add(new Node.Node_CustomNode(text));
        }
        public      override    void                    WriteSpace(int length)
        {
            if (length > 0) {
                WriteText(new string(' ', length));
            }
        }
        public      override    void                    WriteToken(Token token)
        {
            Nodes.Add(token);
        }
    }
}
