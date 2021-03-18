using System;
using System.Collections.Generic;
using System.Linq;

using App.Inspection;
using App.Inspection.Packages;

namespace App.Rendering
{
    public interface ITableFilter
    {
        public bool IsPackageExcluded(IPackage package);
    }

    public class NoopTableFilter : ITableFilter
    {
        public bool IsPackageExcluded(IPackage package)
        {
            return false;
        }
    }

    public class HasMetricValuesTableFilter : ITableFilter
    {
        private readonly ISet<Guid> _packageIDsWithMetrics;

        public HasMetricValuesTableFilter(ProjectInspectionResult project)
        {
            _packageIDsWithMetrics = project.PackageResults
                .Where(pr => pr.Metrics.Any(metric => metric != null))
                .Select(pr => pr.Package.ID)
                .ToHashSet();
        }

        public bool IsPackageExcluded(IPackage package)
        {
            if (_packageIDsWithMetrics.Contains(package.ID))
            {
                return false;
            }

            if (package.Children is null || package.Children.Count == 0)
            {
                return true;
            }

            return package.Children.All(IsPackageExcluded);
        }
    }
}
