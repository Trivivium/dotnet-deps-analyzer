namespace App.Inspection.Metrics
{
    internal sealed class ScatteringMetric : IMetric
    {
        public IMetricResult Compute(Registry registry, Package package)
        {
            // TODO: Implement computation logic of the scattering metric.
            
            return new ScatteringMetricResult();
        }
    }
}
