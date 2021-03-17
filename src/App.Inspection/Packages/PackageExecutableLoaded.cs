using App.Inspection.Executables;

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    public class PackageExecutableLoaded : Package
    {
        /// <summary>
        /// The portable executable (DLL) file on disk where exported types of the package
        /// is loaded.
        /// </summary>
        internal readonly PortableExecutableWrapper Executable;

        internal PackageExecutableLoaded(PackageReferenceType type, string name, PortableExecutableWrapper executable)
            : base(type, name)
        {
            Executable = executable;
        }

        internal PackageExecutableLoaded(PackageReferenceType type, string name, SemanticVersion version, PortableExecutableWrapper executable, Package? parent)
            : base(type, name, version, parent)
        {
            Executable = executable;
        }
    }
}
