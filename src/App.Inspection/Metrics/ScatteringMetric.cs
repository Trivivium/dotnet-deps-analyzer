using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace App.Inspection.Metrics
{
    internal sealed class ScatteringMetric : IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Registry registry, Package package)
        {
            var uniqueTypesCount = GetUniqueTypeCount(compilation);
            var uniqueUseLocationCount = registry.GetMatchesForPackage(package)
                .Where(match => match.Location != null)
                .Select(match => match.Location!.File)
                .Distinct()
                .Count();

            var percentage = uniqueUseLocationCount / (float) uniqueTypesCount * 100;
            
            return new ScatteringMetricResult(uniqueTypesCount, uniqueUseLocationCount, percentage);
        }

        private static int GetUniqueTypeCount(Compilation compilation)
        {
            return compilation.SyntaxTrees
                .SelectMany(tree => tree
                    .GetRoot()
                    .DescendantNodes()
                    .Where(node => node is ClassDeclarationSyntax || node is StructDeclarationSyntax || node is EnumDeclarationSyntax)
                )
                .Count();
        }
        //
        // private static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceSymbol ns)
        // {
        //     foreach (var type in ns.GetTypeMembers())
        //     {
        //         foreach (var nestedType in GetNestedTypes(type))
        //         {
        //             yield return nestedType;
        //         }
        //     }
        //
        //     foreach (var nestedNamespace in ns.GetNamespaceMembers())
        //     {
        //         foreach (var type in GetTypes(nestedNamespace))
        //         {
        //             yield return type;
        //         }
        //     }
        // }
        //
        // private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
        // {
        //     yield return type;
        //
        //     foreach (var nestedType in type.GetTypeMembers().SelectMany(GetNestedTypes))
        //         yield return nestedType;
        // }
    }
}
