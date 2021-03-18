using System;
using System.Collections.Generic;
using System.Linq;

using App.Inspection.Packages;

namespace App.Inspection.Extensions
{
    public static class PackageExtensions
    {
        public static int GetUniqueDependenciesCount(this IPackage package)
        {
            if (package.Children is null)
            {
                return 0;
            }

            var ids = package.GetIds();

            return ids.Distinct().Count() - 1;
        }

        private static IEnumerable<Guid> GetIds(this IPackage package)
        {
            yield return package.ID;

            foreach (var id in package.Children?.SelectMany(GetIds) ?? Enumerable.Empty<Guid>())
            {
                yield return id;
            }
        }
    }
}
