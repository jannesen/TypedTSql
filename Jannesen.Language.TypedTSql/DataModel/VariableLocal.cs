using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class VariableLocal: Variable, ISymbol
    {
        public  override        ISymbol                 Symbol              => this;
        public                  SymbolType              Type                => SymbolType.LocalVariable;
        public                  object                  Declaration         => _declaration;
        public                  DataModel.ISymbol       ParentSymbol        => null;
        public                  DataModel.ISymbol       SymbolNameReference => null;
        public  override        VariableFlags           Flags               => _flags;
        public  override        string                  SqlName             => _sqlName;

        private                 object                  _declaration;
        private                 VariableFlags           _flags;
        private                 string                  _sqlName;

        public                                          VariableLocal(string name, DataModel.ISqlType sqlType, object declaration, VariableFlags flags)
        {
            Name         = name;
            SqlType      = sqlType ?? new DataModel.SqlTypeAny();
            _declaration = declaration;
            _flags       = flags;
        }

        public  override        void                    setUsed()
        {
            _flags |= VariableFlags.Used;
        }
        public  override        void                    setAssigned()
        {
            if ((_flags & VariableFlags.Readonly) != 0)
                throw new InvalidProgramException("Can't assign value const parameter variable.");

            _flags |= VariableFlags.Assigned;
        }

        public                  void                    setSqlName(string sqlName)
        {
            _sqlName = sqlName;
        }
    }
}
