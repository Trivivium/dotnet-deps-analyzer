using System;
using System.Diagnostics;

namespace App.Inspection
{
    [DebuggerDisplay("{Value,nq}")]
    public readonly struct Namespace : IEquatable<Namespace>
    {
        public readonly string Value;

        internal Namespace(string value)
        {
            Value = value;
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

        public static bool operator ==(Namespace left, Namespace right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Namespace left, Namespace right)
        {
            return !(left == right);
        }
    }
}
