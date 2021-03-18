using System.Xml.Linq;

using NuGet.Versioning;

using App.Inspection.Exceptions;

namespace App.Inspection.Packages
{
    internal sealed class PackageReference
    {
        public static PackageReference FromXElement(XElement elem)
        {
            var name = elem.Attribute("Include")?.Value ?? throw new InspectionException("Unable to resolve package name");
            var version = elem.Attribute("Version")?.Value ?? throw new InspectionException("Unable to resolve package version");

            if (!SemanticVersion.TryParse(version, out var sVersion))
            {
                throw new InspectionException($"Unable to parse the semantic version of package: {name}");
            }

            return new PackageReference(name, sVersion, parent: null, isExplicit: true);
        }
        
        public readonly string Name;
        public readonly SemanticVersion Version;
        public readonly PackageReference? Parent;
        public readonly bool IsExplicit;

        public PackageReference(string name, SemanticVersion version, PackageReference? parent = null, bool isExplicit = false)
        {
            Name = name;
            Version = version;
            Parent = parent;
            IsExplicit = isExplicit;
        }
    }
}
