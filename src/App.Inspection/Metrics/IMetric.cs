using System.Collections.Generic;

namespace App.Inspection.Metrics
{
    internal interface IMetric
    {
        public IMetricResult Compute(Registry registry, Package package);
    }
}
