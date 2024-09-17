using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public enum TAttributeType
    {
        String      = 1,
        Integer     = 2,
        Number      = 3,
        Enum        = 4,
        Flags       = 5
    }

    public class TAttribute: ISymbol
    {
        public  readonly    object                  Declaration;
        public  readonly    string                  Name;
        public  readonly    TAttributeType          Type;
        public  readonly    TAttributeEnumValue[]   Names;

                            SymbolType          ISymbol.Type                    => SymbolType.Attribute;
                            string              ISymbol.Name                    => Name;
                            string              ISymbol.FullName                => Name;
                            object              ISymbol.Declaration             => Declaration;
                            ISymbol             ISymbol.ParentSymbol            => null;
                            ISymbol             ISymbol.SymbolNameReference     => null;


        public                                  TAttribute(object declaration, string name, TAttributeType type, TAttributeEnumValue[] names=null)
        {
            Declaration = declaration;
            Name        = name;
            Type        = type;
            Names       = names;

            if (names != null) {
                foreach(var n in names) {
                    n.SetParent(this);
                }
            }
        }

        public            TAttributeEnumValue   FindName(string name)
        {
            for (int i = 0 ; i < Names.Length ; ++i) {
                if (Names[i].Name == name) {
                    return Names[i];
                }
            }

            return null;
        }
    }

    public class TAttributeEnumValue: ISymbol
    {
        public  readonly    object              Declaration;
        public  readonly    string              Name;

                            SymbolType          ISymbol.Type                    => SymbolType.Attribute;
                            string              ISymbol.Name                    => Name;
                            string              ISymbol.FullName                => Name;
                            object              ISymbol.Declaration             => Declaration;
                            ISymbol             ISymbol.ParentSymbol            => _parent;
                            ISymbol             ISymbol.SymbolNameReference     => null;

        private             TAttribute          _parent;

        public                                  TAttributeEnumValue(object declaration, string name)
        {
            Declaration = declaration;
            Name        = name;
        }

        internal            void                SetParent(TAttribute attr)
        {
            _parent = attr;
        }
    }

    public interface IAttributes
    {
        IAttributeValue                 this[int idx]       {get;}
        int                             Count               {get;}
        IAttributeValue                 Find(string Name);
        IEnumerator<IAttributeValue>    GetEnumerator();
    }

    public interface IAttributeValue
    {
        TAttribute      Attr        {get;}
        object          Value       {get;}
    }
}
