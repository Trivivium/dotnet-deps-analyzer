using System.Collections.Generic;

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package of an external dependency.
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// Declares how the package is referenced from the project.
        /// </summary>
        public PackageReferenceType ReferenceType { get; }
        
        /// <summary>
        /// Declares the version of the package. This version may be based on
        /// either a NuGet package specification, or the file-version of a DLL
        /// with an <see cref="PackageReferenceType.Unknown"/> package <see cref="ReferenceType"/>.
        /// </summary>
        public SemanticVersion Version { get; }
        
        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Declares the package dependent of this one, if the <see cref="ReferenceType"/> of this
        /// package is <see cref="PackageReferenceType.Transient"/>. If the <see cref="ReferenceType"/> is
        /// <see cref="PackageReferenceType.Explicit"/> this is always <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ReferenceType"/> of this package is <see cref="PackageReferenceType.Unknown"/>
        /// this is always <see langword="null"/>.
        /// </remarks>
        public IPackage? Parent { get; }
        
        /// <summary>
        /// Declares the dependencies of this package. This applies to if the <see cref="ReferenceType"/> is
        /// both <see cref="PackageReferenceType.Explicit"/> or <see cref="PackageReferenceType.Transient"/>.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ReferenceType"/> of this package is <see cref="PackageReferenceType.Unknown"/>
        /// this is always <see langword="null"/>.
        /// </remarks>
        public IReadOnlyCollection<IPackage>? Children { get; }
    }
}
