using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace App.Inspection
{
    [DebuggerDisplay("{Name,nq}")]
    public class Package
    {
        /// <summary>
        /// An absolute path to the DLL file the package was resolved from.
        /// </summary>
        public readonly string FilePath;
        
        /// <summary>
        /// A collection of types publicly available in the corresponding assembly of the
        /// package.
        /// </summary>
        public readonly ICollection<Type> ExportedTypes;

        /// <summary>
        /// The number of public types in the assembly.
        /// </summary>
        public int ExportedTypesCount => ExportedTypes.Count;

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name => Path.GetFileNameWithoutExtension(FilePath);
        
        public Package(string filePath, ICollection<Type> exportedTypes)
        {
            FilePath = filePath;
            ExportedTypes = exportedTypes;
        }
    }
}
