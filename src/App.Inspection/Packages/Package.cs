using System.Diagnostics;

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    [DebuggerDisplay("{Name,nq}")]
    public class Package
    {
        public readonly PackageReferenceType Type;

        /// <summary>
        /// The name of the package.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The version of the package.
        /// </summary>
        public readonly SemanticVersion? Version;

        /// <summary>
        /// The parent package referencing the package.
        /// </summary>
        public readonly Package? Parent;
        
        /// <summary>
        /// Indicates if the package is a transient package to an explicit
        /// package reference of the inspected project.
        /// </summary>
        public bool IsTransient => Parent != null;

        /// <summary>
        /// Creates a package with minimal information. This should only be used if the
        /// construction of a dependency graph failed, and the package was not resolved.
        /// </summary>
        /// <param name="type">Indicates the type of reference the package is in relation to the project.</param>
        /// <param name="name">The name of the package.</param>
        internal Package(PackageReferenceType type, string name)
        {
            Type = type;
            Name = name;
        }
        
        /// <summary>
        /// Creates an instance of a package that has a corresponding NuGet packaging spec that
        /// was inspected. 
        /// </summary>
        /// <param name="type">Indicates the type of reference the package is in relation to the project.</param>
        /// <param name="name">The name of the package.</param>
        /// <param name="version">The version of the package.</param>
        /// <param name="parent">The parent package that referenced this one as a dependency causing it to be loaded.</param>
        internal Package(PackageReferenceType type, string name, SemanticVersion version, Package? parent)
            : this(type, name)
        {
            Version = version;
            Parent = parent;
        }
    }
}
