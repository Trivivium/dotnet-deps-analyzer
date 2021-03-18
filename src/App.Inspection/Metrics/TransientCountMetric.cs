using Microsoft.CodeAnalysis;

using App.Inspection.Packages;

namespace App.Inspection.Metrics
{
    internal class TransientCountMetric : IMetric
    {
        public IMetricResult? Compute(Project project, Compilation compilation, PackageExecutableLoaded package, Registry registry)
        {
            return new TransientCountMetricResult(0);
        }
    }
}
