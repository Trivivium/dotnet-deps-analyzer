using App.Inspection.Packages;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    /// <summary>
    /// Computes the percentage of used members of a package in a project.
    /// </summary>
    internal sealed class UsageMetric : IMetric
    {
        /// <inheritdoc />
        public IMetricResult? Compute(Project project, Compilation compilation, PackageExecutableLoaded package, Registry registry)
        {
            var usageCount = registry.GetUsedTypeCount(package);

            if (usageCount == 0)
            {
                return null;
            }
            
            var percentage = (float) usageCount / package.Executable.ExportedTypesCount * 100;
            
            return new UsageMetricResult(percentage);
        }
    }
}
