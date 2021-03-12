using System.Linq;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    /// <summary>
    /// Computes the scattering of used package types in the source-files of a project.
    /// </summary>
    internal sealed class ScatteringMetric : IMetric
    {
        /// <inheritdoc />
        public IMetricResult Compute(Project project, Compilation compilation, Package package, Registry registry)
        {
            var uniqueLocationsIds = registry.GetReferenceLocationsAcrossSymbols(package)
                .Select(r => r.Document.Id)
                .ToList();

            var useCount = project.DocumentIds.Intersect(uniqueLocationsIds).Count();
            var totalCount = project.DocumentIds.Count;

            var percentage = (float) useCount / totalCount * 100;
            
            return new ScatteringMetricResult(percentage);
        }
    }
}
