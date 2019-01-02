using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class Label: ISymbol
    {
        public                  SymbolType              Type                { get { return SymbolType.Label; } }
        public                  string                  Name                { get { return _name;            } }
        public                  object                  Declaration         { get { return _declaration;     } }
        public                  DataModel.ISymbol       Parent              { get { return null;             } }
        public                  DataModel.ISymbol       SymbolNameReference { get { return null;             } }

        private                 string                  _name;
        private                 object                  _declaration;

        public                                          Label(string name, object declaration)
        {
            this._name        = name;
            this._declaration = declaration;
        }
    }

    public class LabelList: Library.ListHashName<Label>
    {
        public                                          LabelList(): base(15)
        {

        }

        protected   override    string                  ItemKey(Label item)
        {
            return item.Name;
        }
    }
}
