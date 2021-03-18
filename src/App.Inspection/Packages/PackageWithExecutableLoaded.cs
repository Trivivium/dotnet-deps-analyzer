using System.Collections.Generic;
using System.Diagnostics;

using NuGet.Versioning;

using App.Inspection.Executables;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package that was discovered from a metadata reference to a portable
    /// executable (DLL), but with no corresponding NuGet package in the project's explicit
    /// or transient dependencies.
    /// </summary>
    [DebuggerDisplay("{Name,nq}, {ReferenceType,nq}")]
    public class PackageWithExecutableLoaded : IPackageWithExecutableLoaded
    {
        private readonly PortableExecutableWrapper _executable;
        public PackageReferenceType ReferenceType { get; }
        public SemanticVersion Version { get; }
        public string Name { get; }
        public IPackage? Parent { get; }
        public IReadOnlyCollection<IPackage>? Children { get; }

        internal PackageWithExecutableLoaded(PackageReferenceType type, SemanticVersion version, string name, PortableExecutableWrapper executable)
        {
            _executable = executable;
            
            ReferenceType = type;
            Name = name;
            Version = version;
            Parent = null;
            Children = null;
        }
        
        PortableExecutableWrapper IPackageWithExecutableLoaded.GetExecutable()
        {
            return _executable;
        }
    }
}
