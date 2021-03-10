
using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    internal class MatchLocation
    {
        public static MatchLocation? FromSyntaxNode(SyntaxNode node)
        {
            var location = node.GetLocation();
            
            if (!location.IsInSource)
            {
                return null;
            }

            var span = location.GetMappedLineSpan();

            // Roslyn line numbers are zero-indexed. So to match editors add one.
            var line = span.StartLinePosition.Line + 1;
            
            return new MatchLocation(span.Path, line);
        }
        
        public readonly string File;
        public readonly int? Line;

        public MatchLocation(string file, int? line = null)
        {
            File = file;
            Line = line;
        }
    }
}
