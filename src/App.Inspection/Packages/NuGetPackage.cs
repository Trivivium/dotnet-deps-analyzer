using System.Collections.Generic;
using System.Diagnostics;

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package discovered through either <see cref="PackageReferenceType.Explicit"/>
    /// or <see cref="PackageReferenceType.Transient"/> NuGet package references.
    /// </summary>
    [DebuggerDisplay("{Name,nq}, {Version,nq}, {ReferenceType,nq}")]
    internal class NuGetPackage : IPackage
    {
        /// <inheritdoc cref="ReferenceType"/>
        public PackageReferenceType ReferenceType { get; }
        
        /// <inheritdoc cref="Version"/>
        public SemanticVersion Version { get; }
        
        /// <inheritdoc cref="Name"/>
        public string Name { get; }
        
        /// <inheritdoc cref="Parent"/>
        public IPackage? Parent { get; }

        /// <inheritdoc cref="Children"/>
        public IReadOnlyCollection<IPackage>? Children { get; private set; }

        public NuGetPackage(PackageReferenceType referenceType, SemanticVersion version, string name, IPackage? parent)
        {
            ReferenceType = referenceType;
            Version = version;
            Name = name;
            Parent = parent;
        }

        /// <summary>
        /// Adds children representing dependencies to the package.
        /// </summary>
        /// <param name="children"></param>
        public void AddChildren(List<NuGetPackage>? children)
        {
            Children = children;
        }
    }
}
