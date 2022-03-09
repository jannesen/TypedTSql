using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum SymbolUsageFlags
    {
        Unknown     = 0x8000,
        None        = 0,
        Read        = 0x0001,
        Write       = 0x0002,
        Select      = 0x0010,
        Insert      = 0x0020,
        Update      = 0x0040,
        Delete      = 0x0080,
        Declaration = 0x0100,
        Reference   = 0x0200
    }

    public abstract class SymbolData
    {
        public  abstract    bool                HasSymbol(ISymbol symbol);
        public  abstract    SymbolUsage         GetSymbolUsage(ISymbol symbol);
        public  abstract    void                UpdateSymbolUsage(DataModel.ISymbol symbol, DataModel.SymbolUsageFlags usage);
        public  abstract    ISymbol             GetDatamodelSymbol();
        public  abstract    ISymbol             GetClassificationSymbol();
        public  abstract    object              GetDeclaration();
    }

    public class SymbolUsage: SymbolData
    {
        public              ISymbol             Symbol      { get; private set; }
        public              SymbolUsageFlags    Usage       { get; private set; }

        public                                  SymbolUsage(ISymbol symbol, SymbolUsageFlags usage)
        {
            Symbol = symbol;
            Usage  = usage;
        }
        public  override    bool                HasSymbol(ISymbol symbol)
        {
            return object.ReferenceEquals(Symbol, symbol);
        }
        public  override    SymbolUsage         GetSymbolUsage(ISymbol symbol)
        {
            return (object.ReferenceEquals(Symbol, symbol)) ? this : null;
        }
        public  override    void                UpdateSymbolUsage(DataModel.ISymbol symbol, DataModel.SymbolUsageFlags usage)
        {
            if (object.ReferenceEquals(Symbol, symbol)) {
                Usage = usage;
            }
        }
        public  override    ISymbol             GetDatamodelSymbol()
        {
            return Symbol;
        }
        public  override    ISymbol             GetClassificationSymbol()
        {
            return Symbol;
        }
        public  override    object              GetDeclaration()
        {
            return Symbol.Declaration;
        }
    }

    public class SymbolSourceTarget: SymbolData
    {
        public              SymbolUsage         Source      { get; private set; }
        public              SymbolUsage         Target      { get; private set; }

        public                                  SymbolSourceTarget(SymbolUsage source, SymbolUsage target)
        {
            Source = source;
            Target = target;
        }

        public  override    bool                HasSymbol(ISymbol symbol)
        {
            return Source.HasSymbol(symbol) || Target.HasSymbol(symbol);
        }
        public  override    SymbolUsage         GetSymbolUsage(ISymbol symbol)
        {
            return Source.GetSymbolUsage(symbol) ?? Target.GetSymbolUsage(symbol);
        }
        public  override    void                UpdateSymbolUsage(DataModel.ISymbol symbol, DataModel.SymbolUsageFlags usage)
        {
            Source.UpdateSymbolUsage(symbol, usage);
            Target.UpdateSymbolUsage(symbol, usage);
        }
        public  override    ISymbol             GetDatamodelSymbol()
        {
            return null;
        }
        public  override    ISymbol             GetClassificationSymbol()
        {
            return Source.GetClassificationSymbol();
        }
        public  override    object              GetDeclaration()
        {
            return Source.GetDeclaration();
        }
    }

    public class SymbolWildcard: SymbolData
    {
        public              SymbolData[]        SymbolData  { get; private set; }

        public                                  SymbolWildcard(SymbolData[] symbolData)
        {
            SymbolData = symbolData;
        }

        public  override    bool                HasSymbol(ISymbol symbol)
        {
            foreach (var item in SymbolData) {
                if (item.HasSymbol(symbol)) {
                    return true;
                }
            }
            return false;
        }
        public  override    SymbolUsage         GetSymbolUsage(ISymbol symbol)
        {
            foreach (var item in SymbolData) {
                if (item.HasSymbol(symbol)) {
                    var usage = item.GetSymbolUsage(symbol);
                    if (usage != null) {
                        return usage;
                    }
                }
            }

            return null;
        }
        public  override    void                UpdateSymbolUsage(DataModel.ISymbol symbol, DataModel.SymbolUsageFlags usage)
        {
            foreach (var item in SymbolData) {
                if (item.HasSymbol(symbol)) {
                    item.UpdateSymbolUsage(symbol, usage);
                }
            }
        }
        public  override    ISymbol             GetDatamodelSymbol()
        {
            return null;
        }
        public  override    ISymbol             GetClassificationSymbol()
        {
            return null;
        }
        public  override    object              GetDeclaration()
        {
            return null;
        }
    }
}
