using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum VariableFlags
    {
        None                = 0,
        Nullable            = 0x0001,
        Output              = 0x0002,
        XmlDocument         = 0x0004,
        HasDefaultValue     = 0x0008,
        Readonly            = 0x0010,
        SaveCast            = 0x0020,
        Returns             = 0x0100,
        Used                = 0x0200,
        Assigned            = 0x0400,
        VarDeclare          = 0x8000
    }

    public abstract class Variable: ISymbol
    {
        public  abstract        SymbolType              Type                { get; }
        public                  string                  Name                { get; protected set; }
        public  virtual         string                  SqlName             { get { return null; } }
        public                  ISqlType                SqlType             { get; protected set; }
        public  abstract        object                  Declaration         { get; }
        public                  DataModel.ISymbol       Parent              { get { return null; } }
        public                  DataModel.ISymbol       SymbolNameReference { get { return null; } }
        public  abstract        VariableFlags           Flags               { get; }

        public                  bool                    isNullable          { get { return (Flags & VariableFlags.Nullable       ) != 0;    } }
        public                  bool                    isOutput            { get { return (Flags & VariableFlags.Output         ) != 0;    } }
        public                  bool                    isXmlDocument       { get { return (Flags & VariableFlags.XmlDocument    ) != 0;    } }
        public                  bool                    hasDefaultValue     { get { return (Flags & VariableFlags.HasDefaultValue) != 0;    } }
        public                  bool                    isSaveCast          { get { return (Flags & VariableFlags.SaveCast       ) != 0;    } }
        public                  bool                    isReadonly          { get { return (Flags & VariableFlags.Readonly       ) != 0;    } }
        public                  bool                    isUsed              { get { return (Flags & VariableFlags.Used           ) != 0;    } }
        public                  bool                    isAssigned          { get { return (Flags & VariableFlags.Assigned       ) != 0;    } }
        public                  bool                    isVarDeclare        { get { return (Flags & VariableFlags.VarDeclare     ) != 0;    } }

        public  abstract        void                    setUsed();
        public  abstract        void                    setAssigned();
    }

    public class VariableList: Library.ListHashName<Variable>
    {
        public                                          VariableList(): base(257)
        {

        }
        public                                          VariableList(params Variable[] variable): base(variable)
        {
        }

        protected   override    string                  ItemKey(Variable item)
        {
            return item.Name;
        }
    }
}
