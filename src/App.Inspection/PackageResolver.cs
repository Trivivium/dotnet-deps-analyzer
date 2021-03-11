using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    internal class PackageResolver
    {
        private readonly PackageLoadContext _context;
        private readonly ILogger _logger;

        public PackageResolver(PackageLoadContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public IEnumerable<Package> GetPackages(Project project, NamespaceExclusionList exclusions)
        {
            var packages = new List<Package>();
            
            foreach (var reference in _context.GetReferences())
            {
                var path = reference.FilePath;

                if (path is null)
                {
                    _logger.LogError($"Failed to load referenced assembly: {reference.Display ?? "<unknown>"} by project: {project.Name}");
                    
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

                packages.Add(CreatePackage(reference, assembly, ns));
            }

            return packages;
        }

        private static string? GetNamespace(Assembly assembly)
        {
            var exportedType = assembly.ExportedTypes.FirstOrDefault(type => type.Namespace != null);

            return exportedType != null
                ? exportedType.Namespace
                : null;
        }

        private static Package CreatePackage(PortableExecutableReference reference, Assembly assembly, string ns)
        {
            var exportedTypes = assembly.GetExportedTypes();

            return new Package(reference.Display!, new Namespace(ns), exportedTypes);
        }
    }
}
