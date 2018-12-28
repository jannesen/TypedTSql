using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class VariableGlobal: Variable
    {
        public  override        DataModel.SymbolType    Type                { get { return DataModel.SymbolType.GlobalVariable; } }
        public  override        object                  Declaration         { get { return null; } }
        public  override        VariableFlags           Flags               { get { return VariableFlags.Nullable | VariableFlags.Readonly | VariableFlags.Used; } }

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
