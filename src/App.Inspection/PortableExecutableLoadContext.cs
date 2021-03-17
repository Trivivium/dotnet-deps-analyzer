using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection
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

        /// <summary>
        /// Loads an assembly by the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">An absolute path to a DLL file.</param>
        public Assembly Load(string path)
        {
            return _context.LoadFromAssemblyPath(path);
        }

        /// <summary>
        /// Gets a collection of the executable (DLL) references of the project this
        /// load context is associated with.
        /// </summary>
        public IReadOnlyCollection<PortableExecutableReference> GetExecutableReferences()
        {
            return _references;
        }
        
        /// <summary>
        /// Gets an instance of a <see cref="PortableExecutableResolver"/> capable of loading and filtering
        /// executable references into <see cref="Package"/> instances.
        /// </summary>
        public PortableExecutableResolver GetResolver()
        {
            return new PortableExecutableResolver(this, _logger);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
