using System;
using System.Diagnostics;
using System.Collections.Generic;

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
        
        /// <inheritdoc cref="ID"/>
        public Guid ID { get; }
        
        /// <inheritdoc cref="IPackage.ReferenceType"/>
        public PackageReferenceType ReferenceType { get; }
        
        /// <inheritdoc cref="IPackage.Version"/>
        public SemanticVersion Version { get; }
        
        /// <inheritdoc cref="IPackage.Name"/>
        public string Name { get; }
        
        /// <inheritdoc cref="IPackage.Parents"/>
        public IReadOnlyCollection<IPackage>? Parents { get; }
        
        /// <inheritdoc cref="IPackage.Children"/>
        public IReadOnlyCollection<IPackage>? Children { get; }

        internal PackageWithExecutableLoaded(PackageReferenceType type, SemanticVersion version, string name, PortableExecutableWrapper executable)
        {
            _executable = executable;
            
            ID = Guid.NewGuid();
            ReferenceType = type;
            Name = name;
            Version = version;
            Parents = null;
            Children = null;
        }
        
        PortableExecutableWrapper IPackageWithExecutableLoaded.GetExecutable()
        {
            return _executable;
        }
        
        public bool Equals(IPackage? other)
        {
            if (other is null)
            {
                return false;
            }

            return ID == other.ID;
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
            return HashCode.Combine(_executable, Name, Version, ReferenceType);
        }
    }
}
