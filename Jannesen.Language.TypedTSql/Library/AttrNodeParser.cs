using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jannesen.Language.TypedTSql.Library
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class AttrNodeParser: Attribute
    {
        public              Core.TokenID            FirstToken  { get; private set; }
        public              string                  NameToken   { get; private set; }
        public              int                     Prio        { get; private set; }

        public                                      AttrNodeParser(Core.TokenID firstToken)
        {
            FirstToken = firstToken;
            Prio       = 0;
        }
        public                                      AttrNodeParser(Core.TokenID firstToken, int prio)
        {
            FirstToken = firstToken;
            Prio       = prio;
        }
        public                                      AttrNodeParser(string nameToken)
        {
            FirstToken = Core.TokenID.Name;
            NameToken  = nameToken.ToUpperInvariant();
            Prio       = 0;
        }
        public                                      AttrNodeParser(string nameToken, int prio)
        {
            FirstToken = Core.TokenID.Name;
            NameToken  = nameToken.ToUpperInvariant();
            Prio       = prio;
        }

        public              bool                    CanParse(Core.ParserReader reader)
        {
            var token = reader.CurrentToken;

            if (token.ID == FirstToken) {
                if (NameToken == null)
                    return true;

                if (NameToken == token.Text.ToUpperInvariant())
                    return true;
            }

            return false;
        }
    }

    public class DeclarationParser: AttrNodeParser
    {
        public                                      DeclarationParser(Core.TokenID firstToken): base(firstToken)
        {
        }
        public                                      DeclarationParser(Core.TokenID firstToken, int prio): base(firstToken, prio)
        {
        }
        public                                      DeclarationParser(string nameToken): base(nameToken)
        {
        }
        public                                      DeclarationParser(string nameToken, int prio): base(nameToken, prio)
        {
        }
    }

    public class StatementParser: AttrNodeParser
    {
        public                                      StatementParser(Core.TokenID firstToken): base(firstToken)
        {
        }
        public                                      StatementParser(Core.TokenID firstToken, int prio): base(firstToken, prio)
        {
        }
    }

    public class NodeParser<T> where T: Core.AstParseNode
    {
        class ParserClass
        {
            public  readonly    AttrNodeParser          Attr;

            private             MethodInfo              _canParseMethod;
            private             ConstructorInfo         _constructor;

            public                                      ParserClass(AttrNodeParser attr, Type parserClassType)
            {
                try {
                    Attr = attr;

                    _checkBaseType(parserClassType, typeof(T));

                    _constructor    = parserClassType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(Core.ParserReader), typeof(Node.IParseContext) }, null);
                    if (_constructor == null)
                        throw new InvalidOperationException("Can't get constructor.");

                    _canParseMethod = parserClassType.GetMethod("CanParse",  BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Core.ParserReader), typeof(Node.IParseContext) }, null);

                    if (_canParseMethod == null && attr.Prio > 0)
                        throw new InvalidOperationException("Missing CanParse or Prio > 0.");

                    if (_canParseMethod != null && attr.Prio <= 0)
                        throw new InvalidOperationException("CanParse and Prio <= 0.");
                }
                catch(Exception err) {
                    throw new InvalidOperationException("NodeParser failed type " + parserClassType.FullName + ".", err);
                }
            }

            public              bool                    CanParse(Core.ParserReader reader, Node.IParseContext parseContext)
            {
                if (Attr.CanParse(reader)) {
                    if (_canParseMethod == null)
                        return true;

                    return (bool)_canParseMethod.Invoke(null, new object[] { reader, parseContext });
                }

                return false;
            }
            public              T                       Parse(Core.ParserReader reader, Node.IParseContext parseContext)
            {
                try {
                    return (T)_constructor.Invoke(new object[] { reader, parseContext } );
                }
                catch(TargetInvocationException err) {
                    throw err.InnerException;
                }
            }

            public              string                  GetName()
            {
                return Attr.FirstToken.ToString();
            }

            private static      void                    _checkBaseType(Type type, Type baseClass)
            {
                for (Type t = type ; t != null ; t = t.BaseType) {
                    if (t == baseClass)
                        return;
                }

                throw new ArgumentException("Type " + type.FullName + " is not a derived of " + baseClass.Name + ".");
            }
        }

        private         Dictionary<Core.TokenID, List<ParserClass>>       _nodeParsers;

        public                                  NodeParser()
        {
            _nodeParsers = new Dictionary<Core.TokenID, List<ParserClass>>(64);
        }

        public          void                    AddParser(AttrNodeParser attr, Type parserClassType)
        {
            var parserClass = new ParserClass(attr, parserClassType);

            if (_nodeParsers.TryGetValue(parserClass.Attr.FirstToken, out var parsers)) {
                parsers.Add(parserClass);

                parsers.Sort((ParserClass i1, ParserClass i2) => i2.Attr.Prio - i1.Attr.Prio);
            }
            else
                _nodeParsers.Add(parserClass.Attr.FirstToken, new List<ParserClass>() { parserClass } );
        }

        public          bool                    CanParse(Core.ParserReader reader, Node.IParseContext parseContext)
        {
            if (_nodeParsers.TryGetValue(reader.CurrentToken.ID, out var parsers)) {
                for (int i = 0 ; i < parsers.Count ; ++i) {
                    if (parsers[i].CanParse(reader, parseContext))
                        return true;
                }
            }

            return false;
        }
        public          T                       Parse(Core.ParserReader reader, Node.IParseContext parseContext)
        {
            if (_nodeParsers.TryGetValue(reader.CurrentToken.ID, out var parsers)) {
                for (int i = 0 ; i < parsers.Count ; ++i) {
                    if (parsers[i].CanParse(reader, parseContext))
                        return parsers[i].Parse(reader, parseContext);
                }
            }

            var names = new HashSet<string>();
            var rtn   = new StringBuilder();

            foreach(var parsers2 in _nodeParsers.Values) {
                foreach(var parser in parsers2) {
                    var name = parser.GetName();

                    if (name != null && !names.Contains(name))
                        names.Add(name);
                }
            }

            rtn.Append("Except ");

            var sortedNames = new List<string>(names);
            sortedNames.Sort();

            for(int i = 0 ; i < sortedNames.Count ; ++i) {
                if (i > 0)
                    rtn.Append(", ");

                rtn.Append(sortedNames[i]);
            }

            rtn.Append(" got ");
            rtn.Append(reader.CurrentToken.ToString());
            rtn.Append(".");

            throw new ParseException(reader.CurrentToken, rtn.ToString());
        }
    }
}
