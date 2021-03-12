using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal interface IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Package package, Registry registry);
    }
}
