using System;
using System.Collections.Generic;
using System.Diagnostics;

using NuGet.Versioning;

using App.Inspection.Executables;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package that was discovered from a metadata reference to a portable
    /// executable (DLL) that has a corresponding NuGet package declared as an
    /// <see cref="PackageReferenceType.Explicit"/> or <see cref="PackageReferenceType.Transient"/>
    /// dependency of the project.
    /// </summary>
    [DebuggerDisplay("{Name,nq}, {Version,nq}, {ReferenceType,nq}")]
    public class NuGetPackageWithExecutableLoaded : IPackageWithExecutableLoaded
    {
        private readonly IPackage _package;
        private readonly PortableExecutableWrapper _executable;

        /// <inheritdoc cref="ID"/>
        public Guid ID => _package.ID;
        
        /// <inheritdoc cref="IPackage.ReferenceType"/>
        public PackageReferenceType ReferenceType => _package.ReferenceType;
        
        /// <inheritdoc cref="IPackage.Version"/>
        public SemanticVersion Version => _package.Version;  
        
        /// <inheritdoc cref="IPackage.Name"/>
        public string Name => _package.Name;

        /// <inheritdoc cref="IPackage.Parents"/>
        public IReadOnlyCollection<IPackage>? Parents => _package.Parents;

        /// <inheritdoc cref="IPackage.Children"/>
        public IReadOnlyCollection<IPackage>? Children => _package.Children;

        internal NuGetPackageWithExecutableLoaded(NuGetPackage package, PortableExecutableWrapper executable)
        {
            _package = package;
            _executable = executable;
        }

        /// <inheritdoc cref="IPackageWithExecutableLoaded.GetExecutable()"/>
        PortableExecutableWrapper IPackageWithExecutableLoaded.GetExecutable()
        {
            return _executable;
        }

        public bool Equals(IPackage? other)
        {
            return _package.Equals(other);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(IPackage))
                return false;

            return Equals((IPackage) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_package, _executable);
        }
    }
}
