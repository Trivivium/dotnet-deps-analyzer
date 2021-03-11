using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection.Metrics
{
    internal interface IMetric
    {
        public IMetricResult Compute(Project project, Compilation compilation, Registry registry, Package package);
    }
}
