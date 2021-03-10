using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    [DebuggerDisplay("{Value,nq}")]
    public readonly struct Namespace : IEquatable<Namespace>
    {
        internal static Namespace FromSymbol(INamespaceSymbol symbol)
        {
            return new Namespace(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
        }
        
        public readonly string Value;

        internal Namespace(string value)
        {
            Value = value;
        }

        internal bool IsRootNamespaceOf(Namespace ns)
        {
            return ns.Value.StartsWith(Value, StringComparison.InvariantCultureIgnoreCase);
        }
        
        public bool Equals(Namespace other)
        {
            return Value.Equals(other.Value, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is Namespace other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
