using App.Inspection.Executables;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package with runtime types loaded from a portable
    /// executable (DLL).
    /// </summary>
    internal interface IPackageWithExecutableLoaded : IPackage
    {
        /// <summary>
        /// Gets the executable containing the exported types of the package.
        /// </summary>
        public PortableExecutableWrapper GetExecutable();
    }
}
