using System;
using System.Collections.Generic;

namespace App.Inspection
{
    internal sealed class PortableExecutableWrapper
    {
        /// <summary>
        /// An absolute file path to the location of the executable.
        /// </summary>
        public readonly string Filepath;
        
        /// <summary>
        /// A collection of types publicly available in the corresponding assembly of the
        /// package.
        /// </summary>
        public readonly ICollection<Type> ExportedTypes;

        /// <summary>
        /// The number of public types in the assembly.
        /// </summary>
        public int ExportedTypesCount => ExportedTypes.Count;
        
        public PortableExecutableWrapper(string filepath, ICollection<Type> exportedTypes)
        {
            Filepath = filepath;
            ExportedTypes = exportedTypes;
        }
    }
}
