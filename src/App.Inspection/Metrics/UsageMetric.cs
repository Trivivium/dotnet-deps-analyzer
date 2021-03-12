using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal sealed class UsageMetric : IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Package package, Registry registry)
        {
            var percentage = (float) registry.GetUsedTypeCount(package) / package.ExportedTypesCount * 100;
            
            return new UsageMetricResult(percentage);
        }
    }
}
