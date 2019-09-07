using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.Library
{
    public class EmitContext
    {
        private struct ServiceEntity
        {
            public      Node.DeclarationService             Service;
            public      Node.DeclarationServiceMethod[]     Methods;
            public      string[]                            SourceFiles;
        }

        public  readonly            Transpiler                          Transpiler;
        public  readonly            EmitOptions                         EmitOptions;
        public  readonly            SqlDatabase                         Database;
        public                      List<EmitError>                     EmitErrors;

        private                     bool                                _rebuild;
        private                     IReadOnlyList<EntityDeclaration>    _entities;
        private                     List<ServiceEntity>                 _services;

        public                                                          EmitContext(Transpiler transpiler, EmitOptions emitOptions, SqlDatabase database)
        {
            this.Transpiler  = transpiler;
            this.EmitOptions = emitOptions;
            this.Database    = database;
            this.EmitErrors  = new List<EmitError>();
        }

        public                      void                                Emit(HashSet<string> changedSourceFiles)
        {
            _rebuild  = changedSourceFiles == null;
            _init(changedSourceFiles);

            if (_entities.Count > 0) {
                if (_emitDropCode()) {
                    if (_emitInstallInto()) {
                        if (_emitEntityDeclaration()) {
                            _emitEntityGrant();
                            _emitServiceFiles();
                        }
                    }
                }
            }
        }
        public                      void                                AddEmitError(EmitError emitError)
        {
            EmitErrors.Add(emitError);
            EmitOptions?.OnEmitError(emitError);
        }
        public                      void                                AddEmitMessage(string message)
        {
            EmitOptions?.OnEmitMessage(message);
        }

        private                     void                                _init(HashSet<string> changedSourceFiles)
        {
            _services = new List<ServiceEntity>();

            foreach(var service in Transpiler.ServiceDeclarations) {
                bool    changed = _rebuild;

                List<Node.DeclarationServiceMethod>     methods = new List<Node.DeclarationServiceMethod>(16);
                HashSet<string>                         sourceFiles = new HashSet<string>();

                foreach(var file in Transpiler.Files) {
                    bool    include = false;
                    foreach(var declaration in file.Declarations) {
                        if (declaration is Node.DeclarationServiceMethod method && method.DeclarationService == service) {
                            methods.Add(method);
                            include = true;
                        }
                    }

                    if (include) {
                        sourceFiles.Add(file.Filename);

                        if (!changed && changedSourceFiles.Contains(file.Filename))
                            changed = true;
                    }
                }

                if (changed && methods.Count > 0)
                    _services.Add(new ServiceEntity() { Service=service, Methods=methods.ToArray(), SourceFiles=sourceFiles.ToArray() });
            }

            if (changedSourceFiles != null) {
                var entities = new List<EntityDeclaration>();

                foreach (var e in Transpiler.EntityDeclarations) {
                    if (changedSourceFiles.Contains(e.SourceFile.Filename) ||
                        (e.EntityType == DataModel.SymbolType.Service && _serviceNeedsEmit(e.Declaration, changedSourceFiles)))
                    {
                        if (e.EntityType == DataModel.SymbolType.Assembly     ||
                            e.EntityType == DataModel.SymbolType.TypeUser     ||
                            e.EntityType == DataModel.SymbolType.TypeExternal ||
                            e.EntityType == DataModel.SymbolType.TypeTable)
                        {   // Type changed => emit every this.
                            _entities = Transpiler.EntityDeclarations;
                            return;
                        }

                        entities.Add(e);
                    }
                }

                _entities = entities;
            }
            else
                _entities = Transpiler.EntityDeclarations;
        }
        private                     bool                                _emitDropCode()
        {
            bool    rtn = true;

            Database.ResetSettings();

            if (!Database.AllCodeDropped) {
                using (StringWriter stringWriter = new StringWriter()) {
                    foreach (var entity in _entities)
                        entity.EmitDrop(stringWriter);

                    string sqlcmd = stringWriter.ToString();
                    if (sqlcmd.Length > 0) {
                        try {
                            Database.ExecuteStatement(sqlcmd);
                        }
                        catch(Exception err) {
                            AddEmitError(new EmitError("Drop statements failed: " + err.Message));
                            rtn = false;
                        }
                    }
                }
            }

            return rtn;
        }
        private                     bool                                _emitEntityDeclaration()
        {
            bool    rtn = true;

            foreach (var entityDeclaration in _entities) {
                if (!entityDeclaration.EmitCode(this))
                    rtn = false;
            }

            return rtn;
        }
        private                     bool                                _emitInstallInto()
        {
            bool    rtn = true;

            foreach (var entityDeclaration in _entities) {
                if (!entityDeclaration.EmitInstallInto(this, 1))
                    rtn = false;
            }

            foreach (var entityDeclaration in _entities) {
                if (!entityDeclaration.EmitInstallInto(this, 2))
                    rtn = false;
            }

            return rtn;
        }
        private                     void                                _emitEntityGrant()
        {
            Database.Print("# set permissions");

            foreach (var entityDeclaration in _entities)
                entityDeclaration.EmitGrant(this);
        }
        private                     void                                _emitServiceFiles()
        {
            foreach(var service in _services) {
                service.Service.EmitServiceFiles(this, service.Methods, _rebuild);
            }
        }

        private                     bool                                _serviceNeedsEmit(Node.DeclarationEntity declaration, HashSet<string> changedSourceFiles)
        {
            if (changedSourceFiles == null)
                return true;

            foreach(var service in _services) {
                if (service.Service == declaration) {
                    foreach(var s in service.SourceFiles) {
                        if (changedSourceFiles.Contains(s))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
