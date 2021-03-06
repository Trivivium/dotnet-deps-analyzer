using System.Linq;

using Microsoft.CodeAnalysis;

using App.Inspection.Packages;

namespace App.Inspection.Metrics
{
    /// <summary>
    /// Computes the scattering of used package types in the source-files of a project.
    /// </summary>
    internal sealed class ScatteringMetric : IMetric
    {
        /// <inheritdoc />
        public IMetricResult? Compute(Project project, Compilation compilation, IPackageWithExecutableLoaded package, Registry registry)
        {
            var uniqueLocationsIds = registry.GetReferenceLocationsAcrossSymbols(package)
                .Select(r => r.Document.Id)
                .ToList();

            if (uniqueLocationsIds.Count < 1)
            {
                return null;
            }

            var useCount = project.DocumentIds.Intersect(uniqueLocationsIds).Count();
            var totalCount = project.DocumentIds.Count;

            var percentage = (float) useCount / totalCount * 100;
            
            return new ScatteringMetricResult(percentage);
        }
    }
}
