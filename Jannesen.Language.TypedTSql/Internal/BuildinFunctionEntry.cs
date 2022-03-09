using System;
using System.Reflection;

namespace Jannesen.Language.TypedTSql.Internal
{
    delegate Core.AstParseNode          AstParseNodeConstructor(Core.ParserReader reader);

    class BuildinFunctionDeclaration: DataModel.ISymbol
    {
        public                  DataModel.SymbolType            Type                    { get { return DataModel.SymbolType.BuildinFunction; } }
        public                  string                          Name                    { get; private set; }
        public                  string                          FullName             { get { return Name; } }
        public                  object                          Declaration             { get { return null; } }
        public                  DataModel.ISymbol               ParentSymbol            { get { return null; } }
        public                  DataModel.ISymbol               SymbolNameReference     { get { return null; } }


        private                 ConstructorInfo                 _constructor;

        public                                                  BuildinFunctionDeclaration(Type parserClass, bool rowset)
        {
            _checkBaseType(parserClass, typeof(Core.AstParseNode));

            Name = parserClass.Name;

            if (Enum.TryParse<Core.TokenID>(Name, true, out Core.TokenID tokenid)) {
                if (!(Core.TokenID._beginkeywordswithsymbol < tokenid && tokenid < Core.TokenID._endkeywordswithsymbol))
                    throw new InvalidOperationException("Name is keyword withoutsymbol.");
            }

            var args = rowset ? new Type[] { typeof(BuildinFunctionDeclaration), typeof(Core.ParserReader), typeof(bool) }
                              : new Type[] { typeof(BuildinFunctionDeclaration), typeof(Core.ParserReader) };

            _constructor    = parserClass.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, args, null);
            if (_constructor == null)
                throw new InvalidOperationException("Can't get constructor.");
        }

        public                  Core.AstParseNode               Parse(Core.ParserReader reader)
        {
            try {
                return (Core.AstParseNode)_constructor.Invoke(new object[] { this, reader } );
            }
            catch(TargetInvocationException err) {
                throw err.InnerException;
            }
        }
        public                  Core.AstParseNode               Parse(Core.ParserReader reader, bool b)
        {
            try {
                return (Core.AstParseNode)_constructor.Invoke(new object[] { this, reader, b } );
            }
            catch(TargetInvocationException err) {
                throw err.InnerException;
            }
        }

        private static          void                            _checkBaseType(Type type, Type baseClass)
        {
            for (Type t = type ; t != null ; t = t.BaseType) {
                if (t == baseClass)
                    return;
            }

            throw new ArgumentException("Type " + type.FullName + " is not a derived of " + baseClass.Name + ".");
        }
    }

    class BuildinFunctionDeclarationList: Library.ListHashName<BuildinFunctionDeclaration>
    {
        public                                                  BuildinFunctionDeclarationList(int capacity): base(capacity)
        {
        }

        protected   override    string                          ItemKey(BuildinFunctionDeclaration item)
        {
            return item.Name;
        }
    }
}
