using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    /// <summary>
    /// Computes the percentage of used members of a package in a project.
    /// </summary>
    internal sealed class UsageMetric : IMetric
    {
        /// <inheritdoc />
        public IMetricResult Compute(Project project, Compilation compilation, Package package, Registry registry)
        {
            var percentage = (float) registry.GetUsedTypeCount(package) / package.ExportedTypesCount * 100;
            
            return new UsageMetricResult(percentage);
        }
    }
}
