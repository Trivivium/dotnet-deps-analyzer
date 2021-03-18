using NuGet.Versioning;

namespace App.Inspection.Packages
{
    /// <summary>
    /// Represents a package reference from a .csproj file.
    /// </summary>
    internal sealed class PackageReference
    {
        public readonly string Name;
        public readonly SemanticVersion Version;

        public PackageReference(string name, SemanticVersion version)
        {
            Name = name;
            Version = version;
        }
    }
}
