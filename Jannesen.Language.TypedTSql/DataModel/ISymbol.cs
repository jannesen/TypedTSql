using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public interface ISymbol
    {
        SymbolType                  Type                    { get; }
        string                      Name                    { get; }
        string                      FullName                { get; }
        object                      Declaration             { get; }        // TokenWithSymbol, DocumentSpan
        ISymbol                     ParentSymbol            { get; }
        ISymbol                     SymbolNameReference     { get; }
    }
}
