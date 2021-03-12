using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    internal class NamespaceExclusionList
    {
        private readonly List<Namespace> _namespaces;

        /// <summary>
        /// Creates an exclusion list based on the inspection parameters and the project being inspected. The
        /// project itself and any project-references are also added to the list.
        /// </summary>
        /// <param name="parameters">The inspection parameters provided by the user.</param>
        /// <param name="project">The project being inspected.</param>
        public static NamespaceExclusionList CreateFromParameters(InspectionParameters parameters, Project project)
        {
            var namespaces = parameters.ExcludedNamespaces
                .Select(ns => new Namespace(ns))
                .ToList();

            // Add references to type in the project itself to the exclusions 
            if (project.DefaultNamespace != null)
            {
                var projectRootNs = project.DefaultNamespace;
                
                namespaces.Add(new Namespace(projectRootNs));
                
                // Stem the root ns
                var offset = projectRootNs.IndexOf('.');

                if (offset > 0 && offset < projectRootNs.Length)
                {
                    namespaces.Add(new Namespace(projectRootNs.Substring(0, offset)));
                }
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

            return new NamespaceExclusionList(namespaces);
        }

        private NamespaceExclusionList(List<Namespace> namespaces)
        {
            _namespaces = namespaces;
        }
        
        /// <summary>
        /// Determines if the provided namespace corresponds to an
        /// excluded namespace.
        /// </summary>
        /// <param name="ns">The namespace to check.</param>
        public bool IsExcluded(string ns)
        {
            if ("System".Equals(ns, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return _namespaces.Any(exclusion => ns.StartsWith(exclusion.Value, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if the provided Roslyn namespace symbol corresponds to an
        /// excluded namespace.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        public bool IsExcluded(INamespaceSymbol symbol)
        {
            return IsExcluded(symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
        }
    }
}
