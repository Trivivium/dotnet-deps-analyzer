using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace App.Inspection.Collectors
{
    internal class UsingDirectiveCollector : CSharpSyntaxWalker
    {
        public ICollection<string> UsingDirectives { get; } = new HashSet<string>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            UsingDirectives.Add(node.Name.ToString());
        }
    }
}
