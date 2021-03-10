using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    internal class MatchNamedType
    {
        public static MatchNamedType FromSymbol(INamedTypeSymbol symbol)
        {
            return new MatchNamedType(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
        }
        
        public readonly string ClassName;

        public MatchNamedType(string className)
        {
            ClassName = className;
        }
    }
}
