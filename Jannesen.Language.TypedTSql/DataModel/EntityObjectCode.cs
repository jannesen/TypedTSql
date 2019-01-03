using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class EntityObjectCode: EntityObject
    {
        public                  ParameterList                   Parameters              { get { testTranspiled(); return _parameters;               } }
        public                  ISqlType                        Returns                 { get { testTranspiled(); return _returns;                  } }
        public                  IReadOnlyList<EntityObjectCode> Calling                 { get { return _calling;                                    } }
        public                  IReadOnlyList<EntityObjectCode> Calledby                { get { return _calledby;                                   } }
        public                  Node.DeclarationObjectCode      DeclarationObjectCode   { get { testTranspiled(); return _declarationObjectCode;    } }

        private                 ParameterList                   _parameters;
        private                 ISqlType                        _returns;
        private                 DataModel.TempTableList         _tempTables;
        public                  Node.DeclarationObjectCode      _declarationObjectCode;
        private                 List<EntityObjectCode>          _calling;
        private                 List<EntityObjectCode>          _calledby;

        internal                                                EntityObjectCode(SymbolType type, DataModel.EntityName name, EntityFlags flags): base(type, name, flags)
        {
        }

        internal                void                            CallEntity(EntityObjectCode entity)
        {
            if (entity != null) {
                if (_calling == null)
                    _calling  = new List<EntityObjectCode>();

                _calling.Add(entity);

                if (entity._calledby == null)
                    entity._calledby = new List<EntityObjectCode>();

                entity._calledby.Add(this);
            }
        }

        internal    override    void                            TranspileBefore()
        {
            base.TranspileBefore();
            _declarationObjectCode = null;
            _calling               = null;
            _calledby              = null;
        }
        public                  void                            TranspileInit(Node.DeclarationObjectCode declarationObjectCode, object location)
        {
            base.TranspileInit(location);
            _declarationObjectCode = declarationObjectCode;
            _parameters            = null;
            _returns               = null;
            _tempTables            = null;
        }
        public                  void                            Transpiled(ParameterList parameters = null, ISqlType returns = null)
        {
            _parameters = parameters;
            _returns    = returns;
            base.Transpiled();
        }

        public                  EntityObjectCode[]              CalledbyRecursive()
        {
            var callers = new HashSet<EntityObjectCode>();

            _calledbyRecursiveWalker(callers);

            return callers.ToArray();
        }
        public                  TempTable                       TempTableGet(string name)
        {
            return _tempTables != null && _tempTables.TryGetValue(name, out var tempTable) ? tempTable : null;
        }
        public                  DataModel.TempTable             TempTableGetRecursive(string name, out bool ambiguous)
        {
            ambiguous = false;

            var     rtn = TempTableGet(name);

            foreach(var caller in CalledbyRecursive()) {
                var t = caller.TempTableGet(name);
                if (t != null) {
                    if (rtn == null)
                        rtn = t;
                    else
                        ambiguous = true;
                }
            }

            return rtn;
        }
        public                  bool                            TempTableAdd(string name, object declaration, ColumnList columns, IndexList indexes, out DataModel.TempTable tempTable)
        {
            if (_tempTables == null)
                _tempTables = new DataModel.TempTableList(4);

            if (_tempTables.TryGetValue(name, out tempTable)) {
                if (tempTable.Declaration == declaration) {
                    if ((ColumnList)tempTable.Columns != columns ||
                        tempTable.Indexes != indexes)
                        throw new InvalidOperationException("Internal error in EntityObjectCode.TempTableAdd().");

                    return true;
                }

                tempTable = null;
                return false;
            }

            tempTable = new TempTable(name, declaration, columns, indexes);

            return _tempTables.TryAdd(tempTable);
        }

        public      override    string                          DatabaseReadFromCmd()
        {
            switch(Type) {
            case SymbolType.View:
            case SymbolType.FunctionInlineTable:
            case SymbolType.FunctionMultistatementTable:
            case SymbolType.FunctionMultistatementTable_clr:
                return "EXEC " + (EntityName.Database!=null ? EntityName.Database+".":"")+ "sys.sp_executesql " +
                                        Library.SqlStatic.QuoteNString("DECLARE @object_id INT=OBJECT_ID(@objectname)\n" +
                                                                       Parameter.SqlStatement + "\n" +
                                                                       ColumnDS.SqlStatement + "\n" +
                                                                       IndexColumn.SqlStatement + "\n" +
                                                                       Index.SqlStatement) +
                                    ",\n N'@objectname nvarchar(1024)', @objectname="+Library.SqlStatic.QuoteString(EntityName.SchemaName);

            default:
                return "EXEC " + (EntityName.Database!=null ? EntityName.Database+".":"")+ "sys.sp_executesql " +
                                        Library.SqlStatic.QuoteNString("DECLARE @object_id INT=OBJECT_ID(@objectname)\n" +
                                                                       Parameter.SqlStatement) +
                                    ",\n N'@objectname nvarchar(1024)', @objectname="+Library.SqlStatic.QuoteString(EntityName.SchemaName);
            }
        }
        public      override    void                            DatabaseReadFromResult(GlobalCatalog catalog, SqlDataReader datareader)
        {
            // Reader parameters
            {
                var     parameters   = new List<Parameter>();

                while (datareader.Read()) {
                    if (datareader.GetInt32(0) == 0)
                        _returns = catalog.GetSqlType(EntityName.Database, datareader, 2);
                    else
                        parameters.Add(new Parameter(catalog, EntityName.Database, datareader));
                }

                if (parameters.Count > 0)
                    _parameters = new ParameterList(parameters);
            }

            if (Type==SymbolType.TableUser                      ||
                Type==SymbolType.TableSystem                    ||
                Type==SymbolType.TableInternal                  ||
                Type==SymbolType.View                           ||
                Type==SymbolType.FunctionInlineTable            ||
                Type==SymbolType.FunctionMultistatementTable    ||
                Type==SymbolType.FunctionMultistatementTable_clr)
            {
                if (!datareader.NextResult())
                    throw new ErrorException("Can't goto columns result.");

                _returns = new SqlTypeTable(this, catalog, datareader);
            }

            EntityFlags &= ~EntityFlags.PartialLoaded;
        }

        private                 void                            _calledbyRecursiveWalker(HashSet<EntityObjectCode> callers)
        {
            if (_calledby != null) {
                foreach (var c in _calledby) {
                    if (!callers.Contains(c)) {
                        callers.Add(c);
                        c._calledbyRecursiveWalker(callers);
                    }
                }
            }
        }
    }
}
