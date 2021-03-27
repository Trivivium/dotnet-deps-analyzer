using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Executables
{
    /// <summary>
    /// Loads executable references of a project.
    /// </summary>
    internal class PortableExecutableLoadContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly List<PortableExecutableReference> _references;
        private readonly MetadataLoadContext _context;

        public PortableExecutableLoadContext(Project project, ILogger logger)
        {
            _logger = logger;
            _references = project.MetadataReferences
                .Where(metadata => metadata.Properties.Kind == MetadataImageKind.Assembly)
                .Cast<PortableExecutableReference>()
                .ToList();
            
            var paths = _references
                .Where(executable => executable.FilePath != null)
                .Select(executable => executable.FilePath!)
                .ToList();

            _context = new MetadataLoadContext(new PathAssemblyResolver(paths));
        }

        public IEnumerable<PortableExecutableWrapper> GetExecutables(NamespaceExclusionList exclusions)
        {
            var resolver = new PortableExecutableResolver(this, _logger);

            return resolver.GetExecutables(exclusions);
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
        
        /// <summary>
        /// Loads an assembly by the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">An absolute path to a DLL file.</param>
        private Assembly Load(string path)
        {
            return _context.LoadFromAssemblyPath(path);
        }

        /// <summary>
        /// Gets a collection of the executable (DLL) references of the project this
        /// load context is associated with.
        /// </summary>
        private IReadOnlyCollection<PortableExecutableReference> GetExecutableReferences()
        {
            return _references;
        }
        
        private class PortableExecutableResolver
        {
            private readonly PortableExecutableLoadContext _context;
            private readonly ILogger _logger;

            public PortableExecutableResolver(PortableExecutableLoadContext context, ILogger logger)
            {
                _context = context;
                _logger = logger;
            }
            
            /// <summary>
            /// Gets a collection of packages available from executable references after having
            /// filtered them by the excluded namespaces.
            /// </summary>
            /// <param name="exclusions">A collection of namespaces excluded from the analysis.</param>
            public IEnumerable<PortableExecutableWrapper> GetExecutables(NamespaceExclusionList exclusions)
            {
                var wrappers = new List<PortableExecutableWrapper>();
                
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
                        _logger.LogVerbose($"Skipping referenced assembly: {reference.Display ?? "<unknown>"}. It has no exported types.");

                        continue;
                    }

                    var isSdkAssembly = IsSdkAssembly(assembly);
                    var isReferenceAssembly = IsReferenceAssembly(assembly);
                    
                    if (exclusions.IsExcluded(ns, isSdkAssembly, isReferenceAssembly))
                    {
                        string reason;

                        if (isSdkAssembly)
                        {
                            reason = "It is part of the SDK";
                        }
                        else if (isReferenceAssembly)
                        {
                            reason = "It is a reference assembly";
                        }
                        else
                        {
                            reason = "It is excluded";
                        }
                        
                        _logger.LogVerbose($"Skipping referenced assembly: {reference.Display ?? "<unknown>"}. {reason}.");

                        continue;
                    }

                    wrappers.Add(new PortableExecutableWrapper(reference, assembly));
                }

                return wrappers;
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

            private static bool IsSdkAssembly(Assembly assembly)
            {
                return false;   // TODO: Determine if the assembly is part of the local SDK installation. This needs to take different SDKs into consideration (e.g., Microsoft.Sdk.NET vs Microsoft.Sdk.NET.Web)
            }

            private static bool IsReferenceAssembly(Assembly assembly)
            {
                const string key = "System.Runtime.CompilerServices.ReferenceAssemblyAttribute";
                
                var attributes = assembly.GetCustomAttributesData();

                foreach (var attribute in attributes)
                {
                    if (attribute?.AttributeType.FullName?.Equals(key, StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
