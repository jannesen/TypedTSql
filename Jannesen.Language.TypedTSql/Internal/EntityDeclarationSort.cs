using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Internal
{
    internal class EntityDeclarationSort
    {
        //TODO: recursive functions.

        private class SortEntry
        {
            public      DataModel.EntityName    EntityName;
            public      EntityDeclaration       Declaration;
            public      List<SortEntry>         Dependancies;
            public      int                     RequiredCount;
            public      bool                    InResult;

            public      bool                    isEndPoint
            {
                get {
                    return RequiredCount == 0 && Declaration.Declaration.callableFromCode;
                }
            }

            public      bool                    CanStore()
            {
                if (InResult)
                    return false;

                foreach(SortEntry codeObject in Dependancies) {
                    if (!codeObject.InResult)
                        return false;
                }

                return true;
            }
            public      bool                    IsRecursive()
            {
                return _testRecursive(this);
            }

            public                              SortEntry(EntityDeclaration entryDeclaration)
            {
                EntityName   = entryDeclaration.EntityName;
                Declaration  = entryDeclaration;
                Dependancies = new List<SortEntry>();
            }

            private     bool                    _testRecursive(SortEntry root)
            {
                foreach(SortEntry co in Dependancies) {
                    if (co == root)
                        return true;

                    if (co._testRecursive(root))
                        return true;
                }

                return false;
            }
        }

        private class EntityDeclarationHashList: Library.ListHash<EntityDeclaration, DataModel.EntityName>
        {
            public                                              EntityDeclarationHashList(int capacity): base(capacity)
            {
            }

            protected   override    DataModel.EntityName        ItemKey(EntityDeclaration item)
            {
                return item.EntityName;
            }
        }

        private         EntityDeclarationHashList       _assemblies;
        private         EntityDeclarationHashList       _udts;
        private         EntityDeclarationHashList       _objects;
        private         EntityDeclarationHashList       _service;

        private         List<EntityDeclaration>         _typeAssembly;
        private         List<EntityDeclaration>         _typeUser;
        private         List<EntityDeclaration>         _typeTable;
        private         List<EntityDeclaration>         _other;
        private         List<SortEntry>                 _code_all;
        private         List<SortEntry>                 _code_internal;
        private         List<SortEntry>                 _code_endpoints;

        public                                          EntityDeclarationSort()
        {
            _assemblies = new EntityDeclarationHashList(64);
            _udts       = new EntityDeclarationHashList(1024);
            _objects    = new EntityDeclarationHashList(4096);
            _service    = new EntityDeclarationHashList(64);
        }
        public          void                            AddEntityDeclaration(EntityDeclaration entityDeclaration)
        {
            EntityDeclarationHashList   listhash;

            switch(entityDeclaration.EntityType) {
            case DataModel.SymbolType.Assembly:
                listhash = _assemblies;
                break;

            case DataModel.SymbolType.TypeUser:
            case DataModel.SymbolType.TypeExternal:
            case DataModel.SymbolType.TypeTable:
                listhash = _udts;
                break;

            case DataModel.SymbolType.Service:
                listhash = _service;
                break;

            default:
                listhash = _objects;
                break;
            }

            if (listhash.TryGetValue(entityDeclaration.EntityName, out EntityDeclaration firstDeclaration)) {
                firstDeclaration.SourceFile.AddTranspileMessage (new TypedTSqlTranspileError(firstDeclaration.SourceFile,  firstDeclaration.Declaration.GetNameToken(),  "Duplicate defination of entity."));
                return;
            }

            listhash.Add(entityDeclaration);
        }
        public          List<EntityDeclaration>         Process()
        {
            _fill();
            _getReferences();
            _fix_recurvice();
            _sort_endpoint();
            _sort_internal();
            _check();

            return _result();
        }

        private         void                            _fill()
        {
            _typeAssembly    = new List<EntityDeclaration>();
            _typeUser        = new List<EntityDeclaration>();
            _typeTable       = new List<EntityDeclaration>();
            _other           = new List<EntityDeclaration>();
            _code_all        = new List<SortEntry>();
            _code_internal   = new List<SortEntry>();
            _code_endpoints  = new List<SortEntry>();

            foreach(var declaration in _udts) {
                if (declaration.EntityType == DataModel.SymbolType.TypeExternal)
                    _typeAssembly.Add(declaration);
                else
                if (declaration.EntityType == DataModel.SymbolType.TypeUser)
                    _typeUser.Add(declaration);
                else
                    _typeTable.Add(declaration);
            }

            foreach(var declaration in _objects) {
                if (declaration.Declaration is Node.DeclarationObjectCode)
                    _code_all.Add(new SortEntry(declaration));
                else
                    _other.Add(declaration);
            }

            _typeAssembly   .Sort((i1, i2) => DataModel.EntityName.Compare(i1.EntityName, i2.EntityName));
            _typeUser       .Sort((i1, i2) => DataModel.EntityName.Compare(i1.EntityName, i2.EntityName));
            _typeTable      .Sort((i1, i2) => DataModel.EntityName.Compare(i1.EntityName, i2.EntityName));
            _other          .Sort((i1, i2) => DataModel.EntityName.Compare(i1.EntityName, i2.EntityName));
            _code_all       .Sort((i1, i2) => DataModel.EntityName.Compare(i1.EntityName, i2.EntityName));
        }
        private         void                            _getReferences()
        {
            Dictionary<DataModel.EntityName, SortEntry>         dictionary = new Dictionary<DataModel.EntityName, SortEntry>();

            foreach(SortEntry codeObject in _code_all)
                dictionary.Add(codeObject.EntityName, codeObject);

            foreach(SortEntry codeObject in _code_all) {
                DataModel.EntityName[]      objectReferences = codeObject.Declaration.Declaration.ObjectReferences();

                if (objectReferences != null) {
                    foreach(DataModel.EntityName name in objectReferences) {
                        if (dictionary.TryGetValue(name, out var referencedObject) && referencedObject != codeObject) {
                            ++(referencedObject.RequiredCount);
                            codeObject.Dependancies.Add(referencedObject);

                            (codeObject.Declaration.Declaration as Node.DeclarationObjectCode)?.Entity.CallEntity((referencedObject.Declaration.Declaration as Node.DeclarationObjectCode)?.Entity);
                        }
                    }
                }
            }

            _code_all.Sort((o1, o2) => DataModel.EntityName.Compare(o1.EntityName, o2.EntityName));
        }
        private         void                            _fix_recurvice()
        {
            foreach(SortEntry co in _code_all) {
                if (!co.InResult && co.IsRecursive()) {
                    throw new NotImplementedException("Recursive dependancies not supported (jet).");
                }
            }

        }
        private         void                            _sort_internal()
        {
            bool        f;

            do {
                f = false;

                foreach(SortEntry co in _code_all) {
                    if (co.CanStore()) {
                        co.InResult = true;
                        _code_internal.Add(co);
                        f = true;
                    }
                }
            }
            while (f);
        }
        private         void                            _sort_endpoint()
        {
            foreach(SortEntry co in _code_all) {
                if (co.isEndPoint) {
                    co.InResult = true;
                    _code_endpoints.Add(co);
                }
            }
        }
        private         void                            _check()
        {
            foreach(SortEntry codeObject in _code_all) {
                if (!codeObject.InResult)
                    throw new InvalidOperationException("Internal error failed to sort dependancies.");
            }
        }
        private         List<EntityDeclaration>         _result()
        {
            List<EntityDeclaration> rtn = new List<EntityDeclaration>();

            rtn.AddRange(_assemblies);
            rtn.AddRange(_typeAssembly);
            rtn.AddRange(_typeUser);
            rtn.AddRange(_typeTable);
            rtn.AddRange(_service);
            rtn.AddRange(_other);

            foreach(SortEntry codeObject in _code_internal)
                rtn.Add(codeObject.Declaration);

            foreach (SortEntry codeObject in _code_endpoints)
                rtn.Add(codeObject.Declaration);

            return rtn;
        }
    }
}
