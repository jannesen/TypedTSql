using System;
using System.Collections;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql
{
    public class SourceFile
    {
        public                  Transpiler                                      Transpiler          { get; private set; }
        public                  string                                          Filename            { get; private set; }
        public                  IReadOnlyList<Core.Token>                       Tokens              { get { return _tokens;          } }
        public                  Node.Node_ParseOptions                          Options             { get { return _options;         } }
        public                  IReadOnlyList<Node.Declaration>                 Declarations        { get { return _declarations;    } }
        public                  IReadOnlyList<TypedTSqlMessage>                 ParseMessages       { get { return _parseMessages;   } }
        public                  IReadOnlyList<TypedTSqlMessage>                 TranspileMessages   { get { return _transpileMessages; } }

        private                 Core.Token[]                                    _tokens;
        private                 Node.Node_ParseOptions                          _options;
        private                 Node.Declaration[]                              _declarations;
        private                 List<TypedTSqlMessage>                          _parseMessages;
        private                 List<TypedTSqlMessage>                          _transpileMessages;

        internal                                                                SourceFile(Transpiler transpiler, string filename)
        {
            this.Transpiler    = transpiler;
            this.Filename      = filename;
            _parseMessages     = new List<TypedTSqlMessage>();
            _transpileMessages = new List<TypedTSqlMessage>();
        }

        public                  void                                            ParseFile()
        {
            using (var inputStream = new System.IO.StreamReader(Filename))
            {
                ParseContent(inputStream.ReadToEnd());
            }
        }
        public                  void                                            ParseContent(string content)
        {
            try {
                _tokens       = null;
                _options      = null;
                _declarations = null;
                _parseMessages.Clear();
                _transpileMessages.Clear();

                _tokenizing(content);
                content = null;
                _parse();
            }
            catch(Exception err) {
                throw new ParseFileException("Parse file '" + Filename + "' failed.", err);
            }
        }

        public                  Core.Token                                      GetTokenAt(int position)
        {
            int     b = 0;
            int     e = _tokens.Length - 1;

            while (e >= 0 && b < _tokens.Length) {
                int i = b + (e-b) / 2;

                if (position < _tokens[i].Beginning.Filepos) {
                    e = i - 1;
                }
                else
                if (position >= _tokens[i].Ending.Filepos) {
                    b = i + 1;
                }
                else
                    return _tokens[i];
            }

            throw new KeyNotFoundException("Can't locate token at position.");
        }

        public      static      bool                                            isTypedTSqlFile(string filename)
        {
            return filename.EndsWith(".ttsql", StringComparison.InvariantCultureIgnoreCase);
        }

        internal                void                                            TranspileInit(GlobalCatalog catalog, bool resetSymbols)
        {
            _transpileMessages.Clear();

            if (resetSymbols) {
                foreach(var token in _tokens) {
                    if (token is Core.TokenWithSymbol)
                        ((Core.TokenWithSymbol)token).ClearSymbol();
                }
            }

            _options.TranspileInit(this, catalog);
        }
        internal                void                                            FindSymbols(SymbolReferenceList symbolReferenceList, DataModel.ISymbol symbol)
        {
            foreach(var token in _tokens) {
                if (token is Core.TokenWithSymbol) {
                    if (((Core.TokenWithSymbol)token).Symbol == symbol)
                        symbolReferenceList.Add(new SymbolReference(this, (Core.TokenWithSymbol)token));
                }
            }
        }
        internal                void                                            AddParseMessage(TypedTSqlMessage error)
        {
            _parseMessages.Add(error);
        }
        internal                void                                            AddTranspileMessage(TypedTSqlMessage error)
        {
            _transpileMessages.Add(error);
        }

        private                 void                                            _tokenizing(string content)
        {
            var         tokens = new List<Core.Token>(0x1000);
            var         reader = new Core.LexerReader(this, content, new Library.FilePosition(0, 1, 1));
            Core.Token  token;

            while ((token = reader.ReadToken()) != null)
                tokens.Add(token);

            _tokens = tokens.ToArray();
        }
        private                 void                                            _parse()
        {
            var parserReader  = new Core.ParserReader(this.Transpiler, this, _tokens);

            try {
                _options      = parserReader.ParseOptions();
                _declarations = (new Node.Declarations(parserReader)).n_Declarations;
            }
            catch(Exception err) {
                parserReader.AddError(err);
            }
        }
    }

    public class SourceFileList: IReadOnlyList<SourceFile>
    {
        private                 SortedList<string, SourceFile>              _sortedlist;

        public                  SourceFile                                  this[int index]
        {
            get {
                return _sortedlist.Values[index];
            }
        }
        public                  SourceFile                                  this[string fullpath]
        {
            get {
                if (!_sortedlist.TryGetValue(fullpath.ToUpperInvariant(), out SourceFile rtn))
                    throw new KeyNotFoundException("File '" + fullpath + "' does not exists in source file list.");

                return rtn;
            }
        }
        public                  int                                         Count
        {
            get {
                return _sortedlist.Count;
            }
        }

        internal                                                            SourceFileList()
        {
            _sortedlist = new SortedList<string, SourceFile>();
        }

        public                  IEnumerator<SourceFile>                     GetEnumerator()
        {
            return _sortedlist.Values.GetEnumerator();
        }
                                IEnumerator                                 IEnumerable.GetEnumerator()
        {
            return _sortedlist.Values.GetEnumerator();
        }

        internal                void                                        Add(SourceFile sourceFile)
        {
            _sortedlist.Add(sourceFile.Filename.ToUpperInvariant(), sourceFile);
        }
        internal                void                                        Remove(string filename)
        {
            _sortedlist.Remove(filename.ToUpperInvariant());
        }
    }
}
