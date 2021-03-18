using Microsoft.CodeAnalysis;

using App.Inspection.Packages;

namespace App.Inspection.Metrics
{
    internal class TransientCountMetric : IMetric
    {
        public IMetricResult? Compute(Project project, Compilation compilation, IPackageWithExecutableLoaded package, Registry registry)
        {
            return new TransientCountMetricResult(0);
        }

        // private static int Count(Package package)
        // {
        //     package.
        //     
        // }
    }
}
