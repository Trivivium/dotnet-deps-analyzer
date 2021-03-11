using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace App.Inspection
{
    internal class Registry
    {
        private readonly ICollection<Namespace> _excludedNamespaces;
        private readonly ICollection<Match> _matches = new List<Match>();

        public static Registry CreateFromParameters(InspectionParameters parameters, Project project)
        {
            var namespaces = parameters.ExcludedNamespaces
                .Select(ns => new Namespace(ns))
                .ToList();

            // Add references to type in the project itself to the exclusions 
            if (project.DefaultNamespace != null)
            {
                namespaces.Add(new Namespace(project.DefaultNamespace));
            }
            
            // Add referenced projects in the same solution to the exclusions
            foreach (var reference in project.ProjectReferences)
            {
                var referencedProject = project.Solution.GetProject(reference.ProjectId);

                if (referencedProject?.DefaultNamespace != null)
                {
                    namespaces.Add(new Namespace(referencedProject.DefaultNamespace));
                }
            }
            
            return new Registry(namespaces);
        }
        
        private Registry(ICollection<Namespace> excludedNamespaces)
        {
            _excludedNamespaces = excludedNamespaces;
        }

        public IEnumerable<Match> GetMatchesForPackage(Package package)
        {
            return _matches.Where(match => package.Namespace.IsRootNamespaceOf(match.Namespace));
        }

        public void AddUsingDirective(UsingDirectiveSyntax node, INamespaceSymbol symbol)
        {
            var ns = Namespace.FromSymbol(symbol);
            
            if (IsExcludedNamespace(ns))
            {
                return;
            }

            var match = new Match(MatchKind.UsingDirective, ns)
            {
                Location = MatchLocation.FromSyntaxNode(node)
            };

            _matches.Add(match);
        }

        public void AddMethodInvocation(InvocationExpressionSyntax node, IMethodSymbol symbol, INamedTypeSymbol? invoker)
        {
            var ns = Namespace.FromSymbol(symbol.ContainingNamespace);
            
            if (IsExcludedNamespace(ns))
            {
                return;
            }
            
            var match = new Match(MatchKind.MethodInvocation, ns)
            {
                Location = MatchLocation.FromSyntaxNode(node),
                ContainingType = MatchNamedType.FromSymbol(symbol.ContainingType)
            };

            if (invoker != null)
            {
                match.InvokingType = MatchNamedType.FromSymbol(invoker);
            }

            _matches.Add(match);
        }

        public void AddObjectCreation(ObjectCreationExpressionSyntax node, IMethodSymbol symbol)
        {
            var ns = Namespace.FromSymbol(symbol.ContainingNamespace);
            
            if (IsExcludedNamespace(ns))
            {
                return;
            }

            var match = new Match(MatchKind.ConstructorInvocation, ns)
            {
                Location = MatchLocation.FromSyntaxNode(node)
            };
            
            _matches.Add(match);
        }
        
        private bool IsExcludedNamespace(Namespace ns)
        {
            if (ns.Value.Equals("System"))
            {
                return true;
            }

            for (var i = 0; i < _excludedNamespaces.Count; i++)
            {
                if (_excludedNamespaces.ElementAt(i).IsRootNamespaceOf(ns))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
