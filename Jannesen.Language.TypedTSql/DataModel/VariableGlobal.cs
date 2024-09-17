using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class VariableGlobal: Variable, ISymbol
    {
        public  override        ISymbol                 Symbol              => this;
        public                  SymbolType              Type                => SymbolType.GlobalVariable;
        public                  object                  Declaration         => null;
        public                  DataModel.ISymbol       ParentSymbol        => null;
        public                  DataModel.ISymbol       SymbolNameReference => null;
        public  override        VariableFlags           Flags               => VariableFlags.Nullable | VariableFlags.Readonly | VariableFlags.Used;

        protected                                       VariableGlobal()
        {
        }
        public                                          VariableGlobal(string name, DataModel.ISqlType sqlType)
        {
            Name         = name;
            SqlType      = sqlType;
        }

        public  override        void                    setUsed()
        {
        }
        public  override        void                    setAssigned()
        {
            throw new InvalidOperationException("Can't assign value const global variable.");
        }
    }
}
