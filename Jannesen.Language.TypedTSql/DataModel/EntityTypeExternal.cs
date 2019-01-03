using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityTypeExternal: EntityType
    {
        public      override    SqlTypeFlags            TypeFlags           { get { return SqlTypeFlags.Interface;         } }
        public      override    InterfaceList           Interfaces          { get { testTranspiled(); return _interfaces;  } }
        public                  string                  ClassName           { get { return _className;                     } }
        public                  EntityAssembly          Assembly            { get { testTranspiled(); return _assembly;    } }

        private                 EntityAssembly          _assembly;
        private                 string                  _className;
        private                 InterfaceList           _interfaces;

        internal                                        EntityTypeExternal(DataModel.EntityName name, EntityFlags flags): base(SymbolType.TypeExternal, name, flags)
        {
        }
        internal                                        EntityTypeExternal(GlobalCatalog catalog, DataModel.EntityName entityName, SqlDataReader dataReader, int coloffset): base(SymbolType.TypeExternal, entityName, EntityFlags.SourceDatabase)
        {
            var assemblyName = dataReader.GetString(coloffset + 3);
            _assembly = catalog.GetAssembly(new DataModel.EntityName(null, assemblyName));
            _className = dataReader.GetString(coloffset + 4);
            if (_assembly == null)
                throw new GlobalCatalogException("Unknown assembly '" + assemblyName + "' referenced in type '" + EntityName.ToString() + "'.");
        }

        internal    new         void                    TranspileInit(object location)
        {
            base.TranspileInit(location);
            _assembly   = null;
            _interfaces = null;
        }
        internal                void                    Transpiled(EntityAssembly assembly, string className, InterfaceList interfaces)
        {
            if (_assembly != null && _assembly != assembly)
                throw new ErrorException("Can't redefined type.");

            _assembly   = assembly;
            _className  = className;
            _interfaces = interfaces;
            _setParent();
            Transpiled();
        }

        private                 void                    _setParent()
        {
            if (_interfaces != null) {
                foreach(var intf in _interfaces)
                    intf.SetParent(this);
            }
        }
    }
}
