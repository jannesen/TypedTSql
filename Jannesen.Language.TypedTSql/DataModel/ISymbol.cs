using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public interface ISymbol
    {
        SymbolType                  Type                    { get; }
        string                      Name                    { get; }
        object                      Declaration             { get; }        // TokenWithSymbol, DocumentSpan
        ISymbol                     Parent                  { get; }
        ISymbol                     SymbolNameReference     { get; }
    }
}
