using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text;
using Jannesen.Language.TypedTSql.DataModel;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql
{
    public partial  class Transpiler
    {
        public              SourceFileList                          Files                   { get ; private set; }
        public              int                                     ErrorCount
        {
            get {
                int n = 0;

                foreach(var file in Files)
                    n += file.ParseMessages.Count + file.TranspileMessages.Count;

                return n;
            }
        }
        public              IReadOnlyList<TypedTSqlMessage>         Errors
        {
            get {
                var errorList = new List<TypedTSqlMessage>();

                foreach(var sourceFile in Files) {
                    errorList.AddRange(sourceFile.ParseMessages);
                    errorList.AddRange(sourceFile.TranspileMessages);

                    if (errorList.Count > 256)
                        break;
                }

                return errorList;
            }
        }
        public              IReadOnlyList<EmitError>                EmitErrors
        {
            get {
                return _emitErrors;
            }
        }

        public              NodeParser<Node.Declaration>            DeclarationParsers      { get; private set; }
        public              NodeParser<Node.Statement>              StatementParsers        { get; private set; }
        public              Node.DeclarationServiceList             ServiceDeclarations     { get; private set; }
        public              IReadOnlyList<EntityDeclaration>        EntityDeclarations      { get { return _entityDeclarations; } }

        private             HashSet<Assembly>                       _extensions;
        private             List<EntityDeclaration>                 _entityDeclarations;
        private             int                                     _transpileCount;
        private             List<EmitError>                         _emitErrors;

        public                                                      Transpiler()
        {
            Files              = new SourceFileList();
            DeclarationParsers = new NodeParser<Node.Declaration>();
            StatementParsers   = new NodeParser<Node.Statement>();
            _extensions        = new HashSet<Assembly>();

            BuildIn.Catalog.Load();
            LoadExtension(this.GetType().Assembly);
        }

        public              void                                    LoadExtension(Assembly assembly)
        {
            if (_extensions.Contains(assembly))
                return;

            _extensions.Add(assembly);

            foreach(var type in assembly.GetTypes()) {
                if (type.IsClass && type.IsPublic) {
                    foreach(AttrNodeParser attr in type.GetCustomAttributes(typeof(AttrNodeParser), false)) {
                        if (attr is DeclarationParser)     DeclarationParsers.AddParser(attr, type);
                        if (attr is StatementParser)       StatementParsers.AddParser(attr, type);
                    }
                }
            }
        }
        public              void                                    LoadExtensions(string names)
        {
            if (names != null) { 
                foreach(var name in names.Split(';'))
                    LoadExtension(Assembly.Load(name.Trim()));
            }
        }
        public              SourceFile                              AddFile(string filename)
        {
            var sourceFile = new SourceFile(this, filename);

            Files.Add(sourceFile);

            return sourceFile;
        }
        public              void                                    RemoveFile(string filename)
        {
            Files.Remove(filename);
        }
        public              SourceFile[]                            Parse(string[] filenames)
        {
            int next        = 0;
            var sourceFiles = new SourceFile[filenames.Length];
#if DEBUG
            var workers     = new Thread[1];
#else
            var workers     = new Thread[4];
#endif
            for (int i = 0 ; i < workers.Length ; ++i) {
                workers[i] = new Thread(() => {
                                                for (;;) {
                                                    int n;
                                                    lock(sourceFiles) {
                                                        if (next >= filenames.Length)
                                                            return;
                                                        n = next++;
                                                    }
                                                    var dt = DateTime.UtcNow;
                                                    var sourceFile = new SourceFile(this, filenames[n]);
                                                    try {
                                                        sourceFile.ParseFile();
                                                    }
                                                    catch(Exception err) {
                                                        string  msg =  "Parse failed:";
                                                        for ( ; err != null ; err = err.InnerException)
                                                            msg += " " + err.Message;
                                                        sourceFile.AddTranspileMessage(new TypedTSqlParseError(sourceFile, msg));
                                                    }
                                                    sourceFiles[n] = sourceFile;
                                                }
                                        });
                workers[i].Start();
            }

            for (int i = 0 ; i < workers.Length ; ++i)
                workers[i].Join();

            for(int i = 0 ; i < sourceFiles.Length ; ++i)
                Files.Add(sourceFiles[i] = sourceFiles[i]);

            return sourceFiles;
        }
        public              int                                     Transpile(GlobalCatalog globalCatalog)
        {
            int     passCount = 0;

            globalCatalog.BeforeTranspile();

            var transpileContext = new Transpile.TranspileContext(this, globalCatalog);

            foreach(var f in Files)
                f.TranspileInit(transpileContext, _transpileCount > 0);

            if (ErrorCount == 0) {
                _transpileServiceDeclarations(transpileContext);
                _transpileInit(transpileContext);
                globalCatalog.CleanupTranspile();

                if (ErrorCount == 0)
                    passCount = _transpileEntity(transpileContext);
            }

#if DEBUG
            if (ErrorCount == 0)
                _checkTranspile();
#endif
            _transpileCount++;

            return passCount;
        }
        public              void                                    Analyze(GlobalCatalog globalCatalog)
        {
            foreach (var entityDeclaration in EntityDeclarations) {
                entityDeclaration.CodeAnalyze(this, globalCatalog);
            }
        }
        public              void                                    Emit(EmitOptions emitOptions, SqlDatabase database, HashSet<string> changedSourceFiles = null)
        {
            var emitContext = new EmitContext(this, emitOptions, database);
            _emitErrors = emitContext.EmitErrors;
            emitContext.Emit(changedSourceFiles);
        }

        public              Core.Token                              GetTokenAt(string filename, int position)
        {
            return GetTokenAt(filename, position, position);
        }
        public              Core.Token                              GetTokenAt(string filename, int startposition, int endposition)
        {
            return Files[filename].GetTokenAt(startposition, endposition);
        }
        public              DataModel.DocumentSpan                  GetDocumentSpan(object declaration)
        {
            if (declaration == null)
                return null;

            if (declaration is DataModel.DocumentSpan documentSpan)
                return documentSpan;

            if (declaration is Core.Token token)
                return new DataModel.DocumentSpan(GetSourceFile(token).Filename, token);

            throw new ArgumentException(declaration.GetType().FullName + " is not a valid declaration");
        }
        public              List<SymbolReferenceList>               GetReferences(DataModel.SymbolData symbolData)
        {
            var rtn = new List<SymbolReferenceList>();

            if (symbolData is DataModel.SymbolUsage symbolUsage) {
                rtn.Add(GetReferences(symbolUsage.Symbol));
            }

            if (symbolData is DataModel.SymbolSourceTarget symbolSourceTarget) {
                rtn.Add(GetReferences(symbolSourceTarget.Source.Symbol));
                rtn.Add(GetReferences(symbolSourceTarget.Target.Symbol));
            }

            return rtn;
        }
        public              SymbolReferenceList                     GetReferences(DataModel.ISymbol symbol)
        {
            var symbolReferenceList = new SymbolReferenceList(symbol);

            foreach (var f in Files)
                f.FindSymbols(symbolReferenceList, symbol);

            return symbolReferenceList;
        }
        public              SourceFile                              GetSourceFile(Core.Token token)
        {
            foreach (var sourceFile in Files) {
                foreach (var t in sourceFile.Tokens) {
                    if (object.ReferenceEquals(t, token))
                        return sourceFile;
                }
            }

            throw new KeyNotFoundException("Can't find token in source files.");
        }

        private             void                                    _transpileServiceDeclarations(Transpile.TranspileContext transpileContext)
        {
            var serviceDeclarations   = new Node.DeclarationServiceList(16);

            foreach(var sourceFile in Files) {
                if (sourceFile.Declarations != null) {
                    foreach (var declaration in sourceFile.Declarations) {
                        try {
                            if (declaration is Node.DeclarationService declarationService) {
                                declarationService.TranspileInit(transpileContext, sourceFile);
                                if (!serviceDeclarations.TryAdd(declarationService))
                                    sourceFile.AddTranspileMessage(new TypedTSqlTranspileError(sourceFile,  declaration, "Service already declared."));
                            }
                        }
                        catch(Exception err) {
                            sourceFile.AddTranspileMessage(new TypedTSqlTranspileError(sourceFile, declaration, err));
                        }
                    }
                }
            }

            ServiceDeclarations = serviceDeclarations;
        }
        private             void                                    _transpileInit(Transpile.TranspileContext transpileContext)
        {
            var entityDeclarationSort = new Internal.EntityDeclarationSort();

            foreach(var sourceFile in Files) {
                if (sourceFile.Declarations != null) {
                    foreach (var declaration in sourceFile.Declarations) {
                        try {
                            declaration.TranspileInit(transpileContext, sourceFile);
                            if (declaration is Node.DeclarationEntity declarationEntity) {
                                entityDeclarationSort.AddEntityDeclaration(new EntityDeclaration(sourceFile, sourceFile.Options, declarationEntity));
                            }
                        }
                        catch(Exception err) {
                            sourceFile.AddTranspileMessage(new TypedTSqlTranspileError(sourceFile, declaration, err));
                        }
                    }
                }
            }

            _entityDeclarations = entityDeclarationSort.Process();
        }
        private             int                                     _transpileEntity(Transpile.TranspileContext transpileContext)
        {
            int         passCount = 0;
            bool        transpiled;
            bool        needsTranspiled;

            do {
                transpiled      = false;
                needsTranspiled = false;

                if (ErrorCount != 0)
                    return passCount;

                foreach (var entityDeclaration in EntityDeclarations)
                    entityDeclaration.Transpile(transpileContext, false, ref transpiled, ref needsTranspiled);

                ++passCount;
            }
            while(needsTranspiled && transpiled && passCount < 6);

            if (needsTranspiled) {
                foreach (var entityDeclaration in EntityDeclarations)
                    entityDeclaration.Transpile(transpileContext, true, ref transpiled, ref needsTranspiled);
            }

            return passCount;
        }

#if DEBUG
        private             void                                    _checkTranspile()
        {
            foreach(var sourcefile in Files) {
                foreach(var token in sourcefile.Tokens) {
                    if (token is Core.TokenWithSymbol tokenWithSymbol) {
                        if (tokenWithSymbol.SymbolData == null) {
                            if (token is Token.TokenLocalName ||
//                                token is Token.Name           ||
                                token is Token.QuotedName)
                            {
                                sourcefile.AddTranspileMessage(new TypedTSqlTranspileError(sourcefile, token, "Token not transpiled."));
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}
