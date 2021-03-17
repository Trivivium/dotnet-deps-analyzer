using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Executables
{
    internal sealed class PortableExecutableWrapper
    {
        private readonly MetadataReference _metadata;
        private readonly Assembly _assembly;

        private ICollection<Type>? _exportedTypeCache;

        /// <summary>
        /// An absolute file path to the location of the executable.
        /// </summary>
        public string Filepath => _metadata.Display!;

        public string Name => Path.GetFileNameWithoutExtension(Filepath);
        
        public Version Version => _assembly.GetName().Version ?? Version.Parse("0.0.0"); 

        /// <summary>
        /// A collection of types publicly available in the corresponding assembly of the
        /// package.
        /// </summary>
        public ICollection<Type> ExportedTypes
        {
            get
            {
                if (_exportedTypeCache is null)
                {
                    _exportedTypeCache = _assembly.GetExportedTypes();
                }
            
                return _exportedTypeCache;
            }
        }
        

        /// <summary>
        /// The number of public types in the assembly.
        /// </summary>
        public int ExportedTypesCount => ExportedTypes.Count;
        
        public PortableExecutableWrapper(MetadataReference metadata, Assembly assembly)
        {
            _metadata = metadata;
            _assembly = assembly;
        }
    }
}
