using System;
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
        private List<IPackage>? _parents;
        
        /// <inheritdoc cref="ID"/>
        public Guid ID { get; }

        /// <inheritdoc cref="ReferenceType"/>
        public PackageReferenceType ReferenceType { get; }
        
        /// <inheritdoc cref="Version"/>
        public SemanticVersion Version { get; }
        
        /// <inheritdoc cref="Name"/>
        public string Name { get; }

        /// <inheritdoc cref="Parents"/>
        public IReadOnlyCollection<IPackage>? Parents => _parents;

        /// <inheritdoc cref="Children"/>
        public IReadOnlyCollection<IPackage>? Children { get; private set; }

        public NuGetPackage(PackageReferenceType referenceType, SemanticVersion version, string name, IPackage? parent)
        {
            ID = Guid.NewGuid();
            ReferenceType = referenceType;
            Version = version;
            Name = name;

            if (parent != null)
            {
                _parents = new List<IPackage>
                {
                    parent
                };
            }
        }

        /// <summary>
        /// Adds children representing dependencies to the package.
        /// </summary>
        /// <param name="children"></param>
        internal void AddChildren(List<NuGetPackage>? children)
        {
            Children = children;
        }

        internal void AddParent(IPackage parent)
        {
            if (_parents is null)
            {
                _parents = new List<IPackage>();
            }
            
            _parents.Add(parent);
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
            if (obj.GetType() != GetType())
                return false;

            return Equals((IPackage) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version, Name);
        }
    }
}
