using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NuGet.Versioning;

namespace App.Inspection.Packages
{
    internal class PackageGraphCache
    {
        private readonly IDictionary<string, NuGetPackage> _items = new Dictionary<string, NuGetPackage>();

        public IEnumerable<NuGetPackage> GetExplicitPackages()
        {
            return _items.Values.Where(item => item.ReferenceType == PackageReferenceType.Explicit);
        }
        
        public void Add(NuGetPackage package)
        {
            var key = CreateKey(package.Version, package.Name);

            if (!_items.ContainsKey(key))
            {
                _items.Add(key, package);
            }
        }
        
        public bool TryGet(SemanticVersion version, string name, [NotNullWhen(true)] out NuGetPackage? package)
        {
            var key = CreateKey(version, name);

            return _items.TryGetValue(key, out package);
        }
        
        private static string CreateKey(SemanticVersion version, string name)
        {
            return $"{name}:{version}";
        }
    }
}
