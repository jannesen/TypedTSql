using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class VariableLocal: Variable
    {
        public  override        SymbolType              Type                { get { return DataModel.SymbolType.LocalVariable; } }
        public  override        object                  Declaration         { get { return _declaration; } }
        public  override        VariableFlags           Flags               { get { return _flags; } }
        public  override        string                  SqlName             { get { return _sqlName; } }

        private                 object                  _declaration;
        private                 VariableFlags           _flags;
        private                 string                  _sqlName;

        public                                          VariableLocal(string name, DataModel.ISqlType sqlType, object declaration, VariableFlags flags)
        {
            Name         = name;
            SqlType      = sqlType;
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
