using App.Inspection.Extensions;
using App.Inspection.Packages;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal class TransitiveCountMetric : IMetric
    {
        public IMetricResult? Compute(Project project, Compilation compilation, IPackageWithExecutableLoaded package, Registry registry)
        {
            return new TransitiveCountMetricResult(package.GetUniqueDependenciesCount());
        }
    }
}
