using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    internal class PackageLoadContext : IDisposable
    {
        private readonly ILogger _logger;
        private readonly List<PortableExecutableReference> _references;
        private readonly MetadataLoadContext _context;

        public PackageLoadContext(Project project, ILogger logger)
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

        public Assembly Load(string path)
        {
            return _context.LoadFromAssemblyPath(path);
        }

        public IReadOnlyCollection<PortableExecutableReference> GetReferences()
        {
            return _references;
        }
        
        public PackageResolver GetResolver()
        {
            return new PackageResolver(this, _logger);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
