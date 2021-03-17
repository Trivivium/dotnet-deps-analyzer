using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    /// <summary>
    /// Resolves external packages from executable references of a project.
    /// See <see cref="PackageLoadContext"/> for more details.
    /// </summary>
    internal class PackageResolver
    {
        private readonly PackageLoadContext _context;
        private readonly ILogger _logger;

        public PackageResolver(PackageLoadContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets a collection of packages available from executable references after having
        /// filtered them by the excluded namespaces.
        /// </summary>
        /// <param name="exclusions">A collection of namespaces excluded from the analysis.</param>
        public IEnumerable<Package> GetPackages(NamespaceExclusionList exclusions)
        {
            var packages = new List<Package>();
            
            foreach (var reference in _context.GetExecutableReferences())
            {
                var path = reference.FilePath;

                if (path is null)
                {
                    _logger.LogError($"Failed to load referenced assembly: {reference.Display ?? "<unknown>"}");
                    
                    continue;
                }
                
                var assembly = _context.Load(path);
                var ns = GetNamespace(assembly);

                if (ns is null)
                {
                    _logger.LogVerbose($"Skipping referenced assembly: {reference.Display ?? "<unknown>"}. It has not exported types.");

                    continue;
                }

                if (exclusions.IsExcluded(ns))
                {
                    _logger.LogVerbose($"Skipping referenced assembly: {reference.Display ?? "<unknown>"}. The exported types are excluded.");

                    continue;
                }

                packages.Add(CreatePackage(reference, assembly));
            }

            return packages;
        }

        /// <summary>
        /// Gets the root namespace of an assembly.
        /// </summary>
        private static string? GetNamespace(Assembly assembly)
        {
            var exportedType = assembly.ExportedTypes.FirstOrDefault(type => type.Namespace != null);

            return exportedType != null
                ? exportedType.Namespace
                : null;
        }

        /// <summary>
        /// Creates a <see cref="Package"/> instance from the executable <paramref name="reference"/> and
        /// <paramref name="assembly"/>.
        /// </summary>
        /// <param name="reference">The executable reference that was loaded.</param>
        /// <param name="assembly">The assembly contained within the executable reference.</param>
        private static Package CreatePackage(MetadataReference reference, Assembly assembly)
        {
            var exportedTypes = assembly.GetExportedTypes();

            return new Package(reference.Display!, exportedTypes);
        }
    }
}
