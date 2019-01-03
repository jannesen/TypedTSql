using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class Interface: ISymbol
    {
        public              SymbolType              Type                    { get; private set; }
        public              string                  Name                    { get; private set; }
        public              object                  Declaration             { get; private set; }
        public              ISymbol                 Parent                  { get; private set; }
        public              ISymbol                 SymbolNameReference     { get { return null;                 } }
        public              ParameterList           Parameters              { get; private set; }
        public              ISqlType                Returns                 { get; private set; }

        public                                      Interface(SymbolType type, string name, object declaration, ParameterList parameters, ISqlType returns)
        {
            this.Type        = type;
            this.Name        = name;
            this.Declaration = declaration;
            this.Parameters  = parameters;
            this.Returns     = returns;
        }

        public              void                    SetParent(ISymbol parent)
        {
            this.Parent = parent;
        }
    }

    public class InterfaceList: List<Interface>
    {
        public                                      InterfaceList(): base(16)
        {
        }
        public                                      InterfaceList(int n): base(n)
        {
        }

        public  new         void                    Add(Interface newInterface)
        {
            foreach(var intf in this) {
                if (intf.Name == newInterface.Name) {
                    if (intf.Type         == SymbolType.ExternalStaticProperty || intf.Type         == SymbolType.ExternalProperty ||
                        newInterface.Type == SymbolType.ExternalStaticProperty || newInterface.Type == SymbolType.ExternalProperty)
                        throw new ErrorException("'" + newInterface.Name + "' already defined.");
                }
            }

            base.Add(newInterface);
        }
    }
}
