namespace App.Inspection
{
    internal class Match
    {
        public readonly MatchKind Kind;
        public readonly Namespace Namespace;
        public MatchLocation? Location { get; set; }
        public MatchNamedType? ContainingType { get; set; }
        public MatchNamedType? InvokingType { get; set; }

        public Match(MatchKind kind, Namespace ns)
        {
            Kind = kind;
            Namespace = ns;
        }
    }
}
