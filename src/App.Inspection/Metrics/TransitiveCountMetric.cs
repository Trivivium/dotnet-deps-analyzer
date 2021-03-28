using App.Inspection.Extensions;
using App.Inspection.Packages;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal class TransitiveCountMetric : IMetric
    {
        public IMetricResult? Compute(Project project, Compilation compilation, IPackageWithExecutableLoaded package, Registry registry)
        {
            var count = package.GetUniqueDependenciesCount();

            if (count == 0)
            {
                return null;
            }
            
            return new TransitiveCountMetricResult(count);
        }
    }
}
