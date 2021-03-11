using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace App.Inspection
{
    [DebuggerDisplay("{DisplayName,nq}")]
    public class Package
    {
        public readonly string FilePath;
        public readonly Namespace Namespace;
        public readonly ICollection<Type> ExportedTypes;

        public int ExportedTypesCount => ExportedTypes.Count;

        public string DisplayName => Path.GetFileNameWithoutExtension(FilePath);
        
        public Package(string filePath, Namespace ns, ICollection<Type> exportedTypes)
        {
            FilePath = filePath;
            Namespace = ns;
            ExportedTypes = exportedTypes;
        }
    }
}
