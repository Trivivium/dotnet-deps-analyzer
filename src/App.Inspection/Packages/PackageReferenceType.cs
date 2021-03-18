namespace App.Inspection.Packages
{
    /// <summary>
    /// Declares the origin of how a package is referenced in the context of the
    /// project being inspected.
    /// </summary>
    public enum PackageReferenceType
    {
        /// <summary>
        /// Indicates the package is referenced explicitly as a dependency through NuGet.
        /// </summary>
        Explicit,
        
        /// <summary>
        /// Indicates the package is implicitly referenced by being a transient dependency
        /// to one of the <see cref="Explicit"/> package references.
        /// </summary>
        Transient,
        
        /// <summary>
        /// Indicates the origin of the package is unknown. This likely happens when an external
        /// dependency (both explicit or transitive) of a referenced project is leaked into
        /// this project.
        /// </summary>
        Unknown,
    }
}
